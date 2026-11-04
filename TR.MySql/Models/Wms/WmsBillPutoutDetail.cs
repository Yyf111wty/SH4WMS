using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("拣配单明细")]
    public class WmsBillPutoutDetail : EntityBase<string>
    {
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("物料编码")]
        public string ItemCode { get; set; }

        [Description("物料名称名称")]
        public string ItemName { get; set; }

        [Description("建议拣货批次")]
        public string Lot { get; set; }

        [Description("拣货数量")]
        public int Qty { get; set; }

        [Description("拣货箱数")]
        public int CtnQty { get; set; }

        [Description("优先级")]
        public int Priority { get; set; }

        [Description("拣货仓库")]
        public string WarehouseCode { get; set; }

        [Description("拣货库区")]
        public string ZoneCode { get; set; }

        [Description("拣货库位")]
        public string LocationCode { get; set; }

        [Description("目标工厂")]
        public string Plant { get; set; }

        [Description("目标出库位置")]
        public string OutLine { get; set; }

        [Description("目标AGV位置")]
        public string AgvLine { get; set; }

        [Description("目标工位")]
        public string WorkCell { get; set; }

        [Description("任务状态")]
        public int State { get; set; }

        [Description("执行人")]
        public string Executor { get; set; }

        [Description("执行时间")]
        public DateTime? SelectTime { get; set; }

        [Description("完成")]
        public DateTime? FinishTime { get; set; }
    }
}