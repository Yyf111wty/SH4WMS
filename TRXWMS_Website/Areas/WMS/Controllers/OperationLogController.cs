using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    public class OperationLogController : Controller
    {
        private readonly wms_operation_logService _service = new wms_operation_logService();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRowData(string PageName, string ButtonName, string OperationType, string Result, string OperatorName, string startTime, string endTime)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageId = ParsePageId(Request["page"]);
            DateTime? start = ParseDateTime(startTime);
            DateTime? end = ParseDateTime(endTime);
            var pageName = PageName ?? "";
            var buttonName = ButtonName ?? "";
            var operationType = OperationType ?? "";
            var result = Result ?? "";
            var operatorName = OperatorName ?? "";
            List<wms_operation_log> lists = null;

            try
            {
                lists = _service.QueryPage(x =>
                        (x.PageName.Contains(pageName) || "" == pageName)
                        && (x.ButtonName.Contains(buttonName) || "" == buttonName)
                        && (x.OperationType.Contains(operationType) || "" == operationType)
                        && (x.Result.Contains(result) || "" == result)
                        && (x.OperatorName.Contains(operatorName) || "" == operatorName)
                        && (start == null || x.OperateTime >= start)
                        && (end == null || x.OperateTime <= end),
                    ref totalCount, pageId, pageSize, "OperateTime DESC").ToList();
            }
            catch (Exception ex)
            {
                return Utf8Json(new { total = 0, rows = new List<wms_operation_log>(), error = ex.Message });
            }

            if (lists == null)
            {
                lists = new List<wms_operation_log>();
            }

            return Utf8Json(new { total = totalCount, rows = lists });
        }

        public ActionResult GetDetail(string id)
        {
            var row = _service.Query(x => x.ID == id).FirstOrDefault();
            return Utf8Json(new { success = row != null, data = row });
        }

        private static int ParsePageSize(string limit)
        {
            int pageSize;
            if (string.IsNullOrEmpty(limit) || limit == "NaN" || !int.TryParse(limit, out pageSize))
            {
                return 20;
            }

            return pageSize;
        }

        private static int ParsePageId(string page)
        {
            int pageId;
            if (string.IsNullOrEmpty(page) || page == "NaN" || !int.TryParse(page, out pageId))
            {
                return 1;
            }

            return pageId;
        }

        private static DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            DateTime dt;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
            {
                return dt;
            }

            return DateTime.TryParse(value, out dt) ? dt : (DateTime?)null;
        }

        private JsonResult Utf8Json(object data)
        {
            return new JsonResult
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                ContentType = "application/json; charset=utf-8",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
