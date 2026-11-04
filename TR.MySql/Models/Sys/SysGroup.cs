using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("组别")]
    public partial class SysGroup : EntityBase<int>
    {

        [Description("组别编码")]
        public int GroupId { get; set; }

        [Description("组别名称")]
        public string GroupName { get; set; }

        [Description("组别父节点")]
        public int ParentGroupId { get; set; }

        [Description("是否启用")]
        public bool? Enabled { get; set; }

        [Description("组别描述")]
        public string Description { get; set; }
        public virtual ICollection<SysUser> BodyUser { get; set; }
    }
}
