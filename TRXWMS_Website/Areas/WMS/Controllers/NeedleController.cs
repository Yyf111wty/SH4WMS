//-----------------------------------------------------------------------
// <copyright file="NeedleController.cs" company="xxxx Enterprises">
// * Copyright (C) 2026 xxxx Enterprises All Rights Reserved
// * FileName: NeedleController.cs
// * history : 单针改造·查询页面批次（单针追溯查询 + 盒内针明细）
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.SessionState;
using TR.ORM.Model;
using TR.Service;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class NeedleController : Controller
    {
        private readonly wms_needle_inventoryService _inventoryService = new wms_needle_inventoryService();
        private readonly wms_needle_transactionService _transactionService = new wms_needle_transactionService();
        private readonly wms_base_locationService _locationService = new wms_base_locationService();

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

        public ActionResult Container()
        {
            return View();
        }

        /// <summary>
        /// 单针分页查询：针号/盒号/规格模糊，状态精确；仅有效行(ENABLED=1)
        /// </summary>
        public ActionResult GetRowData(string NEEDLENO, string CONTAINER, string ITEMCODE, string STATUS)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageId = ParsePageId(Request["page"]);
            var needleNo = NEEDLENO ?? "";
            var container = CONTAINER ?? "";
            var itemCode = ITEMCODE ?? "";
            var status = STATUS ?? "";
            List<wms_needle_inventory> lists = null;

            try
            {
                // 空关键字不再拼成 LIKE '%%'：709k 行的针表上 LIKE '%%' 会导致全表回表扫描（实测 5 秒）。
                // 条件为空时直接省略该过滤，语义与原来完全一致（原写法为空串时恒为真）。
                lists = _inventoryService.QueryPage(
                    BuildInventoryFilter(needleNo, container, itemCode, status),
                    ref totalCount, pageId, pageSize, BuildOrderBy(Request["field"], Request["order"]));
            }
            catch (Exception ex)
            {
                return Utf8Json(new { total = 0, rows = new List<wms_needle_inventory>(), error = ex.Message });
            }

            return Utf8Json(new { total = totalCount, rows = lists ?? new List<wms_needle_inventory>() });
        }

        /// <summary>
        /// 组合查询条件：只把用户真实填写的关键字加入 SQL，避免生成 LIKE '%%'。
        /// </summary>
        private static System.Linq.Expressions.Expression<Func<wms_needle_inventory, bool>> BuildInventoryFilter(
            string needleNo, string container, string itemCode, string status)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(wms_needle_inventory), "x");
            System.Linq.Expressions.Expression body = System.Linq.Expressions.Expression.Equal(
                System.Linq.Expressions.Expression.Property(parameter, "ENABLED"),
                System.Linq.Expressions.Expression.Constant(1));

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            AppendContains(ref body, parameter, containsMethod, "NEEDLENO", needleNo);
            AppendContains(ref body, parameter, containsMethod, "CONTAINER", container);
            AppendContains(ref body, parameter, containsMethod, "ITEMCODE", itemCode);

            if (!string.IsNullOrEmpty(status))
            {
                body = System.Linq.Expressions.Expression.AndAlso(
                    body,
                    System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, "STATUS"),
                        System.Linq.Expressions.Expression.Constant(status)));
            }

            return System.Linq.Expressions.Expression.Lambda<Func<wms_needle_inventory, bool>>(body, parameter);
        }

        private static void AppendContains(
            ref System.Linq.Expressions.Expression body,
            System.Linq.Expressions.ParameterExpression parameter,
            System.Reflection.MethodInfo containsMethod,
            string propertyName,
            string keyword)
        {
            if (string.IsNullOrEmpty(keyword) || containsMethod == null)
            {
                return;
            }

            var call = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Property(parameter, propertyName),
                containsMethod,
                System.Linq.Expressions.Expression.Constant(keyword));

            body = System.Linq.Expressions.Expression.AndAlso(body, call);
        }

        /// <summary>
        /// 盒内针明细：整盒槽位行（按槽位升序）+ 盒当前库位
        /// </summary>
        public ActionResult GetContainerData(string container)
        {
            var ctn = container ?? "";
            if (string.IsNullOrWhiteSpace(ctn))
            {
                return Utf8Json(new { success = false, error = "盒号不能为空", rows = new List<wms_needle_inventory>() });
            }

            int totalCount = 0;
            try
            {
                var rows = _inventoryService.QueryPage(x =>
                        x.CONTAINER == ctn && x.ENABLED == 1,
                    ref totalCount, 1, 1000, "POSITIONCODE ASC");

                var location = _locationService.Query(x => x.CONTAINER == ctn).FirstOrDefault();

                return Utf8Json(new
                {
                    success = true,
                    container = ctn,
                    locationcode = location == null ? "" : location.LOCATIONCODE,
                    rows = rows ?? new List<wms_needle_inventory>()
                });
            }
            catch (Exception ex)
            {
                return Utf8Json(new { success = false, error = ex.Message, rows = new List<wms_needle_inventory>() });
            }
        }

        /// <summary>
        /// 单针履历：按针号倒序取最近 300 条流转记录
        /// </summary>
        public ActionResult GetHistory(string needleNo)
        {
            var no = needleNo ?? "";
            if (string.IsNullOrWhiteSpace(no))
            {
                return Utf8Json(new { success = false, error = "针号不能为空", rows = new List<wms_needle_transaction>() });
            }

            int totalCount = 0;
            try
            {
                var rows = _transactionService.QueryPage(x => x.NEEDLENO == no,
                    ref totalCount, 1, 300, "CREATETIME DESC");

                return Utf8Json(new { success = true, total = totalCount, rows = rows ?? new List<wms_needle_transaction>() });
            }
            catch (Exception ex)
            {
                return Utf8Json(new { success = false, error = ex.Message, rows = new List<wms_needle_transaction>() });
            }
        }

        private static string BuildOrderBy(string field, string order)
        {
            if (string.Equals(field, "CREATETIME", StringComparison.OrdinalIgnoreCase))
            {
                return "CREATETIME " + (string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
            }
            if (string.Equals(field, "POSITIONCODE", StringComparison.OrdinalIgnoreCase))
            {
                return "POSITIONCODE " + (string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
            }

            return "LASTEDITTIME " + (string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC");
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
