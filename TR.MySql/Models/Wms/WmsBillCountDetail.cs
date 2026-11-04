using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("盘点明细")]
    public class WmsBillCountDetail : EntityBase<string>
    {
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("库区编号")]
        public string ZoneCode { get; set; }

        [Description("库位编号")]
        public string LocationCode { get; set; }

        [Description("条码编号")]
        public string Barcode { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("批次")]
        public string Lot { get; set; }

        [Description("库存数量")]
        public int? LocalQty { get; set; }

        [Description("盘点数量")]
        public int? CheckQty { get; set; }

        [Description("盘差数量")]
        public int? DiffQty { get; set; }

        [Description("核对标识")]
        public int CheckFlag { get; set; }

        [Description("核对时间")]
        public DateTime? CheckTime { get; set; }
    }
}