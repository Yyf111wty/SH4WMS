using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Response
{
    public class ReslutLYJson
    {

        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public Result Result { get; set; }
    }
    public class Result
    {
        public int length { get; set; }
        public Dictionary<string, int> dictionary { get; set; }
    }
}