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
    [Description("条码入库信息")]
    public class WmsBillPutIn : EntityBase<string>
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

        [Description("条码")]
        public string Barcode { get; set; }

        [Description("请求入库标识")]
        public int QuestFlag { get; set; }

        [Description("请求入库时间")]
        public DateTime? QuestTime { get; set; }

        [Description("数量")]
        public int Qty { get; set; }

        [Description("批次")]
        public string Lot { get; set; }

        [Description("有效标识")]
        public bool Enabled { get; set; }

        [Description("备注")]
        public string Remark { get; set; }
    }
}
