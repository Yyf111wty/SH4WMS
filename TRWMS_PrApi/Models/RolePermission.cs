using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public List<string> PermissionList { get; set; }
        public string CreateOperator { get; set; }
    }
}