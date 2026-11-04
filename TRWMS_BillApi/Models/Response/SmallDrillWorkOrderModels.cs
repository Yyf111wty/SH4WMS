using System;
using System.Collections.Generic;

namespace TRWMS_BillApi.Models.Response
{
    public class SmallDrillWorkOrderTreeNode
    {
        public string NodeId { get; set; }
        public bool IsSpec { get; set; }
        public string OrderId { get; set; }
        public string Workname { get; set; }
        public int SkuSeq { get; set; }
        public string Spec { get; set; }
        public string QueueStatus { get; set; }
        public string QueueStatusText { get; set; }
        public DateTime? CreateTime { get; set; }
        public int DemandQty { get; set; }
        public int ActiveQty { get; set; }
        public int DoneTrays { get; set; }
        public int AbnormalTrays { get; set; }
        public int PendingTrays { get; set; }
        public int SkuCount { get; set; }
        public bool IsCurrentExecutingSpec { get; set; }
        public bool CanActivateNextSpec { get; set; }
        public bool CanActivateNextOrder { get; set; }
        public bool CanCancelCurrentSpec { get; set; }
        public bool CanCancelEntireOrder { get; set; }
        /// <summary>待处理为 0 且仍有待换料异常盘时，可无库存强制结案</summary>
        public bool CanForceCloseWorkOrder { get; set; }
        public bool ReadOnly { get; set; }
        public List<SmallDrillWorkOrderTreeNode> Children { get; set; }
    }

    public class SmallDrillWorkOrderListResult
    {
        public List<SmallDrillWorkOrderTreeNode> Active { get; set; }
        public List<SmallDrillWorkOrderTreeNode> Waiting { get; set; }
        public List<SmallDrillWorkOrderTreeNode> History { get; set; }
        public string CurrentOrderId { get; set; }
        public int CurrentSkuSeq { get; set; }
        public bool CanActivateNextOrder { get; set; }
        /// <summary>CanActivateNextOrder=false 时的简要原因（便于现场排查）</summary>
        public string ActivateNextOrderBlockReason { get; set; }
    }

    public class SmallDrillSpecTrayItem
    {
        public string HeaderId { get; set; }
        public string Container { get; set; }
        public string Spec { get; set; }
        public int State { get; set; }
        public string StateText { get; set; }
        public string TrayType { get; set; }
        public string ReplacedContainer { get; set; }
        public string RootContainer { get; set; }
        public string LocationCode { get; set; }
        public string Remark { get; set; }
        public DateTime? CreateTime { get; set; }
        public bool CanMarkAbnormal { get; set; }
        public bool CanSwap { get; set; }
        public bool AlreadySwappedOut { get; set; }
        public bool ReadOnly { get; set; }
    }

    public class SmallDrillSpecTrayListResult
    {
        public string OrderId { get; set; }
        public string Workname { get; set; }
        public int SkuSeq { get; set; }
        public string Spec { get; set; }
        public int DemandQty { get; set; }
        public int ActiveQty { get; set; }
        public bool IsCurrentExecutingSpec { get; set; }
        public bool ReadOnly { get; set; }
        public List<SmallDrillSpecTrayItem> Trays { get; set; }
    }

    public class SmallDrillWorkOrderActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string NextOrderId { get; set; }
        public int NextSkuSeq { get; set; }
        public string NewContainer { get; set; }
        public int ReleasedLocations { get; set; }
        public int AffectedCount { get; set; }
    }
}
