//-----------------------------------------------------------------------
// <copyright file="wms_needle_transaction.cs" company="xxxx Enterprises">
// * Copyright (C) 2026 xxxx Enterprises All Rights Reserved
// * version : 4.0.30319.42000
// * FileName: wms_needle_transaction.cs
// * history : 单针改造·查询页面批次手工登记（风格对齐 T4 生成物）
//-----------------------------------------------------------------------
using System;
using SqlSugar;

namespace TR.ORM.Model
{
    /// <summary>
    /// wms_needle_transaction Entity Model（单针流转履历表，FROM_/TO_ 镜像）
    /// </summary>
    public partial class wms_needle_transaction
    {
        public wms_needle_transaction() { }

        /// <summary>
        ///
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string ID { get; set; }

        /// <summary>
        /// 针当前表行 ID
        /// </summary>
        public string NEEDLEID { get; set; }

        /// <summary>
        /// 针号
        /// </summary>
        public string NEEDLENO { get; set; }

        /// <summary>
        /// 事件类型：BIND/BIND_CANCEL/INBOUND/RESERVE/UNRESERVE/OUTBOUND/ADJUST_OUT/QUARANTINE
        /// </summary>
        public string BIZTYPE { get; set; }

        /// <summary>
        /// 来源单据号（取消事件记被取消任务号）
        /// </summary>
        public string SOURCEORDER { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FROMSTATUS { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TOSTATUS { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FROMITEMCODE { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TOITEMCODE { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string FROMCONTAINER { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int? FROMPOSITION { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TOCONTAINER { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int? TOPOSITION { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string INBOUNDSOURCE { get; set; }

        /// <summary>
        /// 出库机台号
        /// </summary>
        public int? OUTMACHINENO { get; set; }

        /// <summary>
        /// 出库去向（出库口）
        /// </summary>
        public string OUTTARGET { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CREATOR { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime CREATETIME { get; set; }

    }
}
