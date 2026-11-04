using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
   // [CheckLoginFilter]
    public class LogController : Controller
    {
        wms_logService service = new wms_logService();
        // GET: WMS/Log
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRowData(wms_log SYS_LOG)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_log> lists = null;
            try
            {
                lists = service.QueryPage(x =>
                     (SYS_LOG.Message == null || x.Message.Contains(SYS_LOG.Message))
                     && (SYS_LOG.Result == null || x.Result.Contains(SYS_LOG.Result))
                     && (SYS_LOG.Type == null || x.Type.Contains(SYS_LOG.Type))
                     ,
                     ref TotalCount, pageId, pageSize, "CreateTime DESC").ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }
    }
}