using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Newtonsoft.Json;
using SqlSugar;
using TR.Model;
using TR.ORM;
using TR.ORM.Model;
using TR.Service;
using TRXWMS.BILL.DAL;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
	//[CheckLoginFilter]
	[SessionState(SessionStateBehavior.ReadOnly)]
	public class InventoryController : Controller
	{
		// 按请求缓存数据库上下文，避免跨请求共享连接与事务。

		private static DbContext DbContext
		{
			get
			{
				const string cacheKey = "__InventoryController_DbContext__";
				var httpContext = System.Web.HttpContext.Current;
				if (httpContext == null)
				{
					return new DbContext();
				}

				var current = httpContext.Items[cacheKey] as DbContext;
				if (current == null)
				{
					current = new DbContext();
					httpContext.Items[cacheKey] = current;
				}

				return current;
			}
		}
		private static readonly DateTime DefaultDateTime = DateTime.Parse("1900-01-01");

		wms_bill_inventorybookService service = new wms_bill_inventorybookService();

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Getlist(InventorystatisticsModel model)
		{
			int TotalCount = 0;
			int pageSize = 20;
			int pageId = 1;
			int parsedLimit;
			int parsedPage;
			if (!string.IsNullOrEmpty(Request["limit"]) && Request["limit"] != "NaN" && int.TryParse(Request["limit"], out parsedLimit))
			{
				pageSize = parsedLimit;
			}
			if (!string.IsNullOrEmpty(Request["page"]) && Request["page"] != "NaN" && int.TryParse(Request["page"], out parsedPage))
			{
				pageId = parsedPage;
			}
			List<InventorystatisticsModel> lists = null;

			try
			{
				lists = service.Getlist(a => ((String.IsNullOrEmpty(model.SOURCEORDER) || a.SOURCEORDER.Contains(model.SOURCEORDER))
				& (String.IsNullOrEmpty(model.CONTAINER) || a.CONTAINER == model.CONTAINER)
				& (String.IsNullOrEmpty(model.LOCATIONSTATUS) || a.LOCATIONSTATUS == model.LOCATIONSTATUS)
				& (String.IsNullOrEmpty(model.LOCATIONCODE) || a.LOCATIONCODE == model.LOCATIONCODE)
				& (String.IsNullOrEmpty(model.ROWNUM) || a.ROWNUM == model.ROWNUM)
				& (String.IsNullOrEmpty(model.COLUMNNUM) || a.COLUMNNUM == model.COLUMNNUM)
                & (String.IsNullOrEmpty(model.TUNNELNUM) || a.TUNNELNUM == model.TUNNELNUM)
                     && (model.startTime == null || model.startTime <= a.LASTEDITTIME)
                     && (model.endTime == null || model.endTime >= a.LASTEDITTIME)
				& (String.IsNullOrEmpty(model.LAYERNUM) || a.LAYERNUM == model.LAYERNUM)), ref TotalCount, pageId, pageSize, BuildInventoryOrderBy(Request["field"], Request["order"]));
			}
			catch (Exception ee)
			{
				return Json(new { total = 0, rows = new List<InventorystatisticsModel>(), msg = ee.Message }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { total = TotalCount, rows = lists ?? new List<InventorystatisticsModel>() }, JsonRequestBehavior.AllowGet);
		}

		private static string BuildInventoryOrderBy(string field, string order)
		{
			if (string.Equals(field, "LASTEDITTIME", StringComparison.OrdinalIgnoreCase))
			{
				return "LASTEDITTIME " + (string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
			}

			return "LOCATIONCODE";
		}

		/// <summary>
		/// 获取可选出库终点（出库口）
		/// </summary>
		public JsonResult GetOutTargetList()
		{
			try
			{
				var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var taskTargets = DbContext.Db.Ado.SqlQuery<string>(@"
SELECT DISTINCT ENDTAXI AS code
FROM wms_stocker_task
WHERE TYPE = 2 AND ENDTAXI IS NOT NULL AND ENDTAXI <> ''");
				foreach (var code in taskTargets)
				{
					if (!string.IsNullOrWhiteSpace(code))
					{
						codes.Add(code.Trim());
					}
				}

				var headerTargets = DbContext.Db.Ado.SqlQuery<string>(@"
SELECT DISTINCT TARGET AS code
FROM wms_bill_putoutheader
WHERE TARGET IS NOT NULL AND TARGET <> ''");
				foreach (var code in headerTargets)
				{
					if (!string.IsNullOrWhiteSpace(code))
					{
						codes.Add(code.Trim());
					}
				}

				var zoneTargets = DbContext.Db.Ado.SqlQuery<string>(@"
SELECT DISTINCT OUTENTRANCE AS code
FROM wms_base_zone
WHERE ENABLED = 1 AND OUTENTRANCE IS NOT NULL AND OUTENTRANCE <> ''");
				foreach (var code in zoneTargets)
				{
					if (!string.IsNullOrWhiteSpace(code))
					{
						codes.Add(code.Trim());
					}
				}

				codes.Add("1001");
				codes.Add("1002");

				var data = codes
					.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
					.Select(x => new OutTargetOption { code = x, name = x })
					.ToList();

				return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// 库存报表一键叫箱：批量生成出库任务
		/// </summary>
		[HttpPost]
		public JsonResult CreateCallBoxTasks(string target, string items)
		{
			var stopwatch = Stopwatch.StartNew();
			var successList = new List<CallBoxSuccessDetail>();
			var skipList = new List<CallBoxSkipDetail>();

			if (string.IsNullOrWhiteSpace(target))
			{
				return CallBoxResult(false, "请选择出库终点", target, items, successList, skipList, stopwatch);
			}

			List<InventoryCallBoxItem> requestItems;
			try
			{
				requestItems = JsonConvert.DeserializeObject<List<InventoryCallBoxItem>>(items ?? "[]");
			}
			catch
			{
				return CallBoxResult(false, "提交数据格式错误", target, items, successList, skipList, stopwatch);
			}

			if (requestItems == null || requestItems.Count == 0)
			{
				return CallBoxResult(false, "请至少选择一条有库存的数据", target, items, successList, skipList, stopwatch);
			}

			target = target.Trim();
			string creator = HttpContextHelper.GetCurrentUserName();
			if (string.IsNullOrWhiteSpace(creator))
			{
				creator = "WMS";
			}

			var now = DateTime.Now;

			try
			{
				SqlSugarClientFactory.BeginTran();

				foreach (var item in requestItems)
				{
					if (item == null
						|| string.IsNullOrWhiteSpace(item.LOCATIONCODE)
						|| string.IsNullOrWhiteSpace(item.CONTAINER))
					{
						AddCallBoxSkip(skipList, item?.CONTAINER, item?.LOCATIONCODE, "参数不完整");
						continue;
					}

					string locationCode = item.LOCATIONCODE.Trim();
					string container = item.CONTAINER.Trim();
					string sourceOrder = (item.SOURCEORDER ?? "").Trim();
					string zoneCode = string.IsNullOrWhiteSpace(item.ZONECODE) ? "101" : item.ZONECODE.Trim();

					var location = DbContext.Db.Queryable<wms_base_location>()
						.First(x => x.LOCATIONCODE == locationCode && x.ZONECODE == zoneCode);
					if (location == null)
					{
						AddCallBoxSkip(skipList, container, locationCode, "库位不存在");
						continue;
					}
					if (location.ISAVIALABLE != 0 || string.IsNullOrEmpty(location.CONTAINER))
					{
						AddCallBoxSkip(skipList, container, locationCode, "库位无货");
						continue;
					}
					if (!string.Equals(location.CONTAINER, container, StringComparison.OrdinalIgnoreCase))
					{
						AddCallBoxSkip(skipList, container, locationCode, "载具与库位不一致");
						continue;
					}
					if (location.OUTENABLED != 1)
					{
						AddCallBoxSkip(skipList, container, locationCode, "库位不可出库或已有待出库任务");
						continue;
					}
					if (string.IsNullOrEmpty(sourceOrder))
					{
						sourceOrder = location.SOURCEORDER ?? "";
					}

					int pendingPutout = DbContext.Db.Ado.GetInt(@"
SELECT COUNT(1) FROM wms_bill_putoutheader
WHERE LOCATIONCODE = @LOCATIONCODE AND ENABLED = 1 AND STATE IN (0, 1)",
						new List<SugarParameter> { new SugarParameter("@LOCATIONCODE", locationCode) });
					if (pendingPutout > 0)
					{
						AddCallBoxSkip(skipList, container, locationCode, "已有未完成出库单");
						continue;
					}

					int pendingTask = DbContext.Db.Ado.GetInt(@"
SELECT COUNT(1) FROM wms_stocker_task
WHERE STARTTAXI = @LOCATIONCODE AND TYPE = 2 AND STATE IN (0, 1)",
						new List<SugarParameter> { new SugarParameter("@LOCATIONCODE", locationCode) });
					if (pendingTask > 0)
					{
						AddCallBoxSkip(skipList, container, locationCode, "已有未完成堆垛机出库任务");
						continue;
					}

					int updated = DbContext.Db.Ado.ExecuteCommand(@"
UPDATE wms_base_location
SET OUTENABLED = 0, LASTEDITOR = @CREATOR, LASTEDITTIME = NOW()
WHERE ZONECODE = @ZONECODE AND LOCATIONCODE = @LOCATIONCODE AND OUTENABLED = 1",
						new List<SugarParameter>
						{
							new SugarParameter("@CREATOR", creator),
							new SugarParameter("@ZONECODE", zoneCode),
							new SugarParameter("@LOCATIONCODE", locationCode)
						});
					if (updated <= 0)
					{
						AddCallBoxSkip(skipList, container, locationCode, "锁定库位失败");
						continue;
					}

					string headerId = Guid.NewGuid().ToString("D");
					string bodyId = Guid.NewGuid().ToString("D");
					string taskId = Guid.NewGuid().ToString("D");
					string containerType = string.IsNullOrWhiteSpace(location.CONTAINERTYPE) ? "0" : location.CONTAINERTYPE;

					DbContext.Db.Insertable(new wms_bill_putoutheader
					{
						ID = headerId,
						ZONECODE = zoneCode,
						SOURCEORDER = sourceOrder,
						TASKTYPE = "",
						CONTAINER = container,
						CONTAINERTYPE = containerType,
						QUESTFLAG = 0,
						QUESTTIME = DefaultDateTime,
						FINISHTIME = DefaultDateTime,
						STATE = 0,
						TARGET = target,
						LOCATIONCODE = locationCode,
						ENABLED = 1,
						REMARK = "库存报表叫箱",
						CREATOR = creator,
						CREATETIME = now,
						LASTEDITOR = "",
						LASTEDITTIME = DefaultDateTime,
						SKUSEQ = 0,
						ORDERID = null
					}).ExecuteCommand();

					DbContext.Db.Insertable(new wms_bill_putoutbody
					{
						ID = bodyId,
						SOURCEORDER = sourceOrder,
						CONTAINER = container,
						ITEMCODE = sourceOrder,
						LOT = "",
						QTY = 0,
						CREATOR = creator,
						CREATETIME = now,
						LASTEDITOR = "",
						LASTEDITTIME = DefaultDateTime,
						HEADERID = headerId,
						WEIGHT = "",
						TASK_NO_SN = ""
					}).ExecuteCommand();

					DbContext.Db.Insertable(new wms_stocker_task
					{
						ID = taskId,
						CODE = taskId,
						SOURCEORDER = sourceOrder,
						ZONECODE = zoneCode,
						CONTAINER = container,
						TYPE = 2,
						PRIORITY = 1,
						STARTTAXI = locationCode,
						ENDTAXI = target,
						STATE = 0,
						STARTTIME = now,
						IMPLEMENTTIME = DefaultDateTime,
						FINISHTIME = DefaultDateTime,
						CREATOR = creator,
						CREATETIME = now,
						LASTEDITOR = creator,
						LASTEDITTIME = now,
						SKUSEQ = 0,
						TASKMODE = "NORMAL",
						PALLET = "",
						ORDERINFO = "CALLBOX|" + locationCode + "|" + container,
						WORKNAME = null
					}).ExecuteCommand();

					successList.Add(new CallBoxSuccessDetail
					{
						CONTAINER = container,
						LOCATIONCODE = locationCode,
						SOURCEORDER = sourceOrder,
						TASKID = taskId
					});
				}

				if (successList.Count == 0)
				{
					SqlSugarClientFactory.RollbackTran();
					return CallBoxResult(false, "未生成任何出库任务", target, items, successList, skipList, stopwatch);
				}

				SqlSugarClientFactory.CommitTran();
				return CallBoxResult(
					true,
					"成功生成 " + successList.Count + " 条出库任务",
					target,
					items,
					successList,
					skipList,
					stopwatch);
			}
			catch (Exception ex)
			{
				SqlSugarClientFactory.RollbackTran();
				return CallBoxResult(false, ex.Message, target, items, successList, skipList, stopwatch, ex.Message);
			}
		}

		private JsonResult CallBoxResult(
			bool success,
			string message,
			string target,
			string items,
			List<CallBoxSuccessDetail> successList,
			List<CallBoxSkipDetail> skipList,
			Stopwatch stopwatch,
			string errorMessage = null)
		{
			WriteCallBoxOperationLog(success, message, target, items, successList, skipList, stopwatch, errorMessage);

			return Json(new
			{
				success,
				message,
				count = successList.Count,
				locations = successList.Select(x => x.LOCATIONCODE).ToList(),
				successItems = successList,
				skipped = skipList
			});
		}

		private static void AddCallBoxSkip(List<CallBoxSkipDetail> skipList, string container, string locationCode, string reason)
		{
			skipList.Add(new CallBoxSkipDetail
			{
				CONTAINER = container ?? "",
				LOCATIONCODE = locationCode ?? "",
				REASON = reason
			});
		}

		private static string GetCallBoxTargetName(string target)
		{
			if (target == "1001")
			{
				return "大钻出库";
			}

			if (target == "1002")
			{
				return "小钻出库";
			}

			return target ?? "";
		}

		private void WriteCallBoxOperationLog(
			bool success,
			string message,
			string target,
			string items,
			List<CallBoxSuccessDetail> successList,
			List<CallBoxSkipDetail> skipList,
			Stopwatch stopwatch,
			string errorMessage = null)
		{
			stopwatch.Stop();

			int? pageId = null;
			int parsedPageId;
			if (int.TryParse(Request.Headers["X-Page-Id"], out parsedPageId))
			{
				pageId = parsedPageId;
			}

			var dataAfter = new
			{
				target,
				targetName = GetCallBoxTargetName(target),
				summary = new
				{
					successCount = successList.Count,
					skipCount = skipList.Count,
					total = successList.Count + skipList.Count
				},
				message,
				success = successList,
				skipped = skipList
			};

			WmsOperationLogWriter.Write(new WmsOperationLogEntry
			{
				PageId = pageId,
				PageName = Request.Headers["X-Page-Name"],
				ButtonKey = string.IsNullOrWhiteSpace(Request.Headers["X-Button-Key"])
					? "btnCallBox"
					: Request.Headers["X-Button-Key"],
				ButtonName = string.IsNullOrWhiteSpace(Request.Headers["X-Button-Name"])
					? "一键叫箱"
					: WmsOperationLogWriter.ReadHeaderValue(Request.Headers["X-Button-Name"]),
				Controller = "Inventory",
				Action = "CreateCallBoxTasks",
				OperationType = "业务",
				Success = success,
				ErrorMessage = errorMessage ?? (success ? null : message),
				RequestData = JsonConvert.SerializeObject(new { target, items }),
				DataAfter = JsonConvert.SerializeObject(dataAfter),
				OperatorName = HttpContextHelper.GetCurrentUserName(),
				ClientIp = Request.UserHostAddress,
				OperateTime = DateTime.Now,
				DurationMs = (int)stopwatch.ElapsedMilliseconds
			});
		}

        public ActionResult GetGGlist(InventoryGGModel model)
        {
            int TotalCount = 0;
            int pageSize;
            int pageId;
            if (!int.TryParse(Request["limit"], out pageSize) || pageSize <= 0) pageSize = 20;
            if (!int.TryParse(Request["page"], out pageId) || pageId <= 0) pageId = 1;

            try
            {
                var summary = new InventoryGGModel();
                var lists = service.GetGGlist(model.SOURCEORDER, ref TotalCount, pageId, pageSize, "SOURCEORDER", summary);
                return Json(new { total = TotalCount, rows = lists ?? new List<InventoryGGModel>(), summary }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message)
                    ? ex.Message + " (" + ex.InnerException.Message + ")"
                    : ex.Message;
                return Json(new { total = 0, rows = new List<InventoryGGModel>(), msg = detail }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetExportlist(InventorystatisticsModel model)
        {
            int TotalCount = 0;

            List<InventorystatisticsModel> lists = service.Getlist(
                a => ((String.IsNullOrEmpty(model.SOURCEORDER) || a.SOURCEORDER.Contains(model.SOURCEORDER))
                & (String.IsNullOrEmpty(model.CONTAINER) || a.CONTAINER == model.CONTAINER)
                & (String.IsNullOrEmpty(model.LOCATIONSTATUS) || a.LOCATIONSTATUS == model.LOCATIONSTATUS)
                & (String.IsNullOrEmpty(model.LOCATIONCODE) || a.LOCATIONCODE == model.LOCATIONCODE)
                & (String.IsNullOrEmpty(model.ROWNUM) || a.ROWNUM == model.ROWNUM)
                & (String.IsNullOrEmpty(model.COLUMNNUM) || a.COLUMNNUM == model.COLUMNNUM)
                & (String.IsNullOrEmpty(model.TUNNELNUM) || a.TUNNELNUM == model.TUNNELNUM)
                & (String.IsNullOrEmpty(model.LAYERNUM) || a.LAYERNUM == model.LAYERNUM)),
                ref TotalCount,
                1,
                9999999,
                "LOCATIONCODE");
            lists.ForEach(item=>{
                item.LOCATIONCODE = "'" + item.LOCATIONCODE;
            });
            return new JsonResult
            {
                Data = new
                {
                    total = TotalCount,
                    rows = lists
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }

        public ActionResult GetExportlistold(InventorystatisticsModel model)
		{
			int TotalCount = 0;
			List<InventorystatisticsModel> lists = null;
			try
			{
                lists = service.Getlist(a => ((String.IsNullOrEmpty(model.SOURCEORDER) || a.SOURCEORDER.Contains(model.SOURCEORDER))
                & (String.IsNullOrEmpty(model.CONTAINER) || a.CONTAINER == model.CONTAINER)
                & (String.IsNullOrEmpty(model.LOCATIONSTATUS) || a.LOCATIONSTATUS == model.LOCATIONSTATUS)
                & (String.IsNullOrEmpty(model.LOCATIONCODE) || a.LOCATIONCODE == model.LOCATIONCODE)
                & (String.IsNullOrEmpty(model.ROWNUM) || a.ROWNUM == model.ROWNUM)
                & (String.IsNullOrEmpty(model.COLUMNNUM) || a.COLUMNNUM == model.COLUMNNUM)
                & (String.IsNullOrEmpty(model.TUNNELNUM) || a.TUNNELNUM == model.TUNNELNUM)
                & (String.IsNullOrEmpty(model.LAYERNUM) || a.LAYERNUM == model.LAYERNUM)), ref TotalCount, 1, 9999999, "LOCATIONCODE");
			}
			catch (Exception ee)
			{
				var a = ee.ToString();
			}
			return Json(new { total = TotalCount, rows = lists }, JsonRequestBehavior.AllowGet);
		}
	}
}
