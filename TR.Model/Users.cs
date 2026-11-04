using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class Users
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string trueName { get; set; }
        public int userGroupid { get; set; }
        public string userGroupName { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string createTime { get; set; }
        public string createOperator { get; set; }
        public string updateTime { get; set; }
        public string updateOperator { get; set; }
        public List<string> roleIdList { get; set; }
        public int userId { get; set; }
        public int roleId { get; set; }
        public string roleName { get; set; }
        public string Describe { get; set; }
    }
}