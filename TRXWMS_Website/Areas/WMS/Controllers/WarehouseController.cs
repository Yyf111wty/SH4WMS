using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    //[CheckLoginFilter]
    public class WarehouseController : Controller
    {
        wms_base_warehouseService service = new wms_base_warehouseService();
        wms_base_zoneService zoneservice = new wms_base_zoneService();
        //wms_base_locationService locationservice = new wms_base_locationService();
        // GET: WMS/TEST
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRowData(string CODE)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_base_warehouse> lists = null;
            try
            {
                lists = service.QueryPage(a => (a.WAREHOUSECODE.Contains(CODE) || "" == CODE), ref TotalCount, pageId, pageSize, null);
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOrEdit()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddProduct(string WAREHOUSECODE, string FACTORYCODE, string WAREHOUSENAME, int ENTPRINCIPLE, int OUTPRINCIPLE, int ENABLED)
        {
            wms_base_warehouse model = new wms_base_warehouse();
            try
            {
                //仓库编号不能重复
                var  newcount=  service.Query(t => t.WAREHOUSECODE == WAREHOUSECODE).Count();
                if (newcount > 0)
                {
                    return Json(new { result = "编号重复" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = ResultHelper.NewId;
                    model.WAREHOUSECODE = WAREHOUSECODE;
                    model.FACTORYCODE = FACTORYCODE;
                    model.WAREHOUSENAME = WAREHOUSENAME;
                    model.ENTPRINCIPLE = ENTPRINCIPLE;
                    model.OUTPRINCIPLE = OUTPRINCIPLE;
                    model.ENABLED = ENABLED;
                    model.CREATOR = HttpContextHelper.GetCurrentUserName();
                    model.CREATETIME = DateTime.Now;
                    model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                    model.LASTEDITTIME = DateTime.Now;
                    var results = (service.InsertReturnIdentity(model) == 1);
                    return Json(new { result = results }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult EditProduct(string ID, string WAREHOUSECODE, string FACTORYCODE, string WAREHOUSENAME, int ENTPRINCIPLE, int OUTPRINCIPLE, int ENABLED)
        {
            wms_base_warehouse model = new wms_base_warehouse();
            wms_base_warehouse oldmodel = new wms_base_warehouse();
            oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
            try
            {
                //修改后的编号也不可重复     修改后同时更新库区和库位的编号
                var newcount = service.Query(t => t.WAREHOUSECODE==WAREHOUSECODE&t.WAREHOUSECODE!=oldmodel.WAREHOUSECODE).Count();
                if (newcount > 0)
                {
                    return Json(new { result = "编号重复" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = ID;
                    model.WAREHOUSECODE = WAREHOUSECODE;
                    model.FACTORYCODE = FACTORYCODE;
                    model.WAREHOUSENAME = WAREHOUSENAME;

                    model.ENTPRINCIPLE = ENTPRINCIPLE;
                    model.OUTPRINCIPLE = OUTPRINCIPLE;
                    model.ENABLED = ENABLED;
                    model.CREATOR = oldmodel.CREATOR;
                    model.CREATETIME = oldmodel.CREATETIME;
                    model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                    model.LASTEDITTIME = DateTime.Now;                    
                    var results = (service.Update(model)& zoneservice.Update(t => t.WAREHOUSECODE == WAREHOUSECODE, t => t.WAREHOUSECODE == oldmodel.WAREHOUSECODE));
                   
                    return Json(new { result = results }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeleteProduct(string ID)
        {
            wms_base_warehouse model = new wms_base_warehouse();
            var result = true;
            try
            {
                //含有库区信息不能删除
                wms_base_warehouse oldmodel = new wms_base_warehouse();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                var wcode = oldmodel.WAREHOUSECODE;
                 var  zonecount= zoneservice.Query(t=>t.WAREHOUSECODE==wcode).Count();
                if (zonecount > 0)
                {
                    return Json(new { result = "该仓库包含库区信息，不能删除！" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    model.ID = ID;
                    result = (service.Delete(model));
                    return Json(new { result = result }, JsonRequestBehavior.AllowGet);
                }
              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}