using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_PrApi.Models
{
    public class Login_get
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Sign { get; set; }
        public string UnixTime { get; set; }
    }
}