using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using TR.Model;
using TRWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
	public class BdjInventoryController : Controller
	{
		private static readonly string ApiBaseUrl =
			ConfigurationManager.AppSettings["bdjWarehouseApiUrl"] ?? "http://localhost:8081";

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

		[HttpGet]
		public ActionResult Getlist(BdjWarehouseLocationQueryModel model)
		{
			int pageIndex = 1;
			int pageSize = 20;
			int parsedPage;
			int parsedLimit;
			if (!string.IsNullOrEmpty(Request["page"]) && Request["page"] != "NaN" && int.TryParse(Request["page"], out parsedPage))
			{
				pageIndex = parsedPage;
			}
			if (!string.IsNullOrEmpty(Request["limit"]) && Request["limit"] != "NaN" && int.TryParse(Request["limit"], out parsedLimit))
			{
				pageSize = parsedLimit;
			}

			try
			{
				var request = BuildQueryRequest(model, pageIndex, pageSize);
				var url = ApiBaseUrl.TrimEnd('/') + "/api/warehouse/locations/query";
				var json = HttpRequestPost.HttpPostMethod(url, request, true);
				var response = JsonConvert.DeserializeObject<BdjApiResponse<BdjPagedResult<BdjWarehouseLocationDto>>>(json);

				if (response == null)
				{
					return Utf8Json(new { total = 0, rows = new List<BdjWarehouseLocationDto>(), msg = "\u63a5\u53e3\u8fd4\u56de\u4e3a\u7a7a" });
				}

				if (!response.Success || response.Result == null)
				{
					return Utf8Json(new
					{
						total = 0,
						rows = new List<BdjWarehouseLocationDto>(),
						msg = string.IsNullOrWhiteSpace(response.Message) ? "\u67e5\u8be2\u5931\u8d25" : response.Message
					});
				}

				var items = response.Result.Items ?? new List<BdjWarehouseLocationDto>();
				return Utf8Json(new
				{
					total = response.Result.TotalCount,
					rows = items,
					msg = response.Message
				});
			}
			catch (Exception ex)
			{
				return Utf8Json(new
				{
					total = 0,
					rows = new List<BdjWarehouseLocationDto>(),
					msg = "\u8c03\u7528\u5907\u5200\u5e93\u4f4d\u63a5\u53e3\u5931\u8d25\uff1a" + ex.Message
				});
			}
		}

		private JsonResult Utf8Json(object data)
		{
			return new JsonResult
			{
				Data = data,
				ContentType = "application/json; charset=utf-8",
				ContentEncoding = Encoding.UTF8,
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}

		private static Dictionary<string, object> BuildQueryRequest(
			BdjWarehouseLocationQueryModel model,
			int pageIndex,
			int pageSize)
		{
			model = model ?? new BdjWarehouseLocationQueryModel();
			var request = new Dictionary<string, object>
			{
				{ "PageIndex", pageIndex < 1 ? 1 : pageIndex },
				{ "PageSize", pageSize < 1 ? 20 : (pageSize > 200 ? 200 : pageSize) }
			};

			if (model.warehouseId.HasValue)
			{
				request["WarehouseId"] = model.warehouseId.Value;
			}
			if (!string.IsNullOrWhiteSpace(model.diameter))
			{
				request["Diameter"] = model.diameter.Trim();
			}
			if (!string.IsNullOrWhiteSpace(model.qrCode))
			{
				request["QRCode"] = model.qrCode.Trim();
			}
			if (model.isExistBox.HasValue)
			{
				request["IsExistBox"] = model.isExistBox.Value;
			}
			if (model.storey.HasValue)
			{
				request["Storey"] = model.storey.Value;
			}
			if (model.col.HasValue)
			{
				request["Col"] = model.col.Value;
			}
			if (model.grindCount.HasValue)
			{
				request["GrindCount"] = model.grindCount.Value;
			}
			if (model.minRemain.HasValue)
			{
				request["MinRemain"] = model.minRemain.Value;
			}
			if (model.maxRemain.HasValue)
			{
				request["MaxRemain"] = model.maxRemain.Value;
			}
			if (model.startTime.HasValue)
			{
				request["StartTime"] = model.startTime.Value;
			}
			if (model.endTime.HasValue)
			{
				request["EndTime"] = model.endTime.Value;
			}

			return request;
		}
	}
}
