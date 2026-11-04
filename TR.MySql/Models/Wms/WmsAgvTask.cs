using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Common;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("AGV任务")]
    public class WmsAgvTask : EntityBase<Int32>
    {
        public WmsAgvTask()
        {
            this.Type = 1;
            this.Priority = 1;
            this.State = 0;
        }
     
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("仓库")]
        public string WarehouseCode { get; set; }

        [Description("容器号")]
        public string Container { get; set; }

        [Description("条码")]
        public string Barcode { get; set; }

        [Description("产品代码")]
        public string ItemCode { get; set; }

        [Description("产品名称")]
        public string ItemName { get; set; }
     
        [Description("任务类型")]
        public int Type { get; set; }

        [Description("优先级")]
        public int Priority { get; set; }

        [Description("起点")]
        public string StartTaxi { get; set; }

        [Description("结点")]
        public string EndTaxi { get; set; }

        [Description("任务状态")]
        public int State { get; set; }

        [Description("任务组")]
        public string GroupCode { get; set; }
    }
}
