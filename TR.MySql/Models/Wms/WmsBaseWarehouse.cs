using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("仓库")]
    public class WmsBaseWarehouse : EntityBase<string>
    {
        /// </summary>
        [Description("仓库编号")]
        public string Code { get; set; }

        [Description("工厂编号")]
        public string FactoryCode { get; set; }

        [Description("描述")]
        public string Description { get; set; }

        [Description("库位数量")]
        public int? Qty { get; set; }

        [Description("层数")]
        public int? LayerNum { get; set; }

        [Description("排数")]
        public int? RowNum { get; set; }

        [Description("列数")]
        public int? ColumnNum { get; set; }

        [Description("单伸")]
        public int? SiginNum { get; set; }

        [Description("入库原则")]
        public int? EntPrinciple { get; set; }

        [Description("出库原则")]
        public int OutPrinciple { get; set; }

        [Description("激活标识")]
        public int Enabled { get; set; }

        public List<WmsBaseZone> Zones {get;set;}

        public WmsBaseWarehouse()
        {
            this.Zones = new List<WmsBaseZone>();
        }
    }
}