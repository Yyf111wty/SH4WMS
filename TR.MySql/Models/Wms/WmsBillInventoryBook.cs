using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("库存明细")]
    public class WmsBillInventoryBook : EntityBase<string>
    {
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("单据类型")]
        public string SourceOrderType { get; set; }

        [Description("容器条码")]
        public string Sysbolcode { get; set; }

        [Description("包装条码")]
        public string Barcode { get; set; }


        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("批次")]
        public string Lot { get; set; }

        [Description("单据数量")]
        public int? Qty { get; set; }

        [Description("存储仓库")]
        public string WarehouseCode { get; set; }

        [Description("存储库区")]
        public string ZoneCode { get; set; }

        [Description("存储库位")]
        public string LocationCode { get; set; }

        [Description("供应商编码")]
        public string SupplierId { get; set; }

        [Description("供应商名称")]
        public string SupplierName { get; set; }

        [Description("完成入库标识")]
        public int StockFlag { get; set; }

        [Description("完成入库时间")]
        public DateTime? StockTime { get; set; }

        [Description("冻结标识")]
        public int FreezeFlag { get; set; }

        [Description("冻结时间")]
        public DateTime? FreezeTime { get; set; }

        [Description("冻结原因")]
        public string FreezeReason { get; set; }

        [Description("完成日记账标识")]
        public int FeedbackFlag { get; set; }

        [Description("完成日记账时间")]
        public DateTime? FeedbackTime { get; set; }

        [Description("备注")]
        public string Remark { get; set; }
    }
}