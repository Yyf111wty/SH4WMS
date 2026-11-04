using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS.Models.Request
{
    public class InRequest
    {
        /// <summary>
        /// 任务号
        /// </summary>
        public string SourceOrder { get; set; }
        /// <summary>
        /// 料号
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 料名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Qty { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string Lot { get; set; }       
    }
    
}