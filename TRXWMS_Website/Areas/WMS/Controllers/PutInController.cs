﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.SessionState;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_BillApi.Models.Response;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [CheckLoginFilter]
	[SessionState(SessionStateBehavior.ReadOnly)]
	public class PutInController : Controller
    {
        wms_bill_putinbindingheaderService service = new wms_bill_putinbindingheaderService();
		wms_bill_putinService putinservice = new wms_bill_putinService();
		wms_bill_putinbindingbodyService bodyservice = new wms_bill_putinbindingbodyService();
        // GET: WMS/PutIn
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult updateHead(string id)
        {
            try
            {
                var header = service.Query(x => x.ID == id).FirstOrDefault();

                if (header == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "未找到该入库单！"
                    });
                }

                header.STATE = 0;
                header.QUESTFLAG = 0;

                bool result = service.Update(header);

                return Json(new
                {
                    success = result,
                    message = result ? "修改成功" : "修改失败"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public ActionResult GetHeadData(PutInModel WMS_BILL_PUTIN)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageIndex = ParsePageIndex(Request["page"]);
            List<wms_bill_putinbindingheader> lists = new List<wms_bill_putinbindingheader>();

            if (WMS_BILL_PUTIN == null)
            {
                WMS_BILL_PUTIN = new PutInModel();
            }

            NormalizePutInFilter(WMS_BILL_PUTIN);
            ApplyUserZoneScope(WMS_BILL_PUTIN);

            bool hasUserDate = WMS_BILL_PUTIN.startTime.HasValue || WMS_BILL_PUTIN.endTime.HasValue;

            try
            {
                if (hasUserDate)
                {
                    // 用户指定时间范围：按完成时间过滤
                    lists = putinservice.GetHeaderPage(
                        WMS_BILL_PUTIN, true, null, null,
                        ref totalCount, pageIndex, pageSize);
                }
                else
                {
                    // 未指定时间：默认只查最近 30 天，避免全表扫描
                    DateTime recentStart = DateTime.Now.AddDays(-30);
                    DateTime recentEnd = DateTime.Now.AddDays(1);
                    lists = putinservice.GetHeaderPage(
                        WMS_BILL_PUTIN, false, recentStart, recentEnd,
                        ref totalCount, pageIndex, pageSize);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("入库清单查询失败：" + ex);
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
        private static void NormalizePutInFilter(PutInModel model)
        {
            model.SOURCEORDER = TrimToNull(model.SOURCEORDER);
            model.TARGET = TrimToNull(model.TARGET);
            model.ITEMCODE = TrimToNull(model.ITEMCODE);
            model.LOT = TrimToNull(model.LOT);
            model.CONTAINER = TrimToNull(model.CONTAINER);
            model.LOCATIONCODE = TrimToNull(model.LOCATIONCODE);
        }

        private static string TrimToNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private void ApplyUserZoneScope(PutInModel model)
        {
            string uid = (HttpContextHelper.GetCurrentUserName() ?? string.Empty).ToLower();
            if (uid == "ycl")
            {
                model.TARGET = "101-1";
            }
            else if (uid == "bcp")
            {
                model.TARGET = "201-1";
            }
            else if (uid == "cp")
            {
                model.TARGET = "301-1";
            }
            else if (uid == "lck")
            {
                model.TARGET = "401-1";
            }
        }


		[HttpPost]
		public JsonResult addalllocation()
		{
			
		
					for (int k = 21000201; k <= 21000400; k++)
					{
						wms_bill_putinbindingheader model = new wms_bill_putinbindingheader();
						
						model.ID = "e" + k;
						model.SOURCEORDER = "wms"+k;
						
						model.TASKTYPE = "空托盘入库";
						
						model.CONTAINER = k.ToString();
						model.CONTAINERTYPE = "1";

						model.QUESTFLAG = 0;
						model.STATE = 0;
						model.TARGET = "201-1";
				model.LOCATIONCODE = "";
				model.ENABLED = 1;
				
						model.REMARK = "空托盘入库";
						// model.XYZ = XYZ;
						//  model.CHECKXYZ = CHECKXYZ;
						// model.SPECS = SPECS;
						//model.IFMIXING = IFMIXING;
						model.CREATOR = HttpContextHelper.GetCurrentUserName();
						model.CREATETIME = DateTime.Now;
						//model.LASTEDITOR = HttpContextHelper.GetCurrentUserName();
						//model.LASTEDITTIME = DateTime.Now;
						service.InsertReturnIdentity(model);
					}
			

			return Json(new { result = 0 }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetBodyData(string ID)
        {
            int totalCount = 0;
            int pageSize = ParsePageSize(Request["limit"]);
            int pageIndex = ParsePageIndex(Request["page"]);
            List<wms_bill_putinbindingbody> lists = new List<wms_bill_putinbindingbody>();

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
                Trace.TraceError("入库明细查询失败：" + ex);
            }

            return Json(new { total = totalCount, rows = lists }, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		public JsonResult DeleteHead(string ID)
		{
			wms_bill_putinbindingheader model = new wms_bill_putinbindingheader();
			string result = "";
			try
			{
				//删除堆垛机任务 入库单及明细  更新库存
				wms_bill_putinbindingheader oldmodel = new wms_bill_putinbindingheader();
				oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
				if (oldmodel.STATE == 2)
				{
					result = "入库已完成，无法删除";					
				}
				else
				{
					result= putinservice.DeletePutIn(ID);					
				}
				return Json(new { result = result }, JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


	}
}