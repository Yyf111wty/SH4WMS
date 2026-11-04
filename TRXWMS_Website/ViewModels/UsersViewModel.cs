using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_Website.ViewModels
{
    public class UsersViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string TrueName { get; set; }
        public string Password { get; set; }
        public int UserGroupId { get; set; }
        public string UserGroupName { get; set; }
        public int ParentGroupId { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateOperator { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Describes { get; set; }
        public string SecretKeyType { get; set; }
        public string SecretKeyValue { get; set; }
    }
}