using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.BIDD.Controllers
{
    //[CheckLoginFilter]
    public class UnBiddingHistoryController : Controller
    {
        jc_bidding_detailService bodyservice = new jc_bidding_detailService();
        // GET: BIDD/BiddingHistory
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetRowData(BiddingProjectDetailModel BiddingProjectDetailModel)
        {
            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<BiddingProjectDetailModel> lists = null;
            //根据用户名显示内容
            string uid =HttpContextHelper.GetCurrentUserName();

            try
            {
                lists = bodyservice.Getlist(x =>
                     ((BiddingProjectDetailModel.COMPANYNAME == null || x.COMPANYNAME.Contains(BiddingProjectDetailModel.COMPANYNAME)))
                     && (BiddingProjectDetailModel.PROJECTCODE == null || x.PROJECTCODE.Contains(BiddingProjectDetailModel.PROJECTCODE))
                     && (BiddingProjectDetailModel.PROJECTNAME == null || x.PROJECTNAME.Contains(BiddingProjectDetailModel.PROJECTNAME))
                     && (x.USERNAME == uid) && (x.DESCRIBE != "1") && (x.BIDDINGTYPE == "非标准项目"),
                     ref TotalCount, pageId, pageSize, "BIDDTIME DESC");


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