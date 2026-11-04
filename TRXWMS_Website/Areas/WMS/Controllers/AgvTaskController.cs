using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRWMS_Website.Lib;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    //[CheckLoginFilter]
    public class AgvTaskController : Controller
    {
        wms_agv_taskService service = new wms_agv_taskService();

        public ActionResult Index()
        {
            return View();
        }
        //public string GetRowData(WMS_AGV_TASK WMS_AGV_TASK)
        //{
        //    int TotalCount = 0;
        //    int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
        //    int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
        //    List<WMS_AGV_TASK> lists = null;
        //    try
        //    {
        //        lists = AgvService.QueryPage(null,
        //            ref TotalCount, pageId, pageSize, null);
        //    }
        //    catch (Exception ee)
        //    {
        //        var a = ee.ToString();
        //    }
        //    var total = TotalCount;
        //    var rows = lists;
        //    return JsonConvert.SerializeObject(new ContentUI("", rows, 0, total));
        //}

        public ActionResult GetRowData(wms_agv_task WMS_AGV_TASK)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_agv_task> lists = null;

            try
            {
                lists = service.QueryPage(x =>
                     
                     (WMS_AGV_TASK.BARCODE == null || x.BARCODE.Contains(WMS_AGV_TASK.BARCODE))
                     && (WMS_AGV_TASK.CONTAINER == null || x.CONTAINER.Contains(WMS_AGV_TASK.CONTAINER))
                     && (WMS_AGV_TASK.STARTTAXI == null || x.STARTTAXI.Contains(WMS_AGV_TASK.STARTTAXI))
                     && (WMS_AGV_TASK.ENDTAXI == null || x.ENDTAXI.Contains(WMS_AGV_TASK.ENDTAXI))
                     && (WMS_AGV_TASK.STATE == -10 || x.STATE.Equals(WMS_AGV_TASK.STATE))
                     ,
                     ref TotalCount, pageId, pageSize, "CREATETIME DESC").ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

      

      
        //[HttpPost]
        //public JsonResult AgvCancel(int ID, string SOURCEORDER, string USER)
        //{
        //    ReslutJson hm = new ReslutJson();
        //    var content = new
        //    {
        //        ID = ID,
        //        SOURCEORDER = SOURCEORDER,
        //        USER = USER
        //    };
        //    hm = HttpRequestHelper.ReslutJsonMethod(InterfaceConfig.AgvCancel, content);
        //    try
        //    {
        //        return Json(new { result = hm.success, message = hm.msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}