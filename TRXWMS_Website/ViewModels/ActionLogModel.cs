using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.ViewModels
{
    public class ActionLogModel
    {
        public string Type { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string Creator { get; set; }
        public string CreateTime { get; set; }
    }
}