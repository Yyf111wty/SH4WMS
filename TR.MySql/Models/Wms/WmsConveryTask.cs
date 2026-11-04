using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("输送线任务")]
    public class WmsConveryTask : EntityBase<string>
    {
        [Description("仓库")]
        public string WarehouseCode { get; set; }

        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("条码")]
        public string Barcode { get; set; }

        [Description("容器号")]
        public string Container { get; set; }


        [Description("数量")]
        public string Qty { get; set; }

        [Description("批次数量")]
        public string BatchQty { get; set; }

        [Description("产品代码")]
        public string ItemCode { get; set; }

        [Description("产品名称")]
        public string ItemName { get; set; }

        [Description("任务类型")]
        public int Type { get; set; }

        [Description("任务状态")]
        public int State { get; set; }

        [Description("起始位置")]
        public string StartTaxi { get; set; }

        [Description("终点位置")]
        public string EndTaxi { get; set; }
    }
}
