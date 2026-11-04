using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_BillApi.Models.Response;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    [CheckLoginFilter]
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
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_bill_putinbindingheader> lists = null;
			//List<PutInModel> listsmodel = null;
			//根据用户名显示内容
			string uid =HttpContextHelper.GetCurrentUserName().ToLower();
			if (uid == "ycl")
			{
				WMS_BILL_PUTIN.TARGET = "101-1";

			}
			if (uid == "bcp")
			{
				WMS_BILL_PUTIN.TARGET = "201-1";

			}
			if (uid == "cp")
			{
				WMS_BILL_PUTIN.TARGET = "301-1";

			}
			if (uid == "lck")
			{
				WMS_BILL_PUTIN.TARGET = "401-1";

			}
			try
            {
				lists = putinservice.Getlist(a=>
					 (WMS_BILL_PUTIN.SOURCEORDER == null || a.SOURCEORDER.Contains(WMS_BILL_PUTIN.SOURCEORDER))

					  && (WMS_BILL_PUTIN.TARGET == null || a.TARGET.Equals(WMS_BILL_PUTIN.TARGET))
					   && (WMS_BILL_PUTIN.ITEMCODE == null || a.ITEMCODE.Equals(WMS_BILL_PUTIN.ITEMCODE))
						&& (WMS_BILL_PUTIN.LOT == null || a.LOT.Equals(WMS_BILL_PUTIN.LOT))

					 && (WMS_BILL_PUTIN.CONTAINER == null || a.CONTAINER.Equals(WMS_BILL_PUTIN.CONTAINER))
						&& (WMS_BILL_PUTIN.LOCATIONCODE == null || a.LOCATIONCODE.Equals(WMS_BILL_PUTIN.LOCATIONCODE))
					 && (WMS_BILL_PUTIN.STATE == -10 || a.STATE.Equals(WMS_BILL_PUTIN.STATE))
					 && (WMS_BILL_PUTIN.ENABLED == -10 || a.ENABLED.Equals(WMS_BILL_PUTIN.ENABLED))
					 && (WMS_BILL_PUTIN.QUESTFLAG == -10 || a.QUESTFLAG.Equals(WMS_BILL_PUTIN.QUESTFLAG))
					 && (WMS_BILL_PUTIN.startTime == null || WMS_BILL_PUTIN.startTime<=a.FINISHTIME)
					 && (WMS_BILL_PUTIN.endTime == null || WMS_BILL_PUTIN.endTime>=a.FINISHTIME)
					 ,
					 ref TotalCount, pageId, pageSize, "CREATETIME DESC")					
					.ToList();

				
			

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
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<wms_bill_putinbindingbody> lists = null;

            try
            {
                lists = bodyservice.QueryPage(x =>x.HEADERID==ID
                    
                   
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