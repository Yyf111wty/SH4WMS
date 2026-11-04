using System;
using System.ComponentModel;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("系统配置")]
    public partial class SysSetting : EntityBase<int>
    {
        [Description("名称")]
        public string Name { get; set; }

        [Description("值")]
        public string Value { get; set; }

        [Description("类型")]
        public int Type { get; set; }

        [Description("有效标识")]
        public bool Enabled { get; set; }

        [Description("备注")]
        public string ReMark { get; set; }
    }
}