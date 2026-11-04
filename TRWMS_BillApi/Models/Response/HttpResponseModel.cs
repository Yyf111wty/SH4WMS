using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRXWMS.Models.Response
{
    public class HttpResponseModel
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public List<Object> data { get; set; }
    }
}