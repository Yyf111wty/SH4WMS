using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("货位")]
    public class WmsBaseLocation : EntityBase<string>
    {
        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("库区ID")]
        public string ZoneId { get; set; }

        [Description("库区编号")]
        public string ZoneCode { get; set; }

        [Description("货位编号")]
        public string Code { get; set; }

        [Description("货位类型")]
        public string Type { get; set; }

        [Description("排")]
        public int RowNum { set; get; }

        [Description("列")]
        public int ColumnNum { set; get; }

        [Description("层")]
        public int LayerNum { set; get; }

        [Description("巷道编号")]
        public int? TunnelNum { set; get; }

        [Description("货位限高")]
        public int? LimitHeight { get; set; }

        [Description("货位限高重")]
        public int? LimitWeight { get; set; }

        [Description("拣选顺序")]
        public int? PickOrder { set; get; }

        [Description("占位符")]
        public string Symbol { get; set; }

        [Description("巷道占用")]
        public int? SortType { get; set; }

        [Description("远近伸位标识")]
        public int? ExitOrout { get; set; }

        [Description("分组标识")]
        public int? GroupCode { get; set; }

        [Description("现存数量")]
        public int QTY { get; set; }

        [Description("利用次数")]
        public int? Times { get; set; }

        [Description("激活标识")]
        public int? Enabled { get; set; }

        [Description("可出入库标识")]
        public int? IsAvialable { get; set; }

        [Description("备注信息")]
        public string Remark { get; set; }

        [Description("库位坐标")]
        public string Xyz { get; set; }

        [Description("盘点坐标")]
        public string CheckXyz { get; set; }

        [Description("规格")]
        public string Specs { get; set; }

        [Description("是否混料")]
        public string IfMixing { get; set; }

        public virtual WmsBaseZone Zone { get; set; }
    }
}