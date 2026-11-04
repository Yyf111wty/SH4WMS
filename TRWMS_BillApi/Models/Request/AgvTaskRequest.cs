using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS.Models.Request
{
    public class AgvTaskRequest
    {
        public AgvTaskRequest() {
            State = -1;
            //group = DateTime.Now.ToString("yyyyMMddhhssmmfff");
        }
        /// <summary>
        /// 任务号
        /// </summary>
        public string SourceOrder { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 起始位置
        /// </summary>
        public string StartTaxi { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public string EndTaxi { get; set; }

        public int Priority { get; set; }

        public string group { get; set; }
    }
}