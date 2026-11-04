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
    //[CheckLoginFilter]
    public class WeighController : Controller
    {
        wms_bill_weighService service = new wms_bill_weighService();
        // GET: WMS/TEST
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRowData(wms_bill_weigh WMS_BILL_WEIGH)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_bill_weigh> lists = null;
            try
            {
                lists = service.QueryPage(x =>
                     (WMS_BILL_WEIGH.SOURCEORDER == null || x.SOURCEORDER.Contains(WMS_BILL_WEIGH.SOURCEORDER))
                     && (WMS_BILL_WEIGH.CONTAINER == null || x.CONTAINER.Contains(WMS_BILL_WEIGH.CONTAINER))                  
                    
                   
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

      

    }
}