//-----------------------------------------------------------------------
// <copyright file="WmsNeedleAdjust.cs" company="xxxx Enterprises">
// * Copyright (C) 2026 xxxx Enterprises All Rights Reserved
// * FileName: WmsNeedleAdjust.cs
// * history : 单针改造·第八批（PC 清空库位/清空托盘物料 联动针调整出库）
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using SqlSugar;
using TR.ORM;

namespace TRXWMS_Website.Helper
{
    /// <summary>
    /// 单针改造（第八批）：PC 端清库位/清托盘物料删除台账时，同步把该盒未出库的针
    /// 置 ADJUSTED_OUT 并写 ADJUST_OUT 履历，杜绝"删了台账、针留库中"的假库存。
    /// 使用独立 SqlSugarClient + 手动 BeginTran 事务，不占用共享 SugarContext
    /// 单例的事务状态，避免污染站点其它并发请求。
    /// 盒无针数据（改造上线前的历史盒且未做初始化）时返回 0，静默跳过=旧行为。
    /// 失败抛异常，由调用方中断本次清空操作。
    /// </summary>
    public static class WmsNeedleAdjust
    {
        /// <summary>履历/备注口径：与受控 SQL 时代区分开</summary>
        public const string ADJUST_REMARK = "PC清空库位";

        private const string PENDING_STATUSES = "('INBOUND_PENDING','AVAILABLE','RESERVED')";

        /// <summary>
        /// 将指定盒号的未出库针整盒调整出库。
        /// </summary>
        /// <param name="container">盒号（库位表 CONTAINER）</param>
        /// <param name="sourceOrder">被清空的来源单据号（写入履历 SOURCEORDER，可空）</param>
        /// <param name="user">操作员（Session["username"]，可空，空则记 "PC"）</param>
        /// <returns>翻转的针数（0=该盒无针数据或已全部出库）</returns>
        public static int AdjustOutForBox(string container, string sourceOrder, string user)
        {
            if (string.IsNullOrWhiteSpace(container))
            {
                return 0;
            }

            string usr = string.IsNullOrWhiteSpace(user) ? "PC" : user.Trim();

            var ps = new List<SugarParameter>() {
                new SugarParameter("@ctn", container),
                new SugarParameter("@src", string.IsNullOrWhiteSpace(sourceOrder) ? null : sourceOrder.Trim()),
                new SugarParameter("@rmk", ADJUST_REMARK),
                new SugarParameter("@usr", usr)
            };

            string sqlTxn = @"
INSERT INTO wms_needle_transaction
 (ID,NEEDLEID,NEEDLENO,BIZTYPE,SOURCEORDER,FROMSTATUS,TOSTATUS,
  FROMITEMCODE,TOITEMCODE,FROMCONTAINER,FROMPOSITION,TOCONTAINER,TOPOSITION,
  INBOUNDSOURCE,OUTMACHINENO,OUTTARGET,REMARK,CREATOR,CREATETIME)
SELECT UUID(),i.ID,i.NEEDLENO,'ADJUST_OUT',@src,i.STATUS,'ADJUSTED_OUT',
  i.ITEMCODE,i.ITEMCODE,i.CONTAINER,i.POSITIONCODE,i.CONTAINER,i.POSITIONCODE,
  i.INBOUNDSOURCE,NULL,NULL,@rmk,@usr,sysdate()
  FROM wms_needle_inventory i
 WHERE i.CONTAINER=@ctn AND i.ENABLED=1 AND i.STATUS IN " + PENDING_STATUSES;

            string sqlUpd = @"
UPDATE wms_needle_inventory
   SET STATUS='ADJUSTED_OUT', LASTEDITOR=@usr, LASTEDITTIME=sysdate()
 WHERE CONTAINER=@ctn AND ENABLED=1 AND STATUS IN " + PENDING_STATUSES;

            return SqlSugarClientFactory.ExecuteWithoutRequestSynchronizationContext(() =>
            {
                using (SqlSugarClient db = CreateIndependentClient())
                {
                    int flipped = 0;
                    int txn = 0;

                    // 手动事务（与 BillApi NeedleService 同款 API，BeginTran/Commit/Rollback 在
                    // SqlSugar 5.1.4.x 均已验证可用；独立客户端自开自闭，不影响共享单例事务状态）
                    db.Ado.BeginTran();
                    try
                    {
                        txn = db.Ado.ExecuteCommand(sqlTxn, ps);
                        flipped = db.Ado.ExecuteCommand(sqlUpd, ps);
                        if (txn != flipped)
                        {
                            // 履历与当前表必须同数，异常（并发翻转等）时整体回滚
                            throw new Exception("单针调整出库履历与状态更新行数不一致，操作已回滚");
                        }
                        db.Ado.CommitTran();
                    }
                    catch
                    {
                        db.Ado.RollbackTran();
                        throw;
                    }

                    return flipped;
                }
            });
        }

        /// <summary>
        /// 独立客户端：连接串取站点 Web.config 的 TRWCSConnection（与
        /// SqlSugarClientFactory 同源），仅本方法内使用，用完即弃。
        /// </summary>
        private static SqlSugarClient CreateIndependentClient()
        {
            string conn = SqlSugarClientFactory.DB_ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["TRWCSConnection"];
                if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
                {
                    conn = cs.ConnectionString.Trim();
                }
            }
            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new Exception("未找到 TRWCSConnection 连接串，无法执行单针调整出库");
            }

            if (conn.IndexOf("charset", StringComparison.OrdinalIgnoreCase) < 0)
            {
                conn += ";charset=utf8";
            }
            if (conn.IndexOf("SslMode", StringComparison.OrdinalIgnoreCase) < 0)
            {
                conn += ";SslMode=none";
            }
            if (conn.IndexOf("zero datetime", StringComparison.OrdinalIgnoreCase) < 0)
            {
                conn += ";Convert Zero Datetime=True;Allow Zero Datetime=True";
            }
            if (conn.IndexOf("AllowPublicKeyRetrieval", StringComparison.OrdinalIgnoreCase) < 0)
            {
                conn += ";AllowPublicKeyRetrieval=True";
            }

            return new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = conn,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }
    }
}
