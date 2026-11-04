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
    [Description("实时报工")]
    public class WmsReportWork: EntityBase<string>
    {
        
        [Description("来源单据")]
        public string SourceOrder { get; set; }

        [Description("仓库")]
        public string WarehouseCode { get; set; }
        
        [Description("条码")]
        public string Barcode { get; set; }

        [Description("产品代码")]
        public string ItemCode { get; set; }

        [Description("产品名称")]
        public string ItemName { get; set; }

        [Description("任务类型")]
        public int Type { get; set; }

        [Description("数量")]
        public int Qty { get; set; }

        [Description("任务状态")]
        public int State { get; set; }

        [Description("备注")]
        public string Remark { get; set; }
    }
}
