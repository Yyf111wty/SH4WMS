using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    [Description("载具条码绑定")]
    public class WmsBillPutinBindingHeader : EntityBase<string>
    {
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("载具编号")]
        public string Container { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("请求入库标识")]
        public int QuestFlag { get; set; }

        [Description("请求入库时间")]
        public DateTime? QuestTime { get; set; }

        [Description("巷道编号")]
        public string TunnelNum { get; set; }

        [Description("入库标识")]
        public int StockFlag { get; set; }

        [Description("入库时间")]
        public DateTime? StockTime { get; set; }

        [Description("入库分配库位  ")]
        public string LocationCode { get; set; }

        [Description("有效标识")]
        public int Enabled { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        public virtual ICollection<WmsBillPutinBindingBody> Body { get; set; }
    }
}