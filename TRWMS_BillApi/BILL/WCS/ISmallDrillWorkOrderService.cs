using TRWMS_BillApi.Models.Response;

namespace TRXWMS.BILL.WCS
{
    public interface ISmallDrillWorkOrderService
    {
        SmallDrillWorkOrderListResult GetWorkOrders(int historyDays);

        SmallDrillSpecTrayListResult GetSpecTrays(string orderId, int skuSeq);

        SmallDrillWorkOrderActionResult MarkTrayAbnormal(string orderId, int skuSeq, string container, string reason, string editor, string ip);

        SmallDrillWorkOrderActionResult SwapTray(string orderId, int skuSeq, string abnormalContainer, string editor, string ip);

        SmallDrillWorkOrderActionResult ActivateNextSpec(string orderId, int currentSkuSeq, string editor, string ip);

        SmallDrillWorkOrderActionResult ActivateNextWorkOrder(string currentOrderId, string editor, string ip);

        SmallDrillWorkOrderActionResult CancelCurrentSpec(string orderId, int skuSeq, string reason, string editor, string ip);

        SmallDrillWorkOrderActionResult CancelEntireOrder(string orderId, string reason, string editor, string ip);

        SmallDrillWorkOrderActionResult ForceCloseWorkOrder(string orderId, string reason, string editor, string ip);
    }
}
