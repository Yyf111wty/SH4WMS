using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("拣配清单")]
    public class WmsBillPutoutHeader : EntityBase<string>
    {
        public WmsBillPutoutHeader()
        {
            this.Body = new List<WmsBillPutoutBody>();
        }

        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("单据类型")]
        public string SourceOrderType { get; set; }

        [Description("建议送达时间")]
        public DateTime SuggestTime { get; set; }

        [Description("批准拣货标识")]
        public int AppoveFlag { get; set; }

        [Description("批准拣货时间")]
        public DateTime? AppoveTime { get; set; }

        [Description("拣货人")]
        public string AppoveExecutor { get; set; }

        [Description("单据状态标识")]
        public int StockFlag { get; set; }

        [Description("完成拣货时间")]
        public DateTime? StockTime { get; set; }

        [Description("完成日记账标识")]
        public int FeedbackFlag { get; set; }

        [Description("完成日记账时间")]
        public DateTime? FeedbackTime { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        public virtual ICollection<WmsBillPutoutBody> Body { get; set; }
    }
}