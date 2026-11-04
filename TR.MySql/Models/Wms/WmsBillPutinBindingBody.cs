using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("入库时小包装条码与载具条码绑定")]
    public class WmsBillPutinBindingBody : EntityBase<string>
    {
        [Description("单据ID")]
        public string HeaderId { get; set; }

        [Description("载具编号")]
        public string Container { get; set; }

        [Description("包装条码")]
        public string Barcode { get; set; }

        [Description("批次号")]
        public string Lot { get; set; }

        [Description("数量")]
        public int Qty { get; set; }

        [Description("可用标识")]
        public int Enabled { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        public virtual WmsBillPutinBindingHeader Header { get; set; }
    }
}