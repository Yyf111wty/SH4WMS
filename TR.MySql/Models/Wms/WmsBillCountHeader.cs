using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("盘点单头")]
    public class WmsBillCountHeader : EntityBase<string>
    {
        public WmsBillCountHeader()
        {
            this.Body = new List<WmsBillCountBody>();
        }

        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("盘点类型")]
        public string Type { get; set; }

        [Description("批准盘点标识")]
        public int ApproveFlag { get; set; }

        [Description("批准盘点时间")]
        public DateTime? ApproveTime { get; set; }

        [Description("批准盘点人")]
        public string ApproveExecutor { get; set; }

        [Description("单据状态标识")]
        public int StockFlag { get; set; }

        [Description("完成拣货时间")]
        public DateTime? StockTime { get; set; }


        [Description("完成盘点标识")]
        public int FinishFlag { get; set; }

        [Description("完成盘点时间")]
        public DateTime? FinishTime { get; set; }


        [Description("备注")]
        public string Remark { get; set; }

        public virtual ICollection<WmsBillCountBody> Body { get; set; }
    }
}