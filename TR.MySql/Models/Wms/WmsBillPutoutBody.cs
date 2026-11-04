using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("原料上线请求明细")]
    public class WmsBillPutoutBody : EntityBase<string>
    {
        [Description("单头编号")]
        public string HeaderId { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("工厂编号")]
        public string Plant { get; set; }

        [Description("出库编号")]
        public string OutLine { get; set; }

        [Description("出库编号")]
        public string AgvLine { get; set; }

        [Description("工位编号")]
        public string WorkCell { get; set; }

        [Description("批次")]
        public string Lot { get; set; }

        [Description("需求数量")]
        public int Qty { get; set; }

        [Description("需求箱数")]
        public int CtnQty { get; set; }

        [Description("需求序列")]
        public string Sequence { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        public virtual WmsBillPutoutHeader Header { get; set; }
    }
}