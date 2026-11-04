using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class Roles
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public bool IsEnabled { get; set; }
        public string Describe { get; set; }
        public string RoleLevel { get; set; }
        public string CreateTime { get; set; }
        public string CreateOperator { get; set; }
        public string UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
    }
}