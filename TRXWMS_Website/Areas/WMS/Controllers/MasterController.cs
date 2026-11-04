using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
   // [CheckLoginFilter]
    public class MasterController : Controller
    {
        wms_bill_inventorybookService service = new wms_bill_inventorybookService();
        wms_base_masterService msservice = new wms_base_masterService();
        //  wms_base_warehouseService warehouseservice = new wms_base_warehouseService();
        // GET: WMS/TEST
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AddOrEdit()
        {
            return View();
        }

        public ActionResult GetRowData(InventoryKCModel WMS_BASE_MASTER)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<InventoryKCModel> lists = null;
            try
            {
                lists = service.GetKClist(x =>
                     (WMS_BASE_MASTER.ITEMCODE == null || x.ITEMCODE.Equals(WMS_BASE_MASTER.ITEMCODE))
                     && (WMS_BASE_MASTER.KC == null || x.KC.Equals(WMS_BASE_MASTER.KC))
                                 
                     ,
                     ref TotalCount, pageId, pageSize, "ITEMCODE DESC").ToList();
            }
            catch (Exception ee)
            {
                var a = ee.ToString();
            }
            var total = TotalCount;
            var rows = lists;
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AddProduct(string ITEMCODE, string MINSTORAGE)
        {
          
            try
            {
                //编码不能重复
             
                var codecount = msservice.Query(t => t.ITEMCODE == ITEMCODE).Count;
                if (codecount > 0)
                {
                    return Json(new { result = "录入规格信息重复，请重新录入" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    wms_base_master model = new wms_base_master();
                    string id = Guid.NewGuid().ToString("D");
                    model.ID = id;

                    model.ITEMCODE = ITEMCODE;
                    model.MINSTORAGE = int.Parse(MINSTORAGE);
                   
                    model.CREATOR = HttpContextHelper.GetCurrentUserName();
                    model.CREATETIME = DateTime.Now;
                    model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                    model.LASTEDITTIME = DateTime.Now;
                    var results = (msservice.InsertReturnIdentity(model) == 1);
                    return Json(new { result = results }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public JsonResult EditProduct(string ID, string ITEMCODE, string MINSTORAGE)
        {
              wms_base_master model = new wms_base_master();
              wms_base_master oldmodel = new wms_base_master();
            oldmodel = msservice.Query(t => t.ID == ID).FirstOrDefault();
            try
            {
                //var codeid = ZONECODE +"-"+ LOCATIONCODE;
                //var codecount = service.Query(t => t.ID == codeid&t.ID!=oldmodel.ID).Count;
                //if (codecount > 0)
                //{
                //    return Json(new { result = "库区库位信息重复" }, JsonRequestBehavior.AllowGet);
                //}
                model.ID = ID;

                model.ITEMCODE = oldmodel.ITEMCODE;

                model.MINSTORAGE = int.Parse(MINSTORAGE); ;
              

                model.CREATOR = oldmodel.CREATOR;
                model.CREATETIME = oldmodel.CREATETIME;
                model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                model.LASTEDITTIME = DateTime.Now;
                var results = (msservice.Update(model));
                return Json(new { result = results }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeleteProduct(string ID)
        {
            wms_base_master model = new wms_base_master();
            var result = true;
            try
            {
               
                {
                    model.ID = ID;
                    result = (msservice.Delete(model));
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