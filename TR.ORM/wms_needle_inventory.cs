//-----------------------------------------------------------------------
// <copyright file="wms_needle_inventory.cs" company="xxxx Enterprises">
// * Copyright (C) 2026 xxxx Enterprises All Rights Reserved
// * version : 4.0.30319.42000
// * FileName: wms_needle_inventory.cs
// * history : 单针改造·查询页面批次手工登记（风格对齐 T4 生成物）
//-----------------------------------------------------------------------
using System;
using SqlSugar;

namespace TR.ORM.Model
{
    /// <summary>
    /// wms_needle_inventory Entity Model（单针当前库存/状态表）
    /// </summary>
    public partial class wms_needle_inventory
    {
        public wms_needle_inventory() { }

        /// <summary>
        ///
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }

        /// <summary>
        /// 针号（唯一；临时针以 T- 前缀标识）
        /// </summary>
        public string NEEDLENO { get; set; }

        /// <summary>
        /// 规格（ITEMCODE）
        /// </summary>
        public string ITEMCODE { get; set; }

        /// <summary>
        /// 盒号（库位经 CONTAINER→wms_base_location 关联）
        /// </summary>
        public string CONTAINER { get; set; }

        /// <summary>
        /// 针在盒中的槽位号
        /// </summary>
        public int POSITIONCODE { get; set; }

        /// <summary>
        /// 状态：INBOUND_PENDING/AVAILABLE/RESERVED/OUTBOUND/BIND_CANCELLED/ADJUSTED_OUT/QUARANTINED
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// 入库来源（入库单 TASKTYPE）
        /// </summary>
        public string INBOUNDSOURCE { get; set; }

        /// <summary>
        /// 最近一次绑定的入库单头 ID（wms_bill_putinbindingheader.ID）
        /// </summary>
        public string INBOUNDORDER { get; set; }

        /// <summary>
        /// 预占单据号
        /// </summary>
        public string RESERVEORDER { get; set; }

        /// <summary>
        /// 出库单号
        /// </summary>
        public string OUTBOUNDORDER { get; set; }

        /// <summary>
        /// 出库机台号
        /// </summary>
        public int? OUTMACHINENO { get; set; }

        /// <summary>
        /// 出库去向（出库口 ENDTAXI）
        /// </summary>
        public string OUTTARGET { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        /// 有效标记 1/0
        /// </summary>
        public int ENABLED { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CREATOR { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime CREATETIME { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string LASTEDITOR { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime LASTEDITTIME { get; set; }

    }
}
