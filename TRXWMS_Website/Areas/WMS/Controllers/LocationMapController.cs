using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [CheckLoginFilter]
    public class LocationMapController : Controller
    {
        private const int MaxRow = 8;
        private const int MaxCol = 144;
        private const int MaxLayer = 17;

        private readonly wms_base_locationService _locationService = new wms_base_locationService();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
            base.OnActionExecuting(filterContext);
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

        public ActionResult Index()
        {
            return RedirectToAction("Canvas");
        }

        public ActionResult Canvas()
        {
            return View();
        }

        public JsonResult RegisterMenu()
        {
            var message = LocationMapMenuRegistrar.Register();
            return Utf8Json(new { success = message.IndexOf("\u5931\u8d25", StringComparison.Ordinal) < 0, message });
        }

        /// <summary>
        /// 按排展示：Y轴=层(高到低)，X轴=列。未填排范围时按巷道取对应排。
        /// </summary>
        public JsonResult GetRowMatrix(int tunnelNum = 1, int rowMin = 0, int rowMax = 0, int columnMin = 0, int columnMax = 0)
        {
            string rangeError;
            var useRowRange = rowMin > 0 || rowMax > 0;
            if (useRowRange)
            {
                if (!TryNormalizeRange(rowMin, rowMax, 1, MaxRow, out rowMin, out rowMax, out rangeError))
                {
                    return Utf8Json(new { success = false, message = "\u6392" + rangeError });
                }
            }

            if (columnMin > 0 || columnMax > 0)
            {
                if (!TryNormalizeRange(columnMin, columnMax, 1, MaxCol, out columnMin, out columnMax, out rangeError))
                {
                    return Utf8Json(new { success = false, message = "\u5217" + rangeError });
                }
            }
            else
            {
                columnMin = 1;
                columnMax = MaxCol;
            }

            var rows = ResolveRows(tunnelNum, useRowRange, rowMin, rowMax);
            if (rows.Count == 0)
            {
                return Utf8Json(new { success = false, message = "\u8bf7\u9009\u62e9\u6392\u6216\u5df7\u9053" });
            }

            var locations = QueryLocationsByRows(rows, columnMin, columnMax);
            int maxLayer = MaxLayer;
            if (locations.Count > 0)
            {
                maxLayer = locations.Max(x => x.LAYERNUM);
            }

            var cells = locations.Select(loc => new
            {
                row = loc.ROWNUM,
                layer = loc.LAYERNUM,
                col = loc.COLUMNNUM,
                s = ComputeStatus(loc),
                id = loc.ID,
                lc = loc.LOCATIONCODE,
                ct = loc.CONTAINER ?? string.Empty
            }).ToList();

            return Utf8Json(new
            {
                success = true,
                data = new
                {
                    tunnelNum,
                    rowMin = useRowRange ? rowMin : 0,
                    rowMax = useRowRange ? rowMax : 0,
                    columnMin,
                    columnMax,
                    rows,
                    maxLayer,
                    stats = BuildStats(locations),
                    cells
                }
            });
        }

        public JsonResult GetCellDetail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Utf8Json(new { success = false, message = "\u5e93\u4f4dID\u4e0d\u80fd\u4e3a\u7a7a" });
            }

            var loc = _locationService.Query(x => x.ID == id).FirstOrDefault();
            if (loc == null)
            {
                return Utf8Json(new { success = false, message = "\u5e93\u4f4d\u4e0d\u5b58\u5728" });
            }

            var status = ComputeStatus(loc);
            return Utf8Json(new
            {
                success = true,
                data = new
                {
                    loc.ID,
                    loc.ZONECODE,
                    loc.LOCATIONCODE,
                    loc.ROWNUM,
                    loc.COLUMNNUM,
                    loc.LAYERNUM,
                    loc.TUNNELNUM,
                    loc.CONTAINER,
                    loc.CONTAINERTYPE,
                    loc.SOURCEORDER,
                    loc.ENABLED,
                    loc.ISAVIALABLE,
                    loc.OUTENABLED,
                    status,
                    statusName = GetStatusName(status)
                }
            });
        }

        private static bool TryNormalizeRange(int min, int max, int boundMin, int boundMax, out int nMin, out int nMax, out string error)
        {
            nMin = min;
            nMax = max;
            error = null;

            if (min <= 0) nMin = max;
            if (max <= 0) nMax = min;

            if (nMin < boundMin || nMax > boundMax)
            {
                error = "\u8303\u56f4\u5fc5\u987b\u5728 " + boundMin + "-" + boundMax + " \u4e4b\u95f4";
                return false;
            }

            if (nMin > nMax)
            {
                error = "\u6700\u5c0f\u503c\u4e0d\u80fd\u5927\u4e8e\u6700\u5927\u503c";
                return false;
            }

            return true;
        }

        private static List<int> ResolveRows(int tunnelNum, bool useRowRange, int rowMin, int rowMax)
        {
            if (useRowRange)
            {
                return Enumerable.Range(rowMin, rowMax - rowMin + 1).ToList();
            }

            if (tunnelNum <= 0)
            {
                return new List<int>();
            }

            int startRow = (tunnelNum - 1) * 2 + 1;
            return new List<int> { startRow, startRow + 1 };
        }

        private List<wms_base_location> QueryLocationsByRows(IEnumerable<int> rows, int columnMin, int columnMax)
        {
            var rowList = rows.ToList();
            var zoneCode = GetUserZoneFilter();
            return _locationService.Query(x =>
                (zoneCode == null || x.ZONECODE == zoneCode)
                && rowList.Contains(x.ROWNUM)
                && x.COLUMNNUM >= columnMin
                && x.COLUMNNUM <= columnMax).ToList();
        }

        private string GetUserZoneFilter()
        {
            if (Session["username"] == null)
            {
                return null;
            }

            var uid = Session["username"].ToString().ToLower();
            if (uid == "ycl") return "101-1";
            if (uid == "bcp") return "201-1";
            if (uid == "cp") return "301-1";
            if (uid == "lck") return "401-1";
            return null;
        }

        private static int ComputeStatus(wms_base_location loc)
        {
            if (loc.ENABLED == 0) return 5;
            if (loc.OUTENABLED == 1) return 3;
            if (loc.OUTENABLED == 2) return 4;
            if (loc.ISAVIALABLE == 0 || !string.IsNullOrWhiteSpace(loc.CONTAINER)) return 2;
            return 1;
        }

        private static string GetStatusName(int status)
        {
            switch (status)
            {
                case 1: return "\u7a7a\u95f2";
                case 2: return "\u5360\u7528";
                case 3: return "\u5165\u5e93\u5360\u7528";
                case 4: return "\u51fa\u5e93\u5360\u7528";
                case 5: return "\u7981\u7528";
                default: return "\u65e0\u6570\u636e";
            }
        }

        private static object BuildStats(IEnumerable<wms_base_location> locations)
        {
            var list = locations.ToList();
            int empty = 0, occupied = 0, inbound = 0, outbound = 0, disabled = 0;
            foreach (var loc in list)
            {
                switch (ComputeStatus(loc))
                {
                    case 1: empty++; break;
                    case 2: occupied++; break;
                    case 3: inbound++; break;
                    case 4: outbound++; break;
                    case 5: disabled++; break;
                }
            }

            return new
            {
                total = list.Count,
                empty,
                occupied,
                inbound,
                outbound,
                disabled
            };
        }
    }
}
