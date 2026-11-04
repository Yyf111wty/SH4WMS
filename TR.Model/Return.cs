using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
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
        public string message { get; set; }
    }

}