using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Common;

namespace TR.MySql.Models.Sys
{
    [Serializable]
    [Description("异常信息")]
    public class SysException : EntityBase<string>
    {
        [Description("帮助链接")]
        public string HelpLink { get; set; }

        [Description("错误信息")]
        public string Message { get; set; }

        [Description("来源")]
        public string Source { get; set; }

        [Description("堆栈")]
        public string StackTrace { get; set; }

        [Description("目标页")]
        public string TargetSite { get; set; }

        [Description("程序集")]
        public string Data { get; set; }
    }
}
