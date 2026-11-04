using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS.Models.Request
{
    public class OutRequest
    {

        public OutRequest() {
            OneQty = 0;
            TwoQty = 0;
            ThreeQty = 0;
            FourQty = 0;
        }
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
        /// 批次
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 出库位置
        /// </summary>
        public string OutLine { get; set; }

        /// <summary>
        /// 储位一箱数量
        /// </summary>
        public int OneQty { get; set; }

        /// <summary>
        /// 储位两箱数量
        /// </summary>
        public int TwoQty { get; set; }

        /// <summary>
        /// 储位三箱数量
        /// </summary>
        public int ThreeQty { get; set; }

        /// <summary>
        ///标记
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 储位四箱数量
        /// </summary>
        public int FourQty { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int CtnQty
        {
            get
            {
                return FourQty * 4 + ThreeQty *3 + TwoQty * 2 + OneQty;
            }
        }

        /// <summary>
        /// 序列
        /// </summary>
        public string Sequence
        {
            get {
                return FourQty.ToString().PadLeft(4,'0') + ThreeQty.ToString().PadLeft(4,'0') + TwoQty.ToString().PadLeft(4,'0') + OneQty.ToString().PadLeft(4,'0');
            }           
        }
    }
}