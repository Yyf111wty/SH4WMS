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
    public class ZoneController : Controller
    {
        wms_base_zoneService service = new wms_base_zoneService();
        wms_base_warehouseService warehouse = new wms_base_warehouseService();
        wms_base_locationService location = new wms_base_locationService();
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
            List<wms_base_zone> lists = null;
            try
            {
                lists = service.QueryPage(a => (a.ZONECODE.Contains(CODE) || "" == CODE), ref TotalCount, pageId, pageSize, null);
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
        public JsonResult AddProduct(string ZONECODE, string ZONENAME, string WAREHOUSECODE, string OUTENTRANCE, string INENTRANCE, int ENABLED)
        {
            wms_base_zone model = new wms_base_zone();
            try
            {
                var newcount = service.Query(t => t.ZONECODE == ZONECODE).Count();
                if (newcount > 0)
                {
                    return Json(new { result = "编号重复" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = ResultHelper.NewId;
                    model.ZONECODE = ZONECODE;
                    model.ZONENAME = ZONENAME;
                    model.WAREHOUSECODE = WAREHOUSECODE;
                    model.OUTENTRANCE = OUTENTRANCE;
                    model.INENTRANCE = INENTRANCE;
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
        public JsonResult EditProduct(string ID, string ZONECODE, string ZONENAME, string WAREHOUSECODE, string OUTENTRANCE, string INENTRANCE, int ENABLED)
        {
            wms_base_zone model = new wms_base_zone();
            wms_base_zone oldmodel = new wms_base_zone();
            oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
            try
            {
                //修改后的编号也不可重复     修改后同时更新库区和库位的编号
                var newcount = service.Query(t => t.ZONECODE == ZONECODE & t.ZONECODE != oldmodel.ZONECODE).Count();
                if (newcount > 0)
                {
                    return Json(new { result = "编号重复" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.ID = ID;
                    model.ZONECODE = ZONECODE;
                    model.ZONENAME = ZONENAME;

                    model.WAREHOUSECODE = WAREHOUSECODE;
                    model.OUTENTRANCE = OUTENTRANCE;
                    model.INENTRANCE = INENTRANCE;
                    model.ENABLED = ENABLED;
                    model.CREATOR = oldmodel.CREATOR;
                    model.CREATETIME = oldmodel.CREATETIME;
                    model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
                    model.LASTEDITTIME = DateTime.Now;
                    var results = (service.Update(model) & location.Update(t => t.ZONECODE == ZONECODE, t => t.ZONECODE == oldmodel.ZONECODE));
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
            wms_base_zone model = new wms_base_zone();
            var result = true;
            try
            {
                //含有库位信息不能删除
                wms_base_zone oldmodel = new wms_base_zone();
                oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
                var ZONECODE = oldmodel.ZONECODE;
                var zonecount = location.Query(t => t.ZONECODE == ZONECODE).Count();
                if (zonecount > 0)
                {
                    return Json(new { result = "该库区包含库位信息，不能删除！" }, JsonRequestBehavior.AllowGet);

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

        /// <summary>
        /// 获取仓库下拉列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult GetSearchSelectListByWarehouse(string code, string name)
        {

            var lists = warehouse.Query(t => t.ENABLED==1);
            var Platforms = "<option value=''>" + name + "</option>";
            foreach (var item in lists)
            {
                Platforms += "<option value = '" + item.WAREHOUSECODE + "'> " + item.WAREHOUSENAME + " </option> ";
            }
            return Json(Platforms, JsonRequestBehavior.AllowGet);
        }

       

    }
}