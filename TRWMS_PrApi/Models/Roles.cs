using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class Roles
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int IsEnabled { get; set; }
        public string Describes { get; set; }
        public int RoleLevel { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateOperator { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateOperator { get; set; }
    }
}