using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TR.Model
{

    public class ApiResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("result")]
        public ResultData Result { get; set; }
    }

    public class ResultData
    {
        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
