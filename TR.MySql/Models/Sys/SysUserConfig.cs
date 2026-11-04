
using System;
using System.ComponentModel;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("用户配置")]
    public partial class SysUserConfig : EntityBase<int>
    {
        [Description("名称")]
        public string Name { get; set; }

        [Description("值")]
        public string Value { get; set; }

        [Description("类型")]
        public string Type { get; set; }

        [Description("有效标识")]
        public bool Enabled { get; set; }

        [Description("用户ID")]
        public int SysUserId { get; set; }

        public virtual SysUser SysUser { get; set; }
    }
}