using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class Return
    {
        public bool result { get; set; }
        public string message { get; set; }
    }
    public class LoginReturn
    {
        public bool result { get; set; }
        public string trueName { get; set; }
        public string UserGroupId { get; set; }
        public  string UserId { get; set; }
        public string message { get; set; }
    }

}