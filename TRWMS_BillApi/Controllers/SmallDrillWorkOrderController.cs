using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using TRWMS_BillApi.Models.Response;
using TRXWMS.BILL.WCS;
using TRXWMS.Models.Response;

namespace TRXWMS.Controllers
{
    public class SmallDrillWorkOrderController : ApiController
    {
        private readonly ISmallDrillWorkOrderService _workOrderService;

        public SmallDrillWorkOrderController()
        {
            _workOrderService = new SmallDrillWorkOrderService();
        }

        public SmallDrillWorkOrderController(ISmallDrillWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        [HttpGet, Route("api/wms/smalldrill/workorders")]
        public ReslutJson GetWorkOrders(int historyDays = 7)
        {
            return OkResult(_workOrderService.GetWorkOrders(historyDays));
        }

        [HttpGet, Route("api/wms/smalldrill/spec/trays")]
        public ReslutJson GetSpecTrays(string orderId, int skuSeq)
        {
            return OkResult(_workOrderService.GetSpecTrays(orderId, skuSeq));
        }

        [HttpPost, Route("api/wms/smalldrill/tray/mark-abnormal")]
        public ReslutJson MarkTrayAbnormal(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.MarkTrayAbnormal(
                json?["orderId"]?.ToString(),
                json?["skuSeq"] != null ? int.Parse(json["skuSeq"].ToString()) : 0,
                json?["container"]?.ToString(),
                json?["reason"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/tray/swap")]
        public ReslutJson SwapTray(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.SwapTray(
                json?["orderId"]?.ToString(),
                json?["skuSeq"] != null ? int.Parse(json["skuSeq"].ToString()) : 0,
                json?["container"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/activate-next-spec")]
        public ReslutJson ActivateNextSpec(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.ActivateNextSpec(
                json?["orderId"]?.ToString(),
                json?["skuSeq"] != null ? int.Parse(json["skuSeq"].ToString()) : 0,
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/activate-next-order")]
        public ReslutJson ActivateNextOrder(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.ActivateNextWorkOrder(
                json?["currentOrderId"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/cancel-current-spec")]
        public ReslutJson CancelCurrentSpec(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.CancelCurrentSpec(
                json?["orderId"]?.ToString(),
                json?["skuSeq"] != null ? int.Parse(json["skuSeq"].ToString()) : 0,
                json?["reason"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/cancel-entire-order")]
        public ReslutJson CancelEntireOrder(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.CancelEntireOrder(
                json?["orderId"]?.ToString(),
                json?["reason"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        [HttpPost, Route("api/wms/smalldrill/force-close-order")]
        public ReslutJson ForceCloseWorkOrder(JObject json)
        {
            string ip = HttpContext.Current?.Request?.UserHostAddress ?? string.Empty;
            return ActionResult(_workOrderService.ForceCloseWorkOrder(
                json?["orderId"]?.ToString(),
                json?["reason"]?.ToString(),
                json?["editor"]?.ToString(),
                ip));
        }

        private static ReslutJson OkResult(object data)
        {
            return new ReslutJson { success = true, msg = "ok", data = data };
        }

        private static ReslutJson ActionResult(SmallDrillWorkOrderActionResult actionResult)
        {
            return new ReslutJson
            {
                success = actionResult.Success,
                msg = actionResult.Message,
                data = actionResult
            };
        }
    }
}
