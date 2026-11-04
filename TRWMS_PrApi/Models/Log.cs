using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class Log
    {
        public string ID { get; set; }
        public string Type { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string Creator { get; set; }
        public string CreateTime { get; set; }
    }
    public class ExceptionDatabase
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string TargetSite { get; set; }
        public string Data { get; set; }
        public string Creator { get; set; }
        public string CreateTime { get; set; }
    }
}