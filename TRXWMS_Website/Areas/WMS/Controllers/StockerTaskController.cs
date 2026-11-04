using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    //[CheckLoginFilter]
    public class StockerTaskController : Controller
    {
        wms_stocker_taskService service = new wms_stocker_taskService();
        //WMS_BAS_CODESETService codesetservice = new WMS_BAS_CODESETService();

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取下拉列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //public JsonResult GetSearchSelectListByCodeName(string code,string  name)
        //{

        //    var lists = codesetservice.Query(t => t.CODE == code);
        //    var Platforms = "<option value=''>"+name+"</option>";
        //    foreach (var item in lists)
        //    {
        //        Platforms += "<option value = '" + item.VALUE + "'> " + item.NAME + " </option> ";
        //    }
        //    return Json(Platforms, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult GetRowData(wms_stocker_task WMS_STOCKER_TASK)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_stocker_task> lists = null;
			//根据用户名显示内容
			string uid =HttpContextHelper.GetCurrentUserName().ToLower();
			if (uid == "ycl")
			{
				WMS_STOCKER_TASK.ZONECODE = "101-1";

			}
			if (uid == "bcp")
			{
				WMS_STOCKER_TASK.ZONECODE = "201-1";

			}
			if (uid == "cp")
			{
				WMS_STOCKER_TASK.ZONECODE = "301-1";

			}
			if (uid == "lck")
			{
				WMS_STOCKER_TASK.ZONECODE = "401-1";

			}
			try
            {
				
				lists = service.QueryPage(x =>
                    (WMS_STOCKER_TASK.SOURCEORDER == null || x.SOURCEORDER.Contains(WMS_STOCKER_TASK.SOURCEORDER))
                    && (WMS_STOCKER_TASK.ZONECODE == null || x.ZONECODE.Contains(WMS_STOCKER_TASK.ZONECODE))
                    && (WMS_STOCKER_TASK.CONTAINER == null || x.CONTAINER.Contains(WMS_STOCKER_TASK.CONTAINER))
                    && (WMS_STOCKER_TASK.STARTTAXI == null || x.STARTTAXI.Contains(WMS_STOCKER_TASK.STARTTAXI))
                    && (WMS_STOCKER_TASK.ENDTAXI == null || x.ENDTAXI.Contains(WMS_STOCKER_TASK.ENDTAXI))
                    //&& (WMS_STOCKER_TASK.ITEMCODE == null || x.ITEMCODE.Contains(WMS_STOCKER_TASK.ITEMCODE))
                    //&& (WMS_STOCKER_TASK.ITEMNAME == null || x.ITEMNAME.Contains(WMS_STOCKER_TASK.ITEMNAME))
                    && (WMS_STOCKER_TASK.TYPE == -10 || x.TYPE.Equals(WMS_STOCKER_TASK.TYPE))
                    && (WMS_STOCKER_TASK.STATE == -10 || x.STATE.Equals(WMS_STOCKER_TASK.STATE))
                    //&& (WMS_STOCKER_TASK.STARTTIME == null || x.STARTTIME > WMS_STOCKER_TASK.STARTTIME)
                    //&& (WMS_STOCKER_TASK.FINISHTIME == null || x.STARTTIME < (WMS_STOCKER_TASK.FINISHTIME))
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

		[HttpPost]
		public JsonResult DeleteProduct(string ID)
		{
			wms_stocker_task model = new wms_stocker_task();
			var result = true;
			try
			{
				model.ID = ID;
				result = (service.Delete(model));
				return Json(new { result = result }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		//public ActionResult AddOrEdit()
		//{
		//    return View();
		//}
		#region   暂时作废
		//[HttpPost]
		//public JsonResult AddProduct(string CODE, string SOURCEORDER, string WAREHOUSECODE, string CONTAINER, int TYPE, int PRIORITY, string STARTTAXI, string ENDTAXI, int STATE, string ITEMCODE, string ITEMNAME)
		//{
		//    WMS_STOCKER_TASK model = new WMS_STOCKER_TASK();
		//    try
		//    {

		//        model.ID = ResultHelper.NewId;
		//        model.CODE = CODE;
		//        model.SOURCEORDER = SOURCEORDER;
		//        model.WAREHOUSECODE = WAREHOUSECODE;
		//        model.CONTAINER = CONTAINER;
		//        model.TYPE = TYPE;
		//        model.PRIORITY = PRIORITY;
		//        model.STARTTAXI = STARTTAXI;
		//        model.ENDTAXI = ENDTAXI;
		//        model.STATE = STATE;
		//        model.ITEMCODE = ITEMCODE;
		//        model.ITEMNAME = ITEMNAME;
		//        model.CREATOR = HttpContextHelper.GetCurrentUserName();
		//        model.CREATETIME = DateTime.Now;
		//        model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
		//        model.LASTEDITTIME = DateTime.Now;
		//        var results = (service.InsertReturnIdentity(model) == 1);
		//        return Json(new { result = results }, JsonRequestBehavior.AllowGet);

		//    }
		//    catch (Exception ex)
		//    {
		//        throw ex;
		//    }
		//}

		//[HttpPost]
		//public JsonResult EditProduct(string ID, string CODE, string SOURCEORDER, string WAREHOUSECODE, string CONTAINER, int TYPE, int PRIORITY, string STARTTAXI, string ENDTAXI, int STATE, string ITEMCODE, string ITEMNAME)
		//{
		//    WMS_STOCKER_TASK model = new WMS_STOCKER_TASK();
		//    WMS_STOCKER_TASK oldmodel = new WMS_STOCKER_TASK();
		//    oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
		//    try
		//    {
		//        model.ID = ID;
		//        model.CODE = CODE;
		//        model.SOURCEORDER = SOURCEORDER;
		//        model.WAREHOUSECODE = WAREHOUSECODE;
		//        model.CONTAINER = CONTAINER;
		//        model.TYPE = TYPE;
		//        model.PRIORITY = PRIORITY;
		//        model.STARTTAXI = STARTTAXI;
		//        model.ENDTAXI = ENDTAXI;
		//        model.STATE = STATE;
		//        model.ITEMCODE = ITEMCODE;
		//        model.ITEMNAME = ITEMNAME;
		//        model.STARTTIME = oldmodel.STARTTIME;
		//        model.IMPLEMENTTIME = oldmodel.IMPLEMENTTIME;
		//        model.FINISHTIME = oldmodel.FINISHTIME;
		//        model.CREATOR = oldmodel.CREATOR;
		//        model.CREATETIME = oldmodel.CREATETIME;
		//        model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
		//        model.LASTEDITTIME = DateTime.Now;
		//        var results = (service.Update(model));
		//        return Json(new { result = results }, JsonRequestBehavior.AllowGet);

		//    }
		//    catch (Exception ex)
		//    {
		//        throw ex;
		//    }
		//}



		//[HttpPost]
		//public JsonResult finish(string ID)
		//{
		//    JObject jObject = new JObject() {
		//        {"taskId",ID},
		//        {"user",HttpContextHelper.GetCurrentUserName()},
		//    };
		//    var reslut = ReslutJsonHelper.HttpPostMethod(InterfaceConfig.finish, jObject);
		//    return Json(new { reslut = reslut.success, msg = reslut.msg }, JsonRequestBehavior.AllowGet);

		//}

		//[HttpPost]
		//public JsonResult cancel(string ID)
		//{
		//    JObject jObject = new JObject() {
		//        {"taskId",ID},
		//    };
		//    var reslut = ReslutJsonHelper.HttpPostMethod(InterfaceConfig.cancel, jObject);
		//    return Json(new { reslut = reslut.success, msg = reslut.msg }, JsonRequestBehavior.AllowGet);

		//}

		//[HttpPost]
		//public JsonResult replay(string ID)
		//{
		//    JObject jObject = new JObject() {
		//        {"taskId",ID},
		//    };
		//    var reslut = ReslutJsonHelper.HttpPostMethod(InterfaceConfig.replay, jObject);
		//    return Json(new { reslut = reslut.success, msg = reslut.msg }, JsonRequestBehavior.AllowGet);

		//}
		#endregion
	}
}