using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("立库测高记录")]
    public class WmsBillHeight : EntityBase<string>
    {
        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("载具编号")]
        public string Container { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("理论高度")]
        public decimal? TheoreticalHeight { get; set; }

        [Description("实际高度")]
        public decimal? ActualHeight { get; set; }

        [Description("是否达标")]
        public int Result { get; set; }

        [Description("批准入库标识")]
        public int StockFlag { get; set; }

        [Description("备注")]
        public string Remark { get; set; }
    }
}