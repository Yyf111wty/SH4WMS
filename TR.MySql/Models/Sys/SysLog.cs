using System;
using System.ComponentModel;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("操作日志")]
    public partial class SysLog : EntityBase<string>
    {
        [Description("信息")]
        public string Message { get; set; }

        [Description("操作结果")]
        public string Result { get; set; }

        [Description("类型")]
        public string Type { get; set; }

        [Description("模块名称")]
        public string Module { get; set; }

        [Description("IP地址")]
        public string IP { get; set; }
    }
}