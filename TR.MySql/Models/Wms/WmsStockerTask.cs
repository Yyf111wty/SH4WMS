using TR.Common;
using System;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("堆垛机任务")]
    public class WmsStockerTask : EntityBase<string>
    {
        [Description("任务编号")]
        public string Code { get; set; }

        [Description("单据")]
        public string Sourceorder { get; set; }

        [Description("仓库编号")]
        public string WarehouseCode { get; set; }

        [Description("载具编号")]
        public string Container { get; set; }

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

        [Description("创建时间")]
        public DateTime? StartTime { get; set; }

        [Description("下发时间")]
        public DateTime? ImplementTime { get; set; }


        [Description("完成时间")]
        public DateTime? FinishTime { get; set; }

        [Description("物料号")]
        public string ItemCode { get; set; }

        [Description("物料名称")]
        public string ItemName { get; set; }
    }
}