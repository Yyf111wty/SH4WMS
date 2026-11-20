﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using TR.IService;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    //[CheckLoginFilter]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class PutOutController : Controller
    {
        Iwms_bill_putoutheaderService service = new wms_bill_putoutheaderService();
        wms_bill_putoutbodyService bodyservice = new wms_bill_putoutbodyService();
        wms_base_locationService locationservice = new wms_base_locationService();
        // GET: WMS/PutIn
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetHeadData(PutOutModel putout)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageIndex = ParsePageIndex(Request["page"]);
            List<wms_bill_putoutheader> lists = new List<wms_bill_putoutheader>();

            if (putout == null)
            {
                putout = new PutOutModel();
            }

            NormalizePutOutFilter(putout);
            ApplyUserZoneScope(putout);

            bool hasUserDate = putout.startTime.HasValue || putout.endTime.HasValue;

            try
            {
                if (hasUserDate)
                {
                    // 用户指定时间范围：按完成时间过滤
                    lists = service.GetHeaderPage(
                        putout, true, null, null,
                        ref totalCount, pageIndex, pageSize);
                }
                else
                {
                    // 未指定时间：默认只查最近 30 天，避免全表扫描
                    DateTime recentStart = DateTime.Now.AddDays(-30);
                    DateTime recentEnd = DateTime.Now.AddDays(1);
                    lists = service.GetHeaderPage(
                        putout, false, recentStart, recentEnd,
                        ref totalCount, pageIndex, pageSize);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("出库清单查询失败：" + ex);
            }

            return Json(new { total = totalCount, rows = lists }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBodyData(string ID)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageIndex = ParsePageIndex(Request["page"]);
            List<wms_bill_putoutbody> lists = new List<wms_bill_putoutbody>();

            if (string.IsNullOrWhiteSpace(ID))
            {
                return Json(new { total = 0, rows = lists }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                lists = bodyservice.QueryPage(x => x.HEADERID == ID,
                     ref totalCount, pageIndex, pageSize, "CREATETIME DESC").ToList();
            }
            catch (Exception ex)
            {
                Trace.TraceError("出库明细查询失败：" + ex);
            }

            return Json(new { total = totalCount, rows = lists }, JsonRequestBehavior.AllowGet);
        }

        private static int ParsePageSize(string limit)
        {
            int size;
            if (!int.TryParse(limit, out size) || size <= 0)
            {
                size = 30;
            }

            return size > 200 ? 200 : size;
        }

        private static int ParsePageIndex(string page)
        {
            int index;
            if (!int.TryParse(page, out index) || index <= 0)
            {
                index = 1;
            }

            return index;
        }

        /// <summary>
        /// 空字符串归一为 null，避免生成 LIKE '%%' 导致无法命中索引。
        /// </summary>
        private static void NormalizePutOutFilter(PutOutModel putout)
        {
            putout.SOURCEORDER = TrimToNull(putout.SOURCEORDER);
            putout.CONTAINER = TrimToNull(putout.CONTAINER);
            putout.TARGET = TrimToNull(putout.TARGET);
            putout.ITEMCODE = TrimToNull(putout.ITEMCODE);
            putout.LOT = TrimToNull(putout.LOT);
            putout.ZONECODE = TrimToNull(putout.ZONECODE);
        }

        private static string TrimToNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private void ApplyUserZoneScope(PutOutModel putout)
        {
            string uid = (HttpContextHelper.GetCurrentUserName() ?? string.Empty).ToLower();
            if (uid == "ycl")
            {
                putout.ZONECODE = "101-1";
            }
            else if (uid == "bcp")
            {
                putout.ZONECODE = "201-1";
            }
            else if (uid == "cp")
            {
                putout.ZONECODE = "301-1";
            }
            else if (uid == "lck")
            {
                putout.ZONECODE = "401-1";
            }
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