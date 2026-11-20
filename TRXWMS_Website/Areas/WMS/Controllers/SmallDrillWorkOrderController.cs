using System.Web.Mvc;
using System.Web.SessionState;
using Newtonsoft.Json;
using TRWMS_BillApi.Models.Response;
using TRWMS_Website.Filters;
using TRWMS_Website.Helper;
using TRXWMS.BILL.WCS;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [CheckLoginFilter]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class SmallDrillWorkOrderController : Controller
    {
        private readonly ISmallDrillWorkOrderService _workOrderService;
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd HH:mm:ss"
        };

        public SmallDrillWorkOrderController()
        {
            _workOrderService = new SmallDrillWorkOrderService();
        }

        private ContentResult JsonData(object data)
        {
            string json = JsonConvert.SerializeObject(data, JsonSettings);
            return Content(json, "application/json");
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SpecDetail(string orderId, int skuSeq)
        {
            ViewBag.OrderId = orderId;
            ViewBag.SkuSeq = skuSeq;
            return View();
        }

        public ActionResult GetWorkOrders(int historyDays = 7)
        {
            SmallDrillWorkOrderListResult data = _workOrderService.GetWorkOrders(historyDays);
            return JsonData(new { success = true, data = data });
        }

        public ActionResult GetSpecTrays(string orderId, int skuSeq)
        {
            SmallDrillSpecTrayListResult data = _workOrderService.GetSpecTrays(orderId, skuSeq);
            return JsonData(new { success = true, data = data });
        }

        [HttpPost]
        public ActionResult MarkTrayAbnormal(string orderId, int skuSeq, string container, string reason)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.MarkTrayAbnormal(orderId, skuSeq, container, reason, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult SwapTray(string orderId, int skuSeq, string container)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.SwapTray(orderId, skuSeq, container, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult ActivateNextSpec(string orderId, int skuSeq)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.ActivateNextSpec(orderId, skuSeq, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult ActivateNextOrder(string currentOrderId)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.ActivateNextWorkOrder(currentOrderId, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult CancelCurrentSpec(string orderId, int skuSeq, string reason)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.CancelCurrentSpec(orderId, skuSeq, reason, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult CancelEntireOrder(string orderId, string reason)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.CancelEntireOrder(orderId, reason, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }

        [HttpPost]
        public ActionResult ForceCloseWorkOrder(string orderId, string reason)
        {
            string editor = HttpContextHelper.GetCurrentUserName();
            string ip = Request?.UserHostAddress ?? string.Empty;
            SmallDrillWorkOrderActionResult result = _workOrderService.ForceCloseWorkOrder(orderId, reason, editor, ip);
            return Json(new { success = result.Success, msg = result.Message, data = result });
        }
    }
}
