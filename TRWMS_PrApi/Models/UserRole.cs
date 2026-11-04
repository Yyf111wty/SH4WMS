using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateOperator { get; set; }
    }
}