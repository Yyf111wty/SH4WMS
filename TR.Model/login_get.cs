using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class Login_get
    {
        public Login_get(string userName, string password,string sign, string unixTime) {
            this.userName = userName;
            this.password = password;
            this.sign = sign;
            this.unixTime = unixTime;
        }
        public string userName { get; set; }
        public string password { get; set; }
        public string sign { get; set; }
        public string unixTime { get; set; }
    }
}