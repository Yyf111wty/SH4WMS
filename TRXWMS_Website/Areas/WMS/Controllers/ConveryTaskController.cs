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
    public class ConveryTaskController : Controller
    {
        wms_conveyor_taskService service = new wms_conveyor_taskService();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetRowData(wms_conveyor_task WMS_CONVEYOR_TASK)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_conveyor_task> lists = null;
            try
            {
                lists = service.QueryPage(x =>
                     (WMS_CONVEYOR_TASK.SOURCEORDER == null || x.SOURCEORDER.Contains(WMS_CONVEYOR_TASK.SOURCEORDER))
                     //&& (WMS_CONVEYOR_TASK.WAREHOUSECODE == null || x.WAREHOUSECODE.Contains(WMS_CONVEYOR_TASK.WAREHOUSECODE))
                     //&& (WMS_CONVEYOR_TASK.BARCODE == null || x.BARCODE.Contains(WMS_CONVEYOR_TASK.BARCODE))
                     //&& (WMS_CONVEYOR_TASK.BATCHQTY == null || x.BATCHQTY.Contains(WMS_CONVEYOR_TASK.BATCHQTY))
                     && (WMS_CONVEYOR_TASK.CONTAINER == null || x.CONTAINER.Contains(WMS_CONVEYOR_TASK.CONTAINER))
                     && (WMS_CONVEYOR_TASK.STARTTAXI == null || x.STARTTAXI.Contains(WMS_CONVEYOR_TASK.STARTTAXI))
                     && (WMS_CONVEYOR_TASK.ENDTAXI == null || x.ENDTAXI.Contains(WMS_CONVEYOR_TASK.ENDTAXI))
                     //&& (WMS_CONVEYOR_TASK.ITEMCODE == null || x.ITEMCODE.Contains(WMS_CONVEYOR_TASK.ITEMCODE))
                     //&& (WMS_CONVEYOR_TASK.ITEMNAME == null || x.ITEMNAME.Contains(WMS_CONVEYOR_TASK.ITEMNAME))
                     && (WMS_CONVEYOR_TASK.STATE == -10 || x.STATE.Equals(WMS_CONVEYOR_TASK.STATE))
                     ,
                     ref TotalCount, pageId, pageSize, "CREATETIME DESC").ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            //return JsonConvert.SerializeObject(new ContentUI("", rows, 0, total));
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);

        }

       

       
     
    }
}
