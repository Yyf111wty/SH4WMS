using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Response
{
    public class ReslutLYCodeJson
    {

        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public List<CodeResult> result { get; set; }
    }
    public class CodeResult
    {
        public string QRCode { get; set; }
        public string Content { get; set; }
        public string NeedleNos { get; set; }

    }
}