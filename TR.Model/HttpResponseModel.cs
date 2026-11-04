using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TR.Model
{
    public class HttpResponseModel
    {
        public bool result { get; set; }
        public string trueName { get; set; }
        public string UserGroupId   { get; set; }
        public string UserId { get; set; }
        public string message { get; set; }

        public string token { get; set; }
       
    }
}