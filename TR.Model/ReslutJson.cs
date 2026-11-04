using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace TR.Model
{
    public class ReslutJson
    {
        public Boolean success { get; set; }
        /// <summary>
        /// 操作消息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 操作编码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 操作数据
        /// </summary>
        public Object data { get; set; }
    }
}
