using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("盘点单身")]
    public class WmsBillCountBody : EntityBase<string>
    {
        [Description("单头编号")]
        public string HeaderId { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        public virtual WmsBillCountHeader Header { get; set; }
    }
}