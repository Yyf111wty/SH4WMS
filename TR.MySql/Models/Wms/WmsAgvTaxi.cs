using TR.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.MySql.Models.Wms
{
    [Serializable]
    [Description("AGV停靠站")]
    public class WmsAgvTaxi : EntityBase<Int32>
    {
        [Description("站点编号")]
        public string Code { get; set; }

        [Description("货物编号")]
        public string ItemCode { get; set; }

        [Description("货物名称")]
        public string ItemName { get; set; }

        [Description("条码")]
        public string BarCode { get; set; }

        [Description("类型")]
        public int Type { get; set; }

        [Description("任务分配标识")]
        public int State { get; set; }

        [Description("激活标识")]
        public int Enabeld { get; set; }

    }
}
