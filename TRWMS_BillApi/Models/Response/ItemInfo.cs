using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Response
{
    public class ItemInfo
    {
        /// <summary>
        /// 厂商
        /// </summary>
        public string SUPPLIER { get; set; }
        /// <summary>
        /// 系列
        /// </summary>
        public string Series { get; set; }
        /// <summary>
        /// 刀具类型
        /// </summary>
        public string ToolType { get; set; }
        /// <summary>
        /// 单双刃
        /// </summary>
        public string DSEdged { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string spec { get; set; }
        /// <summary>
        /// 刃长
        /// </summary>
        public string BladeLength { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Qty { get; set; }
        /// <summary>
        /// 磨次
        /// </summary>
        public string MC { get; set; }
        /// <summary>
        /// 出库码
        /// </summary>
        public string SpecCode { get; set; }
    }
}