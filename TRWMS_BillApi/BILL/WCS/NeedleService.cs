using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;

namespace TRXWMS.BILL.WCS
{
    /// <summary>
    /// 单针库存/履历读写（wms_needle_inventory / wms_needle_transaction）。
    /// 设计约定：
    ///  1. 所有方法必须在调用方已开启的 SqlSugar 事务内执行（共用 SugarContext 单例客户端），本服务不自开事务，
    ///     保证与出入库单据写入同提交/同回滚。
    ///  2. 兼容历史盒：改造上线前已入库、没有针清单数据的盒，各流转方法命中 0 行即静默跳过。
    ///  3. 状态机：INBOUND_PENDING→AVAILABLE→RESERVED→OUTBOUND；
    ///     INBOUND_PENDING→BIND_CANCELLED（换绑/入库取消）；RESERVED→AVAILABLE（出库取消释放）。
    ///  4. 履历表采用 FROM 镜像 + INSERT..SELECT（FROMSTATUS 直接取变更前的 STATUS 列），
    ///     务必先写履历再更新当前表。
    /// </summary>
    public static class NeedleService
    {
        public const string ST_PENDING = "INBOUND_PENDING";
        public const string ST_AVAILABLE = "AVAILABLE";
        public const string ST_RESERVED = "RESERVED";
        public const string ST_OUTBOUND = "OUTBOUND";
        public const string ST_BIND_CANCELLED = "BIND_CANCELLED";

        #region 参数解析

