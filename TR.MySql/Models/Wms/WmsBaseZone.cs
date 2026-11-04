using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("货区")]
    public class WmsBaseZone : EntityBase<string>
    {
        public WmsBaseZone()
        {
            this.Locations = new List<WmsBaseLocation>();
        }

        [Description("库区编号")]
        public string Code { get; set; }

        [Description("描述")]
        public string Description { get; set; }

        [Description("仓库ID")]
        public string WarehouseId { get; set; }

        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("激活标识")]
        public  int Enabled { get; set; }

        public virtual WmsBaseWarehouse Warehouse { get; set; }

        public virtual ICollection<WmsBaseLocation> Locations { get; set; }
    }
}