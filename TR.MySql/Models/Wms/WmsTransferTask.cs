using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("移库明细")]
    public class WmsTransferTask : EntityBase<string>
    {
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("条码编号")]
        public string Barcode { get; set; }

        [Description("批次")]
        public string Lot { get; set; }

        [Description("数量")]
        public int? Qty { get; set; }

        [Description("移出仓库编号")]
        public string SourceWarehouseCode { get; set; }

        [Description("移出库区编号")]
        public string SourceZoneCode { get; set; }

        [Description("移出库位编号")]
        public string SourceLocationCode { get; set; }

        [Description("移入仓库编号")]
        public string TargetWarehouseCode { get; set; }

        [Description("移入库区编号")]
        public string TargetZoneCode { get; set; }

        [Description("移入库位编号")]
        public string TargetLocationCode { get; set; }

        [Description("满足标识")]
        public int StockFlag { get; set; }

        [Description("满足转移时间")]
        public DateTime? StockTime { get; set; }

        [Description("转移标识")]
        public int TransferFlag { get; set; }

        [Description("转移时间")]
        public DateTime? TransferTime { get; set; }
    }
}