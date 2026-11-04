using System.Collections.Generic;

namespace TRWMS_BillApi.Models.Response
{
    /// <summary>
    /// 备刀机查询小刀组盘出库托盘信息 - 响应
    /// </summary>
    public class ReslutKnifePalletJson
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public KnifePalletOutboundResult Result { get; set; }
    }

    /// <summary>
    /// 托盘出库汇总信息
    /// </summary>
    public class KnifePalletOutboundResult
    {
        /// <summary>工单名称（出库任务 workname）</summary>
        public string workname { get; set; }
        /// <summary>订单号</summary>
        public string orderId { get; set; }
        /// <summary>大托盘条码</summary>
        public string pallet { get; set; }
        /// <summary>托盘内盒子数量</summary>
        public int boxCount { get; set; }
        /// <summary>托盘内各小托盘/盒子明细</summary>
        public List<KnifePalletBoxInfo> boxes { get; set; }
    }

    /// <summary>
    /// 托盘内单盒信息
    /// </summary>
    public class KnifePalletBoxInfo
    {
        /// <summary>小托盘/盒子条码</summary>
        public string container { get; set; }
        /// <summary>规格</summary>
        public string specc { get; set; }
    }
}
