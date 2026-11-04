using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRWMS_BillApi.Models.Response
{
    public class ReslutKnifeJson
    {
        /// <summary>
        /// 操作结果
        /// </summary>
        public Boolean Success { get; set; }
        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 操作编码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 业务反馈数据
        /// </summary>
        public ReslutresJson Result { get; set; }
    }
    public class ReslutresJson
    {
        /// <summary>
        /// 工单名称
        /// </summary>
        public string workname { get; set; }
        /// <summary>
        /// 工单套数
        /// </summary>
        public int sets { get; set; }
        /// <summary>
        /// 库存不足反馈数据
        /// </summary>
        public Object dictionary { get; set; }
        /// <summary>
        /// 分配的托盘二维码列表
        /// </summary>
        public List<QRCodeList> QRCodeList { get; set; }
    }
    public class QRCodeList
    {
        /// <summary>
        /// 托盘号
        /// </summary>
        public string QRCode { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// WCS 装盘前查询本盘应装数量
    /// </summary>
    public class SmallDrillPackingQtyResult
    {
        public string orderInfo { get; set; }
        public string orderId { get; set; }
        public string specc { get; set; }
        public int sets { get; set; }
        public int skuSeq { get; set; }
        /// <summary>当前规格未标异常盒数（出库单头 STATE≠5，含已装盘与换料补配）</summary>
        //public int normalTotal { get; set; }
        public int remaining { get; set; }
        public int plating { get; set; }
        /// <summary>按 SKUSEQ+STATE 从库中加载的候选任务数（内存过滤前）</summary>
        public int candidateCount { get; set; }
        /// <summary>候选任务逐条匹配诊断（便于排查 remaining=0）</summary>
        public object taskEvaluations { get; set; }
    }
}