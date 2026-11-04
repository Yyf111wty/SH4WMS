using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using TRXWMS_Website.Helper;

namespace TRWMS_Website.Filters
{
    public sealed class WmsOperationLogFilterAttribute : ActionFilterAttribute
    {
        private const string ItemRuleKey = "__WmsOpLogRule";
        private const string ItemBeforeKey = "__WmsOpLogBefore";
        private const string ItemRequestKey = "__WmsOpLogRequest";
        private const string ItemStopwatchKey = "__WmsOpLogStopwatch";
        private const string ItemEntityIdKey = "__WmsOpLogEntityId";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            WmsOperationLogRule rule;
            if (!TryGetRule(filterContext, out rule))
            {
                return;
            }

            var parameters = filterContext.ActionParameters;
            var entityId = WmsOperationSnapshotLoader.ExtractId(parameters);
            filterContext.HttpContext.Items[ItemRuleKey] = rule;
            filterContext.HttpContext.Items[ItemRequestKey] = WmsOperationSnapshotLoader.BuildRequestJson(parameters);
            filterContext.HttpContext.Items[ItemEntityIdKey] = entityId;
            filterContext.HttpContext.Items[ItemStopwatchKey] = Stopwatch.StartNew();

            if (rule.SnapshotMode == WmsOperationSnapshotMode.BeforeOnly
                || rule.SnapshotMode == WmsOperationSnapshotMode.BeforeAndAfter)
            {
                filterContext.HttpContext.Items[ItemBeforeKey] =
                    WmsOperationSnapshotLoader.LoadBefore(rule.SnapshotKey, entityId);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            WmsOperationLogRule rule = filterContext.HttpContext.Items[ItemRuleKey] as WmsOperationLogRule;
            if (rule == null)
            {
                return;
            }

            var request = filterContext.HttpContext.Request;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            var success = filterContext.Exception == null;
            string errorMessage = null;

            if (filterContext.Exception != null)
            {
                errorMessage = filterContext.Exception.Message;
            }
            else if (filterContext.Result is JsonResult)
            {
                var jsonResult = (JsonResult)filterContext.Result;
                success = WmsOperationLogWriter.TryParseSuccess(jsonResult.Data, out errorMessage);
            }
            else if (filterContext.Result is FileResult)
            {
                success = true;
            }

            var entityId = filterContext.HttpContext.Items[ItemEntityIdKey] as string;
            var dataBefore = filterContext.HttpContext.Items[ItemBeforeKey] as string;
            var requestData = filterContext.HttpContext.Items[ItemRequestKey] as string;
            string dataAfter = null;

            if (success)
            {
                if (rule.SnapshotMode == WmsOperationSnapshotMode.BeforeAndAfter)
                {
                    dataAfter = WmsOperationSnapshotLoader.LoadBefore(rule.SnapshotKey, entityId);
                }
                else if (rule.SnapshotMode == WmsOperationSnapshotMode.RequestOnly)
                {
                    dataAfter = requestData;
                }
            }

            if (filterContext.Result is FileResult fileResult)
            {
                requestData = MergeExportInfo(requestData, fileResult);
            }

            int? durationMs = null;
            var stopwatch = filterContext.HttpContext.Items[ItemStopwatchKey] as Stopwatch;
            if (stopwatch != null)
            {
                stopwatch.Stop();
                durationMs = (int)stopwatch.ElapsedMilliseconds;
            }

            int pageId;
            int? parsedPageId = null;
            if (int.TryParse(request.Headers["X-Page-Id"], out pageId))
            {
                parsedPageId = pageId;
            }

            WmsOperationLogWriter.Write(new WmsOperationLogEntry
            {
                PageId = parsedPageId,
                PageName = request.Headers["X-Page-Name"],
                ButtonKey = request.Headers["X-Button-Key"],
                ButtonName = string.IsNullOrWhiteSpace(request.Headers["X-Button-Name"])
                    ? rule.DefaultButtonName
                    : WmsOperationLogWriter.ReadHeaderValue(request.Headers["X-Button-Name"]),
                Controller = controller,
                Action = action,
                OperationType = rule.OperationType,
                Success = success,
                ErrorMessage = errorMessage,
                RequestData = requestData,
                DataBefore = dataBefore,
                DataAfter = dataAfter,
                OperatorName = HttpContextHelper.GetCurrentUserName(),
                ClientIp = request.UserHostAddress,
                OperateTime = DateTime.Now,
                DurationMs = durationMs
            });
        }

        private static bool TryGetRule(ActionExecutingContext filterContext, out WmsOperationLogRule rule)
        {
            rule = null;
            var routeData = filterContext.RouteData;
            var area = routeData.DataTokens["area"] as string;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            return WmsOperationLogRules.ShouldLog(area, controller, action, out rule);
        }

        private static string MergeExportInfo(string requestData, FileResult fileResult)
        {
            var payload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(requestData))
            {
                payload["query"] = requestData;
            }

            payload["fileName"] = fileResult.FileDownloadName;
            payload["contentType"] = fileResult.ContentType;
            return WmsOperationSnapshotLoader.ToJson(payload);
        }
    }
}
