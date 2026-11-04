using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class Group
    {
        public int id { get; set; }
        public string groupName { get; set; }
        public int parentGroupId { get; set; }
        public string parentGroupName { get; set; }
        public bool isEnabled { get; set; }
        public string describe { get; set; }
        public string createTime { get; set; }
        public string createOperator { get; set; }
        public string updateTime { get; set; }
        public string updateOperator { get; set; }
        public string state { get; set; }
    }
}