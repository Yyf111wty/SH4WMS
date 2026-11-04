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
    //[CheckLoginFilter]
    public class PutOutController : Controller
    {
        wms_bill_putoutheaderService service = new wms_bill_putoutheaderService();
        wms_bill_putoutbodyService bodyservice = new wms_bill_putoutbodyService();
        wms_base_locationService locationservice = new wms_base_locationService();
        // GET: WMS/PutIn
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetHeadData(PutOutModel putout)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_bill_putoutheader> lists = null;
			//根据用户名显示内容
			string uid =HttpContextHelper.GetCurrentUserName().ToLower();
			if (uid == "ycl")
			{
				putout.ZONECODE = "101-1";

			}
			if (uid == "bcp")
			{
				putout.ZONECODE = "201-1";

			}
			if (uid == "cp")
			{
				putout.ZONECODE = "301-1";

			}
			if (uid == "lck")
			{
				putout.ZONECODE = "401-1";

			}
			try
			{
                lists = service.Getlist(x =>
                     (putout.SOURCEORDER == null || x.SOURCEORDER.Contains(putout.SOURCEORDER))
                     && (putout.CONTAINER == null || x.CONTAINER.Contains(putout.CONTAINER))
					  && (putout.TARGET == null || x.TARGET.Contains(putout.TARGET))
					   && (putout.ITEMCODE == null || x.ITEMCODE.Equals(putout.ITEMCODE))
						&& (putout.LOT == null || x.LOT.Equals(putout.LOT))
						&& (putout.ZONECODE == null || x.ZONECODE.Equals(putout.ZONECODE))
					 && (putout.STATE == -10 || x.STATE.Equals(putout.STATE))
                     && (putout.ENABLED == -10 || x.ENABLED.Equals(putout.ENABLED))
                     && (putout.QUESTFLAG == -10 || x.QUESTFLAG.Equals(putout.QUESTFLAG))
                     && (putout.startTime == null || putout.startTime <= x.FINISHTIME)
                     && (putout.endTime == null || putout.endTime >= x.FINISHTIME)
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


		
		public ActionResult GetBodyData(string ID)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_bill_putoutbody> lists = null;

            try
            {
                lists = bodyservice.QueryPage(x => x.HEADERID == ID


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
		public JsonResult DeleteHead(string ID)
		{
			wms_bill_putoutheader model = new wms_bill_putoutheader();
			var result = true;
			try
			{
				wms_bill_putoutheader oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
				if (oldmodel == null)
				{
					return Json(new { result = false, msg = "出库单不存在" }, JsonRequestBehavior.AllowGet);
				}

				ReleasePutOutLocationOccupation(oldmodel);

				var list = bodyservice.Query(t => t.HEADERID == oldmodel.ID);
				foreach (var item in list)
				{
					bodyservice.Delete(item);
				}
				model.ID = ID;
				result = service.Delete(model);
				return Json(new { result = result }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void ReleasePutOutLocationOccupation(wms_bill_putoutheader putoutHeader)
		{
			if (putoutHeader == null || string.IsNullOrWhiteSpace(putoutHeader.LOCATIONCODE))
			{
				return;
			}

			string zoneCode = string.IsNullOrWhiteSpace(putoutHeader.ZONECODE) ? "101" : putoutHeader.ZONECODE.Trim();
			string locationCode = putoutHeader.LOCATIONCODE.Trim();
			string container = (putoutHeader.CONTAINER ?? string.Empty).Trim();
			string editor = HttpContextHelper.GetCurrentUserName();
			if (string.IsNullOrWhiteSpace(editor))
			{
				editor = "WMS";
			}

			var location = locationservice.Query(t =>
				t.ZONECODE == zoneCode
				&& t.LOCATIONCODE == locationCode
				&& (string.IsNullOrEmpty(container) || t.CONTAINER == container)).FirstOrDefault();

			if (location == null || location.OUTENABLED != 0)
			{
				return;
			}

			location.OUTENABLED = 1;
			location.LASTEDITOR = editor;
			location.LASTEDITTIME = DateTime.Now;
			locationservice.Update(location);
		}


	}
}