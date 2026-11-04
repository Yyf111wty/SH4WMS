using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.ORM.Model;
using TR.Service;
using TRWMS_BillApi.Models.Response;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.BIDD.Controllers
{
   // [CheckLoginFilter]
    public class CustomerLevelController : Controller
    {
        jc_customer_levelService service = new jc_customer_levelService();
        jc_bidding_detailService dservice = new jc_bidding_detailService();
        sys_usersService userservice = new sys_usersService();
        sys_userroleService userroleservice = new sys_userroleService();
        // GET: BIDD/CustomerLevel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AddOrEdit()
        {
            return View();
        }

        [HttpPost]
        public JsonResult EditProduct(string ID, string CUSTOMERNAME, string PREPRICE)
        {
            jc_customer_level model = new jc_customer_level();
            jc_customer_level oldmodel = new jc_customer_level();
            oldmodel = service.Query(t => t.ID == ID).FirstOrDefault();
            try
            {
                //var codeid = ZONECODE +"-"+ LOCATIONCODE;
                //var codecount = service.Query(t => t.ID == codeid&t.ID!=oldmodel.ID).Count;
                //if (codecount > 0)
                //{
                //    return Json(new { result = "库区库位信息重复" }, JsonRequestBehavior.AllowGet);
                //}
                model.ID = ID;
                model.MYNAME=oldmodel.MYNAME;
                model.CUSTOMERNAME = CUSTOMERNAME;              
                model.PREPRICE = int.Parse(PREPRICE);    
                model.CREATOR = oldmodel.CREATOR;
                model.CREATETIME = oldmodel.CREATETIME;
                model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                model.LASTEDITTIME = DateTime.Now;
                var results = (service.Update(model));
                return Json(new { result = results }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult GetRowData(jc_customer_level jc_customer_level)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<jc_customer_level> lists = null;
            try
            {


                lists = service.QueryPage(x =>
                     ((jc_customer_level.CUSTOMERNAME == null || x.CUSTOMERNAME.Contains(jc_customer_level.CUSTOMERNAME))),
                      //&& (jc_customer_level.CREDITRATING == null || x.CREDITRATING.Contains(jc_customer_level.CREDITRATING))
                      //&&(jc_customer_level.CUSTOMERNAME ==HttpContextHelper.GetCurrentUserName() ),
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
        public JsonResult DBiNIT()
        {
         
            try
            {

               var  s=  dservice.DBiNIT();
                return Json(new
                {
                    result = s,
                   
                    msg = "获取数据成功"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                  
                    msg = "报错" + ex.Message
                });

            }
        }

    }
}