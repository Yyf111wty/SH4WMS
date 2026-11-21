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
    ///  4. 批量状态流转采用 FROM 镜像 + INSERT..SELECT；绑定使用已锁定旧行的镜像批量写履历，
    ///     旧针务必先写履历再更新当前表；新针先建库存行再写履历。
    /// </summary>
    public static class NeedleService
    {
        public const string ST_PENDING = "INBOUND_PENDING";
        public const string ST_AVAILABLE = "AVAILABLE";
        public const string ST_RESERVED = "RESERVED";
        public const string ST_OUTBOUND = "OUTBOUND";
        public const string ST_BIND_CANCELLED = "BIND_CANCELLED";
        public const string ST_QUARANTINED = "QUARANTINED"; // A02：隔离针禁止重绑

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

        /// <summary>
        /// A01 多盒归属强校验版过滤（替代 FilterByContainer 的宽松默认归属）：
        ///  - 单盒请求（boxes.Length==1）：条目归属字段可省略（默认该盒）；带了归属则必须等于该盒号；
        ///  - 多盒请求：每条针**必须**带归属字段（Container/Barcode），缺失或不在本次盒号列表内 → 抛异常，
        ///    由调用方事务整体回滚（拒绝"无归属默认属于所有盒"造成的针被后盒抢绑）。
        /// 归属比对忽略大小写与首尾空白。返回 null 表示该盒无针（调用方按缺省逻辑处理）。
        /// </summary>
        public static JArray FilterByContainerStrict(JArray all, string container, string[] boxes)
        {
            if (all == null || all.Count == 0) return null;
            bool multi = boxes != null && boxes.Length > 1;
            HashSet<string> boxSet = null;
            if (boxes != null && boxes.Length > 0)
            {
                boxSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string b in boxes)
                {
                    if (!string.IsNullOrEmpty(b)) boxSet.Add(b.Trim());
                }
            }
            JArray outp = new JArray();
            for (int i = 0; i < all.Count; i++)
            {
                JToken jt = all[i];
                string owner = GetStr(jt, "Container", "container", "Barcode", "barcode");
                if (string.IsNullOrEmpty(owner))
                {
                    if (multi)
                    {
                        string no = GetStr(jt, "NeedleNo", "needleNo", "No", "no");
                        throw new Exception("多盒入库时每条针必须带归属盒号(Container)，第" + (i + 1) + "条"
                            + (string.IsNullOrEmpty(no) ? "" : "（针号" + no + "）") + "缺失归属!");
                    }
                    outp.Add(jt); // 单盒：省略归属默认本盒
                    continue;
                }
                owner = owner.Trim();
                if (!string.Equals(owner, (container ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // 归属其它盒：多盒时必须是本次请求内的盒（否则=错误归属拒绝）；单盒时直接视为不相关条目
                    if (multi && (boxSet == null || !boxSet.Contains(owner)))
                    {
                        throw new Exception("针清单第" + (i + 1) + "条归属盒号(" + owner + ")不在本次入库请求内，归属错误!");
                    }
                    continue;
                }
                outp.Add(jt);
            }
            return outp.Count > 0 ? outp : null;
        }

        /// <summary>调用方未传针明细时自动生成的临时针号数量（默认盒容量）。</summary>
        public const int TEMP_NEEDLE_COUNT = 50;

        /// <summary>A01 校验：针号最大长度（与表列 wms_needle_inventory.NEEDLENO varchar(100) 对齐）。</summary>
        public const int NEEDLENO_MAX_LEN = 100;

        /// <summary>A01 校验：盒内槽位号上限（物理盒 50 槽，与 TEMP_NEEDLE_COUNT 同口径）。</summary>
        public const int POSITION_MAX = TEMP_NEEDLE_COUNT;

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
        /// A01 校验：针号非空且≤100字符、忽略大小写不重复；槽位 1~50 且同盒不重复——
        /// 不合法抛异常，由调用方整体回滚（多盒归属校验在 FilterByContainerStrict）。
        /// A02 状态闸门：针号已存在时先核验原状态/所在盒——RESERVED（他单预占）、QUARANTINED、ENABLED=0 一律拒绝；
        /// 跨盒仅允许 OUTBOUND（返库）/BIND_CANCELLED/ADJUSTED_OUT 等已结束绑定的针复用；
        /// 其余状态即使原盒没有库存台账，也提示原盒号并拒绝重复绑定。同盒沿用既有状态校验。
        /// 拒绝均抛异常整笔回滚，不动原行不清预占；每盒批量查询并锁定旧针，全部校验通过后批量写入，共 4~6 次数据库调用。
        /// </summary>
        public static int BindNeedles(SqlSugarClient db, string container, string itemcode, string inboundHeaderId,
                                      string inboundSource, JArray needles, string creator)
        {
            if (needles == null || needles.Count == 0) return 0;
            if (string.IsNullOrEmpty(container)) throw new Exception("单针绑定失败：盒号不能为空!");
            if (string.IsNullOrWhiteSpace(inboundHeaderId)) throw new Exception("单针绑定失败：入库单头ID不能为空!");

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<int> seenPos = new HashSet<int>(); // A01：同盒槽位查重
            List<SugarParameter> kept = new List<SugarParameter>();
            List<string> keptNames = new List<string>();
            List<BindNeedle> batch = new List<BindNeedle>();
            List<string> requested = new List<string>();

            // 整盒参数先校验，最多 50 个不同槽位；校验通过前不读写数据库。
            foreach (JToken jt in needles)
            {
                string no = GetStr(jt, "NeedleNo", "needleNo", "No", "no");
                if (string.IsNullOrEmpty(no)) throw new Exception("单针清单缺少针号(No)!");
                no = no.Trim();
                if (no.Length > NEEDLENO_MAX_LEN) throw new Exception("针号长度超过" + NEEDLENO_MAX_LEN + "字符:" + no.Substring(0, 20) + "...!"); // A01：针号长度校验
                if (!seen.Add(no)) throw new Exception("单针清单存在重复针号:" + no + "!");

                int pos;
                string posStr = GetStr(jt, "Position", "position", "Pos", "pos", "PositionCode", "positionCode");
                if (!int.TryParse(posStr, out pos) || pos < 1 || pos > POSITION_MAX)
                    throw new Exception("针号" + no + "的盒内位置(Position)无效，必须在1~" + POSITION_MAX + "之间!"); // A01：槽位上下限校验
                if (!seenPos.Add(pos))
                    throw new Exception("单针清单盒内槽位重复:Position=" + pos + "（针号" + no + "）!"); // A01：同盒槽位不重复

                int index = batch.Count;
                string parameter = "@k" + index;
                kept.Add(new SugarParameter(parameter, no));
                keptNames.Add(parameter);
                requested.Add("SELECT " + index + " AS BINDINDEX, " + parameter + " AS NEEDLENO");
                batch.Add(new BindNeedle { No = no, Position = pos });
            }

            // 在数据库中按针号匹配并返回输入序号，保持数据库排序规则（包括大小写/重音）的一致性。
            // 锁住已有针行，防止读取旧状态后被其他事务预占或重绑；事务仍由调用方管理。
            DataTable old = db.Ado.GetDataTable(@"
SELECT b.BINDINDEX, i.ID, i.STATUS, i.ITEMCODE, i.CONTAINER, i.POSITIONCODE, i.ENABLED, i.RESERVEORDER
  FROM (" + string.Join(" UNION ALL ", requested.ToArray()) + @") b
  JOIN wms_needle_inventory i ON i.NEEDLENO=b.NEEDLENO
 ORDER BY i.ID FOR UPDATE", kept);
            HashSet<string> matchedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in old.Rows)
            {
                BindNeedle needle = batch[Convert.ToInt32(row["BINDINDEX"])];
                string id = row["ID"].ToString();
                if (!matchedIds.Add(id)) throw new Exception("单针清单存在重复针号:" + needle.No + "!");
                needle.Id = id;
                needle.Original = row;
            }

            List<BindNeedle> inserts = new List<BindNeedle>();
            List<BindNeedle> updates = new List<BindNeedle>();
            foreach (BindNeedle needle in batch)
            {
                string no = needle.No;
                DataRow original = needle.Original;
                if (original != null)
                {
                    // 复用原行（针循环使用/换盒重绑）
                    string fromStatus = original["STATUS"].ToString();
                    string fromCtn = original["CONTAINER"].ToString();

                    // ===== A02 状态闸门：绑定前核验针原状态/所在盒/关联单据，拒绝抛异常由调用方整笔回滚（不动原行、不写履历、不清他单预占） =====
                    int fromEnabled = original["ENABLED"] == DBNull.Value ? 1 : Convert.ToInt32(original["ENABLED"]);
                    if (fromEnabled == 0)
                    {
                        throw new Exception("针号" + no + "已停用(ENABLED=0)，禁止重绑!");
                    }
                    if (fromStatus == ST_RESERVED)
                    {
                        string ro = original["RESERVEORDER"] == DBNull.Value ? "" : original["RESERVEORDER"].ToString();
                        throw new Exception("针号" + no + "已被单据(" + (string.IsNullOrEmpty(ro) ? "未知" : ro) + ")预占（原盒" + fromCtn + "），禁止重绑!");
                    }
                    if (fromStatus == ST_QUARANTINED)
                    {
                        throw new Exception("针号" + no + "处于隔离状态(QUARANTINED)（原盒" + fromCtn + "），禁止重绑!");
                    }
                    bool sameContainer = string.Equals(fromCtn.Trim(), container.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool bindingEnded = fromStatus == ST_OUTBOUND || fromStatus == ST_BIND_CANCELLED || fromStatus == "ADJUSTED_OUT";
                    if (!sameContainer && !bindingEnded)
                    {
                        // 绑定关系以针状态为准，不以原盒是否已有库存台账为准，禁止隐式跨盒转移。
                        throw new Exception("针号" + no + "已绑定到盒" + fromCtn + "（状态" + fromStatus
                            + "），禁止重复绑定到盒" + container + "!");
                    }
                    // 结束绑定的针可返库复用；仍有效的针只允许同盒更新。

                    updates.Add(needle);
                }
                else
                {
                    needle.Id = Guid.NewGuid().ToString("D");
                    inserts.Add(needle);
                }
            }

            // 所有状态闸门通过后再批量写入。旧针履历使用已锁定行的变更前镜像，并先于更新写入。
            InsertBindNeedles(db, inserts, container, itemcode, inboundHeaderId, inboundSource, creator);
            WriteBindTransactions(db, batch, container, itemcode, inboundSource, creator);
            UpdateBindNeedles(db, updates, container, itemcode, inboundHeaderId, inboundSource, creator);

            // 换绑清理：本盒旧 PENDING 且不在新清单内的针 → BIND_CANCELLED
            if (batch.Count > 0)
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
            return batch.Count;
        }

        private sealed class BindNeedle
        {
            public string Id;
            public string No;
            public int Position;
            public DataRow Original;
        }

        private static List<SugarParameter> BuildBindParams(string container, string itemcode, string inboundSource, string creator)
        {
            return new List<SugarParameter>() {
                new SugarParameter("@ctn", container),
                new SugarParameter("@itemcode", itemcode),
                new SugarParameter("@src", DbNullIfNull(inboundSource)),
                new SugarParameter("@cr", DbNullIfNull(creator))
            };
        }

        private static void InsertBindNeedles(SqlSugarClient db, List<BindNeedle> needles, string container,
                                              string itemcode, string inboundHeaderId, string inboundSource, string creator)
        {
            if (needles.Count == 0) return;
            List<SugarParameter> ps = BuildBindParams(container, itemcode, inboundSource, creator);
            ps.Add(new SugarParameter("@inboundHeaderId", inboundHeaderId));
            List<string> values = new List<string>();
            for (int i = 0; i < needles.Count; i++)
            {
                ps.Add(new SugarParameter("@id" + i, needles[i].Id));
                ps.Add(new SugarParameter("@no" + i, needles[i].No));
                ps.Add(new SugarParameter("@pos" + i, needles[i].Position));
                values.Add("(@id" + i + ", @no" + i + ", @itemcode, @ctn, @pos" + i
                    + ", 'INBOUND_PENDING', @src, @inboundHeaderId, 1, @cr, sysdate(), @cr, sysdate())");
            }
            int inserted = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_inventory
      (ID, NEEDLENO, ITEMCODE, CONTAINER, POSITIONCODE, STATUS, INBOUNDSOURCE, INBOUNDORDER,
       ENABLED, CREATOR, CREATETIME, LASTEDITOR, LASTEDITTIME)
VALUES " + string.Join(",", values.ToArray()), ps);
            if (inserted != needles.Count) throw new Exception("单针批量入库绑定失败（写入数量与清单不一致）!");
        }

        private static void UpdateBindNeedles(SqlSugarClient db, List<BindNeedle> needles, string container,
                                              string itemcode, string inboundHeaderId, string inboundSource, string creator)
        {
            if (needles.Count == 0) return;
            List<SugarParameter> ps = BuildBindParams(container, itemcode, inboundSource, creator);
            ps.Add(new SugarParameter("@inboundHeaderId", inboundHeaderId));
            List<string> rows = new List<string>();
            for (int i = 0; i < needles.Count; i++)
            {
                ps.Add(new SugarParameter("@id" + i, needles[i].Id));
                ps.Add(new SugarParameter("@pos" + i, needles[i].Position));
                rows.Add("SELECT @id" + i + " AS ID, @pos" + i + " AS POSITIONCODE");
            }
            int updated = db.Ado.ExecuteCommand(@"
UPDATE wms_needle_inventory i
  JOIN (" + string.Join(" UNION ALL ", rows.ToArray()) + @") b ON i.ID=b.ID
   SET i.ITEMCODE=@itemcode, i.CONTAINER=@ctn, i.POSITIONCODE=b.POSITIONCODE, i.STATUS='INBOUND_PENDING',
       i.INBOUNDSOURCE=@src, i.INBOUNDORDER=@inboundHeaderId,
       i.RESERVEORDER=NULL, i.OUTBOUNDORDER=NULL, i.OUTMACHINENO=NULL, i.OUTTARGET=NULL,
       i.ENABLED=1, i.LASTEDITOR=@cr, i.LASTEDITTIME=sysdate()", ps);
            if (updated != needles.Count) throw new Exception("单针批量绑定更新失败（更新数量与清单不一致）!");
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
        /// 仅统计指定候选盒的 AVAILABLE 针数，盒号去重后分批查询；空列表不访问数据库。
        /// 无针数据的历史盒不在结果中，调用方按默认容量（TEMP_NEEDLE_COUNT）折算。
        /// 用途：按针数叫料时的库存计数与选盒累计。
        /// </summary>
        public static Dictionary<string, int> GetAvailableCountMap(SqlSugarClient db, IEnumerable<string> containers)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (containers == null) return map;

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> boxes = new List<string>();
            foreach (string container in containers)
            {
                if (!string.IsNullOrWhiteSpace(container) && seen.Add(container)) boxes.Add(container);
            }

            const int batchSize = 500;
            for (int offset = 0; offset < boxes.Count; offset += batchSize)
            {
                List<string> names = new List<string>();
                List<SugarParameter> parameters = new List<SugarParameter>();
                int count = Math.Min(batchSize, boxes.Count - offset);
                for (int i = 0; i < count; i++)
                {
                    string name = "@ctn" + i;
                    names.Add(name);
                    parameters.Add(new SugarParameter(name, boxes[offset + i]));
                }

                DataTable dt = db.Ado.GetDataTable(
                    "SELECT CONTAINER, COUNT(*) AS CNT FROM wms_needle_inventory"
                    + " WHERE CONTAINER IN (" + string.Join(",", names.ToArray()) + ")"
                    + " AND STATUS='AVAILABLE' AND ENABLED=1 GROUP BY CONTAINER", parameters);
                foreach (DataRow r in dt.Rows)
                {
                    map[r["CONTAINER"].ToString()] = Convert.ToInt32(r["CNT"]);
                }
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

        /// <summary>整盒绑定履历：新针使用已生成的 ID，旧针保留批量查询得到的变更前镜像。</summary>
        private static void WriteBindTransactions(SqlSugarClient db, List<BindNeedle> needles, string container,
                                                   string itemcode, string inboundSource, string creator)
        {
            List<SugarParameter> ps = BuildBindParams(container, itemcode, inboundSource, creator);
            List<string> values = new List<string>();
            for (int i = 0; i < needles.Count; i++)
            {
                BindNeedle needle = needles[i];
                DataRow original = needle.Original;
                ps.Add(new SugarParameter("@id" + i, needle.Id));
                ps.Add(new SugarParameter("@no" + i, needle.No));
                ps.Add(new SugarParameter("@pos" + i, needle.Position));
                ps.Add(new SugarParameter("@fs" + i, original == null ? DBNull.Value : original["STATUS"]));
                ps.Add(new SugarParameter("@fi" + i, original == null ? DBNull.Value : original["ITEMCODE"]));
                ps.Add(new SugarParameter("@fc" + i, original == null ? DBNull.Value : original["CONTAINER"]));
                ps.Add(new SugarParameter("@fp" + i, original == null ? DBNull.Value : original["POSITIONCODE"]));
                ps.Add(new SugarParameter("@rm" + i, original == null ? "新绑" : "重绑"));
                values.Add("(UUID(), @id" + i + ", @no" + i + ", 'BIND', @ctn, @fs" + i + ", 'INBOUND_PENDING', "
                    + "@fi" + i + ", @itemcode, @fc" + i + ", @ctn, @fp" + i + ", @pos" + i
                    + ", @src, NULL, NULL, @rm" + i + ", @cr, sysdate())");
            }
            int written = db.Ado.ExecuteCommand(@"
INSERT INTO wms_needle_transaction
      (ID, NEEDLEID, NEEDLENO, BIZTYPE, SOURCEORDER, FROMSTATUS, TOSTATUS,
       FROMITEMCODE, TOITEMCODE, FROMCONTAINER, TOCONTAINER, FROMPOSITION, TOPOSITION,
       INBOUNDSOURCE, OUTMACHINENO, OUTTARGET, REMARK, CREATOR, CREATETIME)
VALUES " + string.Join(",", values.ToArray()), ps);
            if (written != needles.Count) throw new Exception("单针批量绑定履历写入失败（写入数量与清单不一致）!");
        }

        #endregion
    }
}