        /// <summary>从请求 Needles 数组中过滤出属于指定盒的针清单（条目不带容器字段时视为属于该盒）。</summary>
        public static JArray FilterByContainer(JArray all, string container)
        {
            if (all == null || all.Count == 0) return null;
            JArray outp = new JArray();
            foreach (JToken jt in all)
            {
                string owner = GetStr(jt, "Container", "container", "Barcode", "barcode");
                if (string.IsNullOrEmpty(owner) || string.Equals(owner.Trim(), (container ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    outp.Add(jt);
                }
            }
            return outp.Count > 0 ? outp : null;
        }

        /// <summary>调用方未传针明细时自动生成的临时针号数量（默认盒容量）。</summary>
        public const int TEMP_NEEDLE_COUNT = 50;

        /// <summary>
        /// 生成临时针清单：T-{8位随机字符}-{位置号}，位置号 1..count，同批共用同一随机串便于识别来源批次。
        /// 用途：上游设备尚未上报真实针号明细时先行占位，保证针追踪链路完整；
        /// 后续真实明细到达时由 BindNeedles 换绑逻辑自动将临时针置为 BIND_CANCELLED。
        /// </summary>
        public static JArray GenerateTempNeedles(int count)
        {
            if (count <= 0) count = TEMP_NEEDLE_COUNT;
            string rand = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            JArray arr = new JArray();
            for (int i = 1; i <= count; i++)
            {
                JObject o = new JObject();
                o["No"] = "T-" + rand + "-" + i.ToString();
                o["Pos"] = i;
                arr.Add(o);
            }
            return arr;
        }

        /// <summary>
        /// 解析逗号分隔的槽位针号串（备刀机回流等接口格式）："1001,2001,,,,9002"
        /// 数组下标即盒内槽位号（第1个值=槽位1），值为该槽位的真实针号；
        /// 空槽位（连续逗号）跳过不建针。输出格式与 BindNeedles 入参一致（No/Pos）。
        /// </summary>
        public static JArray ParseNeedleString(string needles)
        {
            if (string.IsNullOrEmpty(needles) || needles.Trim().Length == 0) return null;
            string[] slots = needles.Split(',');
            JArray arr = new JArray();
            for (int i = 0; i < slots.Length; i++)
            {
                string no = slots[i] == null ? string.Empty : slots[i].Trim();
                if (no.Length == 0) continue; // 空槽位跳过
                JObject o = new JObject();
                o["No"] = no;
                o["Pos"] = i + 1; // 下标=槽位号（从1起）
                arr.Add(o);
            }
            return arr.Count > 0 ? arr : null;
        }

        private static string GetStr(JToken jt, params string[] keys)
        {
            foreach (string k in keys)
            {
                JToken v = jt[k];
                if (v != null && v.Type != JTokenType.Null)
                {
                    string s = v.ToString().Trim();
                    if (!string.IsNullOrEmpty(s)) return s;
                }
            }
            return null;
        }

        private static object DbNullIfNull(object v)
        {
            return v == null ? (object)DBNull.Value : v;
        }

        #endregion

        #region 入库绑定（扫码建入库单）

        /// <summary>
        /// 绑定/换绑一个盒的针清单：同针号复用原行重置（支持针循环使用），新针号插入；
        /// 本盒中原为 INBOUND_PENDING 但不在新清单内的针 → BIND_CANCELLED（换绑移出）。
        /// 针数据不合法（缺针号/非法位置/请求内重复）抛异常，由调用方整体回滚。
        /// </summary>
        public static int BindNeedles(SqlSugarClient db, string container, string itemcode,
                                      string inboundSource, JArray needles, string creator)
        {
            if (needles == null || needles.Count == 0) return 0;
            if (string.IsNullOrEmpty(container)) throw new Exception("单针绑定失败：盒号不能为空!");

            int bound = 0;
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<SugarParameter> kept = new List<SugarParameter>();
            List<string> keptNames = new List<string>();

            foreach (JToken jt in needles)
            {
                string no = GetStr(jt, "NeedleNo", "needleNo", "No", "no");
                if (string.IsNullOrEmpty(no)) throw new Exception("单针清单缺少针号(No)!");
                no = no.Trim();
                if (!seen.Add(no)) throw new Exception("单针清单存在重复针号:" + no + "!");

                int pos;
                string posStr = GetStr(jt, "Position", "position", "Pos", "pos", "PositionCode", "positionCode");
                if (!int.TryParse(posStr, out pos) || pos < 1)
                    throw new Exception("针号" + no + "的盒内位置(Position)无效!");

                DataTable old = db.Ado.GetDataTable(
                    "SELECT ID, STATUS, ITEMCODE, CONTAINER, POSITIONCODE FROM wms_needle_inventory WHERE NEEDLENO=@no",
                    new List<SugarParameter>() { new SugarParameter("@no", no) });

                if (old.Rows.Count > 0)
                {
                    // 复用原行（针循环使用/换盒重绑）
                    string fromStatus = old.Rows[0]["STATUS"].ToString();
                    string fromItem = old.Rows[0]["ITEMCODE"].ToString();
                    string fromCtn = old.Rows[0]["CONTAINER"].ToString();
                    string fromPos = old.Rows[0]["POSITIONCODE"].ToString();

                    int u = db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory
   SET ITEMCODE=@itemcode, CONTAINER=@ctn, POSITIONCODE=@pos, STATUS='INBOUND_PENDING',
       INBOUNDSOURCE=@src, INBOUNDORDER=@ctn,
       RESERVEORDER=NULL, OUTBOUNDORDER=NULL, OUTMACHINENO=NULL, OUTTARGET=NULL,
       ENABLED=1, LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE NEEDLENO=@no",
                        new List<SugarParameter>() {
                            new SugarParameter("@itemcode", itemcode),
                            new SugarParameter("@ctn", container),
                            new SugarParameter("@pos", pos),
                            new SugarParameter("@src", inboundSource),
                            new SugarParameter("@cr", creator),
                            new SugarParameter("@no", no)
                        });
                    if (u == 0) throw new Exception("针号" + no + "绑定更新失败!");

                    WriteTxnSingle(db, no, "BIND", container, fromStatus, ST_PENDING,
                        fromItem, itemcode, fromCtn, container, fromPos, pos.ToString(),
                        inboundSource, null, null, "重绑", creator);
                }
                else
                {
                    int i = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_inventory
      (ID, NEEDLENO, ITEMCODE, CONTAINER, POSITIONCODE, STATUS, INBOUNDSOURCE, INBOUNDORDER,
       ENABLED, CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME)
VALUES(UUID(), @no, @itemcode, @ctn, @pos, 'INBOUND_PENDING', @src, @ctn,
       1, @cr, sysdate(), @cr, sysdate())",
                        new List<SugarParameter>() {
                            new SugarParameter("@no", no),
                            new SugarParameter("@itemcode", itemcode),
                            new SugarParameter("@ctn", container),
                            new SugarParameter("@pos", pos),
                            new SugarParameter("@src", inboundSource),
                            new SugarParameter("@cr", creator)
                        });
                    if (i == 0) throw new Exception("针号" + no + "入库绑定失败（唯一键冲突或数据错误）!");

                    WriteTxnSingle(db, no, "BIND", container, null, ST_PENDING,
                        null, itemcode, null, container, null, pos.ToString(),
                        inboundSource, null, null, "新绑", creator);
                }

                kept.Add(new SugarParameter("@k" + bound, no));
                keptNames.Add("@k" + bound);
                bound++;
            }

            // 换绑清理：本盒旧 PENDING 且不在新清单内的针 → BIND_CANCELLED
            if (bound > 0)
            {
                string notIn = string.Join(",", keptNames.ToArray());
                int cancelled = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT UUID(), ID, NEEDLENO, 'BIND_CANCEL', @ctn, STATUS, 'BIND_CANCELLED',
       ITEMCODE, ITEMCODE, CONTAINER, CONTAINER, POSITIONCODE, POSITIONCODE,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, '换绑移出', @cr, sysdate()
  FROM wms_needle_inventory
 WHERE CONTAINER=@ctn AND STATUS='INBOUND_PENDING' AND NEEDLENO NOT IN (" + notIn + ")",
                    BuildParams(container, creator, kept));
                if (cancelled > 0)
                {
                    db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory
   SET STATUS='BIND_CANCELLED', LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS='INBOUND_PENDING' AND NEEDLENO NOT IN (" + notIn + ")",
                        BuildParams(container, creator, kept));
                }
            }
            return bound;
        }

        private static List<SugarParameter> BuildParams(string container, string creator, List<SugarParameter> kept)
        {
            List<SugarParameter> ps = new List<SugarParameter>() {
                new SugarParameter("@ctn", container),
                new SugarParameter("@cr", creator)
            };
            ps.AddRange(kept);
            return ps;
        }

        #endregion

        #region 状态流转（履历先行，再更新当前表）

        /// <summary>入库完成：本盒 INBOUND_PENDING → AVAILABLE。返回更新针数（历史盒为 0 属正常）。</summary>
        public static int MarkInboundComplete(SqlSugarClient db, string container, string creator)
        {
            int n = BulkTxn(db, container, "INBOUND", container, ST_PENDING, ST_AVAILABLE, null, creator);
            if (n > 0)
            {
                db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory SET STATUS='AVAILABLE', LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS='INBOUND_PENDING'",
                    new List<SugarParameter>() {
                        new SugarParameter("@ctn", container),
                        new SugarParameter("@cr", creator)
                    });
            }
            return n;
        }

        /// <summary>
        /// 出库建单预占：本盒 AVAILABLE → RESERVED，记录预占单据、机台号、出库口。
        /// 防重复分配：盒内有针数据但已无 AVAILABLE 针时，视为已被其它单据预占或状态异常，
        /// 抛异常由调用方事务整体回滚；历史盒（无针数据）返回 0 静默跳过。
        /// machineno / outtarget 可为 null（对应列不动）。
        /// </summary>
        public static int ReserveContainer(SqlSugarClient db, string container, string reserveOrder, string machineno, string outtarget, string creator)
        {
            object mach = null;
            int m;
            if (!string.IsNullOrEmpty(machineno) && int.TryParse(machineno.Trim(), out m)) mach = m;

            // ===== 防重复分配校验：有针数据但零可用 → 拒绝预占 =====
            object totalObj = db.Ado.GetScalar(
                "SELECT COUNT(*) FROM wms_needle_inventory WHERE CONTAINER=@ctn AND ENABLED=1",
                new List<SugarParameter>() { new SugarParameter("@ctn", container) });
            int total = totalObj == null ? 0 : Convert.ToInt32(totalObj);
            if (total > 0)
            {
                object availObj = db.Ado.GetScalar(
                    "SELECT COUNT(*) FROM wms_needle_inventory WHERE CONTAINER=@ctn AND ENABLED=1 AND STATUS='AVAILABLE'",
                    new List<SugarParameter>() { new SugarParameter("@ctn", container) });
                int avail = availObj == null ? 0 : Convert.ToInt32(availObj);
                if (avail <= 0)
                {
                    throw new Exception("盒" + container + "无可用针（可能已被其它单据预占），预占失败!");
                }
            }

            int n = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT UUID(), ID, NEEDLENO, 'RESERVE', @ro, STATUS, 'RESERVED',
       ITEMCODE, ITEMCODE, CONTAINER, CONTAINER, POSITIONCODE, POSITIONCODE,
       INBOUNDSOURCE, @mach, @ot, '', @cr, sysdate()
  FROM wms_needle_inventory
 WHERE CONTAINER=@ctn AND STATUS='AVAILABLE'",
                new List<SugarParameter>() {
                    new SugarParameter("@ro", reserveOrder),
                    new SugarParameter("@mach", DbNullIfNull(mach)),
                    new SugarParameter("@ot", DbNullIfNull(outtarget)),
                    new SugarParameter("@ctn", container),
                    new SugarParameter("@cr", creator)
                });
            if (n > 0)
            {
                db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory
   SET STATUS='RESERVED', RESERVEORDER=@ro,
       OUTMACHINENO=COALESCE(@mach, OUTMACHINENO), OUTTARGET=COALESCE(@ot, OUTTARGET),
       LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS='AVAILABLE'",
                    new List<SugarParameter>() {
                        new SugarParameter("@ro", reserveOrder),
                        new SugarParameter("@mach", DbNullIfNull(mach)),
                        new SugarParameter("@ot", DbNullIfNull(outtarget)),
                        new SugarParameter("@ctn", container),
                        new SugarParameter("@cr", creator)
                    });
            }
            return n;
        }

        /// <summary>
        /// 出库取消：本盒 RESERVED → AVAILABLE，清空预占单号。
        /// sourceOrder=被取消的单据号（写入履历 SOURCEORDER 便于追溯；取消记录保留在履历表，不删除历史）。
        /// </summary>
        public static int UnreserveContainer(SqlSugarClient db, string container, string sourceOrder, string creator)
        {
            int n = BulkTxn(db, container, "UNRESERVE", sourceOrder, ST_RESERVED, ST_AVAILABLE, null, creator);
            if (n > 0)
            {
                db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory SET STATUS='AVAILABLE', RESERVEORDER=NULL, LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS='RESERVED'",
                    new List<SugarParameter>() {
                        new SugarParameter("@ctn", container),
                        new SugarParameter("@cr", creator)
                    });
            }
            return n;
        }

        /// <summary>入库绑定取消（入库任务被清除）：本盒 INBOUND_PENDING → BIND_CANCELLED。</summary>
        public static int MarkBindCancelled(SqlSugarClient db, string container, string creator)
        {
            int n = BulkTxn(db, container, "BIND_CANCEL", container, ST_PENDING, ST_BIND_CANCELLED, null, creator);
            if (n > 0)
            {
                db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory SET STATUS='BIND_CANCELLED', LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS='INBOUND_PENDING'",
                    new List<SugarParameter>() {
                        new SugarParameter("@ctn", container),
                        new SugarParameter("@cr", creator)
                    });
            }
            return n;
        }

        /// <summary>
        /// 出库完成：先核验本盒 RESERVED 针的预占单据与本出库单一致（防串单/错盒），
        /// 再整盒 RESERVED（及未经预占旁路的 AVAILABLE）→ OUTBOUND，记录出库单与去向口。
        /// 核验失败抛异常，由调用方事务整体回滚。
        /// </summary>
        public static int MarkOutboundComplete(SqlSugarClient db, string container, string outboundOrder, string outtarget, string creator)
        {
            // ===== 预占一致性核验：RESERVED 针的预占单号必须属于本出库单（单据ID / 其 ORDERID / outboundOrder） =====
            List<string> allowed = new List<string>();
            if (!string.IsNullOrEmpty(outboundOrder)) allowed.Add(outboundOrder);
            DataTable hdr = db.Ado.GetDataTable(
                "SELECT ID, ORDERID FROM wms_bill_putoutheader WHERE SOURCEORDER=@so AND CONTAINER=@ctn ORDER BY CREATETIME DESC LIMIT 1",
                new List<SugarParameter>() {
                    new SugarParameter("@so", outboundOrder),
                    new SugarParameter("@ctn", container)
                });
            if (hdr != null && hdr.Rows.Count > 0)
            {
                allowed.Add(hdr.Rows[0]["ID"].ToString());
                string oid = hdr.Rows[0]["ORDERID"] == DBNull.Value ? string.Empty : hdr.Rows[0]["ORDERID"].ToString();
                if (!string.IsNullOrEmpty(oid)) allowed.Add(oid);
            }
            DataTable reservedOrders = db.Ado.GetDataTable(
                "SELECT DISTINCT RESERVEORDER FROM wms_needle_inventory WHERE CONTAINER=@ctn AND STATUS='RESERVED' AND ENABLED=1",
                new List<SugarParameter>() { new SugarParameter("@ctn", container) });
            if (reservedOrders != null)
            {
                foreach (DataRow r in reservedOrders.Rows)
                {
                    string ro = r["RESERVEORDER"] == DBNull.Value ? string.Empty : r["RESERVEORDER"].ToString();
                    if (!string.IsNullOrEmpty(ro) && !allowed.Contains(ro))
                    {
                        throw new Exception("出库完成核验失败：盒" + container + "预占单据(" + ro + ")与本出库单不一致，疑似串单/错盒，已阻止出库!");
                    }
                }
            }

            // AVAILABLE 一并翻转：兼容未经预占的旁路出库（如 SAP settaskoutbound 链路）；历史盒命中 0 行静默跳过
            int n = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT UUID(), ID, NEEDLENO, 'OUTBOUND', @so, STATUS, 'OUTBOUND',
       ITEMCODE, ITEMCODE, CONTAINER, CONTAINER, POSITIONCODE, POSITIONCODE,
       INBOUNDSOURCE, OUTMACHINENO, @ot, '', @cr, sysdate()
  FROM wms_needle_inventory
 WHERE CONTAINER=@ctn AND STATUS IN ('RESERVED','AVAILABLE')",
                new List<SugarParameter>() {
                    new SugarParameter("@so", outboundOrder),
                    new SugarParameter("@ot", outtarget),
                    new SugarParameter("@ctn", container),
                    new SugarParameter("@cr", creator)
                });
            if (n > 0)
            {
                db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory
   SET STATUS='OUTBOUND', OUTBOUNDORDER=@so, OUTTARGET=@ot, LASTEDITOR=@cr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND STATUS IN ('RESERVED','AVAILABLE')",
                    new List<SugarParameter>() {
                        new SugarParameter("@so", outboundOrder),
                        new SugarParameter("@ot", outtarget),
                        new SugarParameter("@ctn", container),
                        new SugarParameter("@cr", creator)
                    });
            }
            return n;
        }

        #endregion

        #region 库存统计

        /// <summary>
        /// 全仓按盒统计 AVAILABLE 针数。无针数据的历史盒不在结果中，调用方按默认容量（TEMP_NEEDLE_COUNT）折算。
        /// 用途：按针数叫料时的库存计数与选盒累计。
        /// </summary>
        public static Dictionary<string, int> GetAvailableCountMap(SqlSugarClient db)
        {
            DataTable dt = db.Ado.GetDataTable(
                "SELECT CONTAINER, COUNT(*) AS CNT FROM wms_needle_inventory WHERE STATUS='AVAILABLE' AND ENABLED=1 GROUP BY CONTAINER");
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in dt.Rows)
            {
                map[r["CONTAINER"].ToString()] = Convert.ToInt32(r["CNT"]);
            }
            return map;
        }

        #endregion

        #region 履历写入内部实现

        /// <summary>整盒批量流转的履历（FROMSTATUS 直接取变更前 STATUS 列，须在更新当前表之前调用）。</summary>
        private static int BulkTxn(SqlSugarClient db, string container, string biztype, string sourceOrder,
                                   string fromStatusFilter, string toStatus, string outtarget, string creator)
        {
            return db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT UUID(), ID, NEEDLENO, @biz, @so, STATUS, @to,
       ITEMCODE, ITEMCODE, CONTAINER, CONTAINER, POSITIONCODE, POSITIONCODE,
       INBOUNDSOURCE, OUTMACHINENO, @ot, '', @cr, sysdate()
  FROM wms_needle_inventory
 WHERE CONTAINER=@ctn AND STATUS=@fs",
                new List<SugarParameter>() {
                    new SugarParameter("@biz", biztype),
                    new SugarParameter("@so", sourceOrder),
                    new SugarParameter("@to", toStatus),
                    new SugarParameter("@ot", DbNullIfNull(outtarget)),
                    new SugarParameter("@cr", creator),
                    new SugarParameter("@ctn", container),
                    new SugarParameter("@fs", fromStatusFilter)
                });
        }

        /// <summary>单针履历（绑定/重绑逐针写入）。</summary>
        private static void WriteTxnSingle(SqlSugarClient db, string needleNo, string biztype, string sourceOrder,
                                           string fromStatus, string toStatus,
                                           string fromItem, string toItem,
                                           string fromContainer, string toContainer,
                                           string fromPosition, string toPosition,
                                           string inboundSource, object machineno, string outtarget,
                                           string remark, string creator)
        {
            db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
SELECT UUID(), ID, @no, @biz, @so, @fs, @to,
       @fi, @ti, @fc, @tc, @fp, @tp,
       @inb, @mach, @ot, @rm, @cr, sysdate()
  FROM wms_needle_inventory WHERE NEEDLENO=@no",
                new List<SugarParameter>() {
                    new SugarParameter("@no", needleNo),
                    new SugarParameter("@biz", biztype),
                    new SugarParameter("@so", sourceOrder),
                    new SugarParameter("@fs", DbNullIfNull(fromStatus)),
                    new SugarParameter("@to", toStatus),
                    new SugarParameter("@fi", DbNullIfNull(fromItem)),
                    new SugarParameter("@ti", DbNullIfNull(toItem)),
                    new SugarParameter("@fc", DbNullIfNull(fromContainer)),
                    new SugarParameter("@tc", DbNullIfNull(toContainer)),
                    new SugarParameter("@fp", DbNullIfNull(fromPosition)),
                    new SugarParameter("@tp", DbNullIfNull(toPosition)),
                    new SugarParameter("@inb", DbNullIfNull(inboundSource)),
                    new SugarParameter("@mach", DbNullIfNull(machineno)),
                    new SugarParameter("@ot", DbNullIfNull(outtarget)),
                    new SugarParameter("@rm", DbNullIfNull(remark)),
                    new SugarParameter("@cr", creator)
                });
        }

        #endregion
    }
}
