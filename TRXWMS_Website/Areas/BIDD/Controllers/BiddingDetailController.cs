using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.Model;
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRXWMS_Website.Helper;

namespace TRXWMS_Website.Areas.BIDD.Controllers
{
  // [CheckLoginFilter]
    public class BiddingDetailController : Controller
    {

        jc_bidding_projectService service = new jc_bidding_projectService();
        jc_bidding_detailService bodyservice = new jc_bidding_detailService();
        jc_bidding_historyService hisservice = new jc_bidding_historyService();
        jc_customer_levelService cusservice= new jc_customer_levelService();

        // GET: BIDD/BiddingDetail
        public ActionResult Index()
        {
            return View();
        }
		public ActionResult Rrice()
		{
			return View();
		}
        public ActionResult Test()
        {
            return View();
        }
        public ActionResult GetHeadData(jc_bidding_project JC_BIDDING_PROJECT)
		{
			int TotalCount = 0;
			int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
			int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
			List<jc_bidding_project> lists = null;
		
			try
			{

				lists = service.QueryPage(x =>
					 ((JC_BIDDING_PROJECT.COMPANYNAME == null || x.COMPANYNAME.Contains(JC_BIDDING_PROJECT.COMPANYNAME)))
					 && (JC_BIDDING_PROJECT.PROJECTCODE == null || x.PROJECTCODE.Contains(JC_BIDDING_PROJECT.PROJECTCODE))
					 && (JC_BIDDING_PROJECT.PROJECTNAME == null || x.PROJECTNAME.Contains(JC_BIDDING_PROJECT.PROJECTNAME))
					 && (JC_BIDDING_PROJECT.PROJECTTYPE == null || x.PROJECTTYPE.Contains(JC_BIDDING_PROJECT.PROJECTTYPE))
					 && (x.STATE == 0) && (x.BIDDINGTYPE == "标准项目" ),
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
			List<BiddingProjectDetailModel> lists = null;
			//根据用户名显示内容
			string uid =HttpContextHelper.GetCurrentUserName();
		
			try
			{
				lists = bodyservice.Getlist(a => ((a.PROJECTCODE==ID)&(a.USERNAME==uid) & a.DESCRIBE != "1"), ref TotalCount, pageId, pageSize, "BIDDTIME DESC").Take(5).ToList();


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
        public JsonResult AddProduct( string PROJECTCODE, string MYPRICE)
        {
            jc_bidding_project projectmodel = new jc_bidding_project();
            jc_bidding_project projectupmodel = new jc_bidding_project();
            jc_bidding_detail model = new jc_bidding_detail();
            jc_bidding_detail newmodel = new jc_bidding_detail();
            jc_bidding_history hismodel = new jc_bidding_history();
           
            try
            {
                //判断是否时间已经截止，是否触发延迟次数（当前出价最低且在最后一分钟），出价是否入围，且此次出价是否满足幅度且大于最小值   超出值也要记录但客户端不显示
                //返回排名
                int price = 100;
                var  projectcount= service.Query(t => t.PROJECTCODE == PROJECTCODE );
                var cuscount = cusservice.Query(t => t.CUSTOMERNAME ==HttpContextHelper.GetCurrentUserName());
                if (cuscount.Count > 0)
                {
                    price = cuscount.FirstOrDefault().PREPRICE;
                }
               

                if (projectcount.Count > 0)
                {
                    projectmodel = projectcount.FirstOrDefault();
                  
                    // DateTime finishtime=   projectmodel.ENDTIME;
                    // 获取当前时间
                    DateTime now = DateTime.Now;
                    if (now >= projectmodel.ENDTIME || now< projectmodel.STARTTIME)
                    {
                        return Json(new
                        {
                            result = false,
                            rank = "",
                            endtime = "",
                            msg = "当前项目已截止或未开始，无法报价"
                        });
                    }
                    else
                    {
                        //判断出价是否合理
                        double value = (100 + (100 - price)) / 100.0;
                        if (int.Parse(MYPRICE) > projectmodel.MAXPRICE )
                        {

                            hismodel.ID = ResultHelper.NewId;
                            hismodel.PROJECTCODE = PROJECTCODE;
                            hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                            hismodel.MYPRICE = int.Parse(MYPRICE);

                            hismodel.PREPRICE = price;

                            hismodel.TRUEPRICE =(int) (int.Parse(MYPRICE)*value);

                            hismodel.DESCRIBE = "1";
                            hismodel.BIDDTIME = DateTime.Now;
                            hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                            hismodel.CREATETIME = DateTime.Now;
                            hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                            hismodel.LASTEDITTIME = DateTime.Now;
                            hisservice.InsertReturnIdentity(hismodel);

                            return Json(new
                            {
                                result = false,
                                rank = "",
                                endtime = "",
                                msg = "当前项目报价高于最高入围价，请重新报价"
                            });
                        }
                       else if (int.Parse(MYPRICE) < projectmodel.MINPRICE)
                        {

                            hismodel.ID = ResultHelper.NewId;
                            hismodel.PROJECTCODE = PROJECTCODE;
                            hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                            hismodel.MYPRICE = int.Parse(MYPRICE);
                            hismodel.PREPRICE = price;
                            hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                            hismodel.DESCRIBE = "1";
                            hismodel.BIDDTIME = DateTime.Now;
                            hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                            hismodel.CREATETIME = DateTime.Now;
                            hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                            hismodel.LASTEDITTIME = DateTime.Now;
                            hisservice.InsertReturnIdentity(hismodel);

                            return Json(new
                            {
                                result = false,
                                rank = "",
                                endtime = "",
                                msg = "当前项目报价低于最小入围价，请重新报价"
                            });
                        }
                        else
                        {
                            //判断出价
                            var mylist = hisservice.Query(t => t.PROJECTCODE == PROJECTCODE & t.USERNAME ==HttpContextHelper.GetCurrentUserName() & t.DESCRIBE != "1")
                                                 .OrderByDescending(t=>t.BIDDTIME).ToList();
                            //获取当前最低出价
                            var lowlist = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE).OrderBy(t => t.TRUEPRICE)
                                                                  .ThenBy(t => t.BIDDTIME).ToList();
                            if (mylist.Count > 0)
                            {
                                if (int.Parse(MYPRICE) >= mylist.First().MYPRICE || (int.Parse(MYPRICE) > (mylist.First().MYPRICE - projectmodel.RANGEPRICE)))
                                {
                                    return Json(new
                                    {
                                        result = false,
                                        rank = "",
                                        endtime = "",
                                        msg = "当前项目报价低于降价区间，请重新报价"
                                    });
                                }
                                else
                                {
                                    //出价
                                    //获取当前最低出价
                                    var templist = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE&t.USERNAME !=HttpContextHelper.GetCurrentUserName()).OrderBy(t => t.TRUEPRICE)
                                                                          .ThenBy(t => t.BIDDTIME).ToList();

                                    TimeSpan timeSpan = projectmodel.ENDTIME - now;
                                    if (templist.Count > 0)
                                    {
                                        //有其他人竞争出价时int.Parse(MYPRICE) < templist.First().MYPRICE &
                                        //有价格变动
                                        if ( (timeSpan.TotalSeconds > 0 & timeSpan.TotalSeconds < 60))
                                        {
                                            //更新项目结束时间 剩余延迟次数
                                            if (projectmodel.REMAINDELAYTIMES > 0)
                                            {
                                                projectupmodel.ID = projectmodel.ID;
                                                projectupmodel.PROJECTCODE = PROJECTCODE;
                                                projectupmodel.ENDTIME = projectmodel.ENDTIME.AddMinutes(3);
                                                projectupmodel.REMAINDELAYTIMES = projectmodel.REMAINDELAYTIMES - 1;

                                                projectupmodel.COMPANYNAME = projectmodel.COMPANYNAME;
                                                projectupmodel.PROJECTNAME = projectmodel.PROJECTNAME; ;
                                                projectupmodel.PROJECTTYPE = projectmodel.PROJECTTYPE; ;
                                                projectupmodel.UNITPRICE = projectmodel.UNITPRICE; ;
                                                //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                                                projectupmodel.NUMBER = projectmodel.NUMBER; 
                                                projectupmodel.DESCRIBE = projectmodel.DESCRIBE; 
                                                projectupmodel.BUDGET = projectmodel.BUDGET;
                                                projectupmodel.MINPRICE = projectmodel.MINPRICE;
                                                projectupmodel.MAXPRICE = projectmodel.MAXPRICE;
                                                projectupmodel.RANGEPRICE = projectmodel.RANGEPRICE;
                                                projectupmodel.FILEADDRESS = projectmodel.FILEADDRESS;
                                                projectupmodel.DELAYTIMES = projectmodel.DELAYTIMES;                                             
                                                projectupmodel.STARTTIME = projectmodel.STARTTIME;
                                                projectupmodel.STATE = projectmodel.STATE;
                                                projectupmodel.CUSTOMERS = projectmodel.CUSTOMERS;
                                                projectupmodel.BID = projectmodel.BID;
                                                projectupmodel.BIDDINGTYPE = projectmodel.BIDDINGTYPE;
                                                projectupmodel.BIDPRICE = projectmodel.BIDPRICE;
                                                projectupmodel.BIDTIME = projectmodel.BIDTIME;
                                                projectupmodel.CREATOR = projectmodel.CREATOR;
                                                projectupmodel.CREATETIME = projectmodel.CREATETIME;
                                                projectupmodel.LASTEDITOR = projectmodel.LASTEDITOR;
                                                projectupmodel.LASTEDITTIME = projectmodel.LASTEDITTIME;


                                                service.Update(projectupmodel);
                                                //更新detail
                                                var detailcount = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE & t.USERNAME ==HttpContextHelper.GetCurrentUserName());


                                                model = detailcount.FirstOrDefault();
                                                newmodel.ID = model.ID;
                                                newmodel.PROJECTCODE = PROJECTCODE;
                                                newmodel.USERNAME = model.USERNAME;

                                                newmodel.MYPRICE = int.Parse(MYPRICE);
                                                newmodel.PREPRICE = price;
                                                newmodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                                //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));

                                                newmodel.DESCRIBE = "";

                                                newmodel.BIDDTIME = DateTime.Now;

                                                newmodel.CREATOR = model.CREATOR;
                                                newmodel.CREATETIME = model.CREATETIME;
                                                newmodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                                newmodel.LASTEDITTIME = DateTime.Now;
                                                bodyservice.Update(newmodel);


                                                hismodel.ID = ResultHelper.NewId;
                                                hismodel.PROJECTCODE = PROJECTCODE;
                                                hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                                hismodel.MYPRICE = int.Parse(MYPRICE);
                                                hismodel.PREPRICE = price;
                                                hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                                hismodel.DESCRIBE = "";
                                                hismodel.BIDDTIME = DateTime.Now;
                                                hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                                hismodel.CREATETIME = DateTime.Now;
                                                hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                                hismodel.LASTEDITTIME = DateTime.Now;
                                                hisservice.InsertReturnIdentity(hismodel);


                                                return Json(new
                                                {
                                                    result = true,
                                                    rank = "1",
                                                    endtime = projectmodel.ENDTIME.AddMinutes(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                                    msg = "报价成功"
                                                });

                                            }
                                            else
                                            {
                                                //更新detail
                                                var detailcount = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE & t.USERNAME ==HttpContextHelper.GetCurrentUserName());


                                                model = detailcount.FirstOrDefault();
                                                newmodel.ID = model.ID;
                                                newmodel.PROJECTCODE = PROJECTCODE;
                                                newmodel.USERNAME = model.USERNAME;

                                                newmodel.MYPRICE = int.Parse(MYPRICE);
                                                newmodel.PREPRICE = price;
                                                newmodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                                //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));

                                                newmodel.DESCRIBE = "";

                                                newmodel.BIDDTIME = DateTime.Now;

                                                newmodel.CREATOR = model.CREATOR;
                                                newmodel.CREATETIME = model.CREATETIME;
                                                newmodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                                newmodel.LASTEDITTIME = DateTime.Now;
                                                bodyservice.Update(newmodel);

                                              

                                                hismodel.ID = ResultHelper.NewId;
                                                hismodel.PROJECTCODE = PROJECTCODE;
                                                hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                                hismodel.MYPRICE = int.Parse(MYPRICE);
                                                hismodel.PREPRICE = price;
                                                hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                                hismodel.DESCRIBE = "";
                                                hismodel.BIDDTIME = DateTime.Now;
                                                hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                                hismodel.CREATETIME = DateTime.Now;
                                                hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                                hismodel.LASTEDITTIME = DateTime.Now;
                                                hisservice.InsertReturnIdentity(hismodel);
                                                return Json(new
                                                {
                                                    result = true,
                                                    rank = "1",
                                                    endtime = projectmodel.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    msg = "报价成功"
                                                });
                                            }


                                        }
                                        else
                                        {
                                            //更新detail
                                            var detailcount = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE & t.USERNAME ==HttpContextHelper.GetCurrentUserName());


                                            model = detailcount.FirstOrDefault();
                                            newmodel.ID = model.ID;
                                            newmodel.PROJECTCODE = PROJECTCODE;
                                            newmodel.USERNAME = model.USERNAME;

                                            newmodel.MYPRICE = int.Parse(MYPRICE);
                                            newmodel.PREPRICE = price;
                                            newmodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                            //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));

                                            newmodel.DESCRIBE = "";

                                            newmodel.BIDDTIME = DateTime.Now;

                                            newmodel.CREATOR = model.CREATOR;
                                            newmodel.CREATETIME = model.CREATETIME;
                                            newmodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            newmodel.LASTEDITTIME = DateTime.Now;
                                            bodyservice.Update(newmodel);


                                            hismodel.ID = ResultHelper.NewId;
                                            hismodel.PROJECTCODE = PROJECTCODE;
                                            hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                            hismodel.MYPRICE = int.Parse(MYPRICE);

                                            hismodel.PREPRICE = price;
                                            hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                            hismodel.DESCRIBE = "";
                                            hismodel.BIDDTIME = DateTime.Now;
                                            hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.CREATETIME = DateTime.Now;
                                            hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.LASTEDITTIME = DateTime.Now;
                                            hisservice.InsertReturnIdentity(hismodel);

                                            var sstemplist = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE).OrderBy(t => t.TRUEPRICE)
                                                                     .ThenBy(t => t.BIDDTIME).ToList();
                                            //查询排名
                                            int tempss = sstemplist.FindIndex(t => t.USERNAME ==HttpContextHelper.GetCurrentUserName()) + 1;
                                            return Json(new
                                            {
                                                result = true,
                                                rank = tempss.ToString(),
                                                endtime = projectmodel.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                                                msg = "报价成功"
                                            });


                                        }
                                    }
                                    else
                                    {
                                        //无人竞争出价时，替换竞价数据，增加历史数据
                                        var detailcount = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE & t.USERNAME ==HttpContextHelper.GetCurrentUserName());
                                     
                                      
                                            model = detailcount.FirstOrDefault();
                                            newmodel.ID = model.ID;
                                            newmodel.PROJECTCODE = PROJECTCODE;
                                            newmodel.USERNAME = model.USERNAME;

                                            newmodel.MYPRICE = int.Parse(MYPRICE);
                                            newmodel.PREPRICE = price;
                                            newmodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                        //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));

                                        newmodel.DESCRIBE = "";

                                            newmodel.BIDDTIME = DateTime.Now;

                                            newmodel.CREATOR = model.CREATOR;
                                            newmodel.CREATETIME = model.CREATETIME;
                                            newmodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            newmodel.LASTEDITTIME = DateTime.Now;
                                           bodyservice.Update(newmodel);
                                      

                                        hismodel.ID = ResultHelper.NewId;
                                        hismodel.PROJECTCODE = PROJECTCODE;
                                        hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                        hismodel.MYPRICE = int.Parse(MYPRICE);
                                        hismodel.PREPRICE = price;
                                        hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                        hismodel.DESCRIBE = "";
                                        hismodel.BIDDTIME = DateTime.Now;
                                        hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                        hismodel.CREATETIME = DateTime.Now;
                                        hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                        hismodel.LASTEDITTIME = DateTime.Now;
                                        hisservice.InsertReturnIdentity(hismodel);


                                        return Json(new
                                        {
                                            result = true,
                                            rank = "1",
                                            endtime = projectmodel.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                                            msg = "报价成功"
                                        });
                                    }
                                }
                            }
                            else
                            {
                                //比较最低出价，如果是最后一分钟出价且比最低价低则触发延时，否则执行正常报价逻辑
                                TimeSpan timeSpan = projectmodel.ENDTIME - now;
                                if (lowlist.Count > 0)
                                {
                                    //int.Parse(MYPRICE) < lowlist.First().MYPRICE &
                                    if ( (timeSpan.TotalSeconds > 0 & timeSpan.TotalSeconds < 60))
                                    {
                                        //更新项目结束时间 剩余延迟次数
                                        if (projectmodel.REMAINDELAYTIMES > 0)
                                        {   
                                            projectupmodel.ID = projectmodel.ID;
                                            projectupmodel.PROJECTCODE = PROJECTCODE;
                                            projectupmodel.ENDTIME = projectmodel.ENDTIME.AddMinutes(3);
                                            projectupmodel.REMAINDELAYTIMES = projectmodel.REMAINDELAYTIMES - 1;

                                            projectupmodel.COMPANYNAME = projectmodel.COMPANYNAME;
                                            projectupmodel.PROJECTNAME = projectmodel.PROJECTNAME; ;
                                            projectupmodel.PROJECTTYPE = projectmodel.PROJECTTYPE; ;
                                            projectupmodel.UNITPRICE = projectmodel.UNITPRICE; ;
                                            //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                                            projectupmodel.NUMBER = projectmodel.NUMBER;
                                            projectupmodel.DESCRIBE = projectmodel.DESCRIBE;
                                            projectupmodel.BUDGET = projectmodel.BUDGET;
                                            projectupmodel.MINPRICE = projectmodel.MINPRICE;
                                            projectupmodel.MAXPRICE = projectmodel.MAXPRICE;
                                            projectupmodel.RANGEPRICE = projectmodel.RANGEPRICE;
                                            projectupmodel.FILEADDRESS = projectmodel.FILEADDRESS;
                                            projectupmodel.DELAYTIMES = projectmodel.DELAYTIMES;
                                            projectupmodel.STARTTIME = projectmodel.STARTTIME;
                                            projectupmodel.STATE = projectmodel.STATE;
                                            projectupmodel.CUSTOMERS = projectmodel.CUSTOMERS;
                                            projectupmodel.BID = projectmodel.BID;
                                            projectupmodel.BIDDINGTYPE = projectmodel.BIDDINGTYPE;
                                            projectupmodel.BIDPRICE = projectmodel.BIDPRICE;
                                            projectupmodel.BIDTIME = projectmodel.BIDTIME;
                                            projectupmodel.CREATOR = projectmodel.CREATOR;
                                            projectupmodel.CREATETIME = projectmodel.CREATETIME;
                                            projectupmodel.LASTEDITOR = projectmodel.LASTEDITOR;
                                            projectupmodel.LASTEDITTIME = projectmodel.LASTEDITTIME;
                                            service.Update(projectupmodel);

                                            model.ID = ResultHelper.NewId;
                                            model.PROJECTCODE = PROJECTCODE;
                                            model.USERNAME =HttpContextHelper.GetCurrentUserName();
                                            model.MYPRICE = int.Parse(MYPRICE);
                                            model.PREPRICE = price;
                                            model.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                            model.DESCRIBE = "";
                                            model.BIDDTIME = DateTime.Now;
                                            model.CREATOR =HttpContextHelper.GetCurrentUserName();
                                            model.CREATETIME = DateTime.Now;
                                            model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            model.LASTEDITTIME = DateTime.Now;
                                            bodyservice.InsertReturnIdentity(model);

                                            hismodel.ID = ResultHelper.NewId;
                                            hismodel.PROJECTCODE = PROJECTCODE;
                                            hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                            hismodel.MYPRICE = int.Parse(MYPRICE);
                                            hismodel.PREPRICE = price;
                                            hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                            hismodel.DESCRIBE = "";
                                            hismodel.BIDDTIME = DateTime.Now;
                                            hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.CREATETIME = DateTime.Now;
                                            hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.LASTEDITTIME = DateTime.Now;
                                            hisservice.InsertReturnIdentity(hismodel);


                                            return Json(new
                                            {
                                                result = true,
                                                rank = "1",
                                                endtime = projectmodel.ENDTIME.AddMinutes(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                                msg = "报价成功"
                                            });

                                        }
                                        else
                                        {
                                            model.ID = ResultHelper.NewId;
                                            model.PROJECTCODE = PROJECTCODE;
                                            model.USERNAME =HttpContextHelper.GetCurrentUserName();
                                            model.MYPRICE = int.Parse(MYPRICE);
                                            model.PREPRICE = price;
                                            model.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                            model.DESCRIBE = "";
                                            model.BIDDTIME = DateTime.Now;
                                            model.CREATOR =HttpContextHelper.GetCurrentUserName();
                                            model.CREATETIME = DateTime.Now;
                                            model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            model.LASTEDITTIME = DateTime.Now;
                                            bodyservice.InsertReturnIdentity(model);

                                            hismodel.ID = ResultHelper.NewId;
                                            hismodel.PROJECTCODE = PROJECTCODE;
                                            hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                            hismodel.MYPRICE = int.Parse(MYPRICE);
                                            hismodel.PREPRICE = price;
                                            hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                            hismodel.DESCRIBE = "";
                                            hismodel.BIDDTIME = DateTime.Now;
                                            hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.CREATETIME = DateTime.Now;
                                            hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                            hismodel.LASTEDITTIME = DateTime.Now;
                                            hisservice.InsertReturnIdentity(hismodel);
                                            return Json(new
                                            {
                                                result = true,
                                                rank = "1",
                                                endtime = "",
                                                msg = "报价成功"
                                            });
                                        }


                                    }
                                    else
                                    {
                                        model.ID = ResultHelper.NewId;
                                        model.PROJECTCODE = PROJECTCODE;
                                        model.USERNAME =HttpContextHelper.GetCurrentUserName();
                                        model.MYPRICE = int.Parse(MYPRICE);
                                        model.PREPRICE = price;
                                        model.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                        model.DESCRIBE = "";
                                        model.BIDDTIME = DateTime.Now;
                                        model.CREATOR =HttpContextHelper.GetCurrentUserName();
                                        model.CREATETIME = DateTime.Now;
                                        model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                        model.LASTEDITTIME = DateTime.Now;
                                        bodyservice.InsertReturnIdentity(model);

                                     

                                        hismodel.ID = ResultHelper.NewId;
                                        hismodel.PROJECTCODE = PROJECTCODE;
                                        hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                        hismodel.MYPRICE = int.Parse(MYPRICE);
                                        hismodel.PREPRICE = price;
                                        hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                        hismodel.DESCRIBE = "";
                                        hismodel.BIDDTIME = DateTime.Now;
                                        hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                        hismodel.CREATETIME = DateTime.Now;
                                        hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                        hismodel.LASTEDITTIME = DateTime.Now;
                                        hisservice.InsertReturnIdentity(hismodel);

                                        var templist = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE).OrderBy(t => t.TRUEPRICE)
                                                                 .ThenBy(t => t.BIDDTIME).ToList();
                                        //查询排名
                                        int temp = templist.FindIndex(t=>t.USERNAME ==HttpContextHelper.GetCurrentUserName())+1;
                                        return Json(new
                                        {
                                            result = true,
                                            rank = temp.ToString(),
                                            endtime = projectmodel.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                                            msg = "报价成功"
                                        });


                                    }
                                }
                                else
                                {
                                    model.ID = ResultHelper.NewId;
                                    model.PROJECTCODE = PROJECTCODE;
                                    model.USERNAME =HttpContextHelper.GetCurrentUserName();
                                    model.MYPRICE = int.Parse(MYPRICE);
                                    model.PREPRICE = price;
                                    model.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);
                                    model.DESCRIBE = "";
                                    model.BIDDTIME = DateTime.Now;
                                    model.CREATOR =HttpContextHelper.GetCurrentUserName();
                                    model.CREATETIME = DateTime.Now;
                                    model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                    model.LASTEDITTIME = DateTime.Now;
                                    bodyservice.InsertReturnIdentity(model) ;

                                    hismodel.ID = ResultHelper.NewId;
                                    hismodel.PROJECTCODE = PROJECTCODE;
                                    hismodel.USERNAME =HttpContextHelper.GetCurrentUserName();
                                    hismodel.MYPRICE = int.Parse(MYPRICE);
                                    hismodel.PREPRICE = price;
                                    hismodel.TRUEPRICE = (int)(int.Parse(MYPRICE) * value);

                                    hismodel.DESCRIBE = "";
                                    hismodel.BIDDTIME = DateTime.Now;
                                    hismodel.CREATOR =HttpContextHelper.GetCurrentUserName();
                                    hismodel.CREATETIME = DateTime.Now;
                                    hismodel.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
                                    hismodel.LASTEDITTIME = DateTime.Now;
                                    hisservice.InsertReturnIdentity(hismodel);


                                    return Json(new
                                    {
                                        result = true,
                                        rank = "1",
                                        endtime = projectmodel.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss"),
                                        msg = "报价成功"
                                    });
                                }
                               
                            }
                        }
                      

                    }


                }
                else
                {
                    return Json(new
                    {
                        result = false,
                        rank = "",
                        endtime = "",
                        msg = "项目不存在或已删除，请重新刷新页面"
                    });

                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    rank = "",
                    endtime = "",
                    msg = "报错"+ex.Message
                });

            }
        }

        [HttpPost]
        public JsonResult GetMyData(string PROJECTCODE)
        {
          
          
            try
            {
                var sstemplist = bodyservice.Query(t => t.PROJECTCODE == PROJECTCODE).OrderBy(t => t.TRUEPRICE)
                                                                .ThenBy(t => t.BIDDTIME).ToList();
                //查询排名
                int tempss = sstemplist.FindIndex(t => t.USERNAME ==HttpContextHelper.GetCurrentUserName()) + 1;
                var projectcount = service.Query(t => t.PROJECTCODE == PROJECTCODE).FirstOrDefault();
                var endtime = projectcount.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss");
                var starttime =DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return Json(new
               {
                   result = true,
                   rank = tempss,
                    starttime = starttime,
                    endtime = endtime,
                   msg = "获取数据成功"
               });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    rank = "",
                    starttime = "",
                    endtime = "",
                    msg = "报错" + ex.Message
                });

            }
        }



    }


}