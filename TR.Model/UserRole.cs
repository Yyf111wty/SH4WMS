using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string CreateTime { get; set; }
        public string CreateOperator { get; set; }
    }
}