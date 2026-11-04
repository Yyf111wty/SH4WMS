using System;
using System.Collections.Generic;
using System.IO;
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
    //[CheckLoginFilter]
    public class BiddingProjectController : Controller
    {
        // GET: BIDD/BiddingProject
        jc_bidding_projectService service = new jc_bidding_projectService();
        jc_bidding_detailService bodyservice = new jc_bidding_detailService();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetRowData(jc_bidding_project JC_BIDDING_PROJECT)
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
                      && x.BIDDINGTYPE.Equals("标准项目")
                     && (JC_BIDDING_PROJECT.STATE == -10 || x.STATE.Equals(JC_BIDDING_PROJECT.STATE))   ,
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

        public ActionResult AddOrEdit()
        {
            return View();
        }

        /// <summary>
        /// 查看详情
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetMyData(string PROJECTCODE)
        {


            try
            {
               
                var projectcount = service.Query(t => t.PROJECTCODE == PROJECTCODE).FirstOrDefault();
                var endtime="";
                if (projectcount != null)
                {
                     endtime = projectcount.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss");
                    var starttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    return Json(new
                    {
                        result = true,                       
                        starttime = starttime,
                        endtime = endtime,
                        msg = "获取数据成功"
                    });
                }
                else
                {
                    
                    return Json(new
                    {
                        result = false,
                        starttime = "",
                        endtime = endtime,
                        msg = "获取数据失败"
                    });
                }
             


           
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,

                    endtime = "",
                    msg = "报错" + ex.Message
                });

            }
        }

        public ActionResult GetBodyData(string PROJECTCODE,string type)
        {
            //var projectcount = service.Query(t => t.PROJECTCODE == PROJECTCODE).FirstOrDefault();
            //var endtime = projectcount.ENDTIME.ToString("yyyy-MM-dd HH:mm:ss");

            int TotalCount = 0;
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数 
            List<BiddingProjectDetailModel> lists = null;
            //根据用户名显示内容
           // string uid =HttpContextHelper.GetCurrentUserName();

            try
            {
                if (type == "0")
                {
                    lists = bodyservice.Getlist(a => ((a.PROJECTCODE == PROJECTCODE) & a.DESCRIBE != "1"), ref TotalCount, pageId, pageSize, null).OrderBy(t => t.MYPRICE)
                                                                .ThenBy(t => t.BIDDTIME).ToList();

                }
                else
                {
                    lists = bodyservice.Getlist(a => ((a.PROJECTCODE == PROJECTCODE) & a.DESCRIBE == "1"), ref TotalCount, pageId, pageSize, "BIDDTIME DESC").ToList();

                }


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
        public JsonResult AddProduct(string COMPANYNAME, string PROJECTCODE, string PROJECTNAME, string PROJECTTYPE, string UNITPRICE, string NUMBER, string DESCRIBE, string BUDGET, string MINPRICE,string MAXPRICE, string RANGEPRICE, string STARTTIME, string ENDTIME, string DELAYTIMES, string FILEADDRESS)
        {
            jc_bidding_project model = new jc_bidding_project();
            try
            {
                //编码不能重复
              
                var codecount = service.Query(t => t.PROJECTCODE == PROJECTCODE).Count;
                if (codecount > 0)
                {
                    return Json(new { result = "项目号重复！" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                   
                    model.ID = ResultHelper.NewId;
                    model.COMPANYNAME = COMPANYNAME;
                    model.PROJECTCODE = PROJECTCODE;
                    model.PROJECTNAME = PROJECTNAME;
                    model.PROJECTTYPE = PROJECTTYPE;
                    model.UNITPRICE = UNITPRICE;
                    //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                    model.NUMBER = int.Parse(NUMBER);
                    model.DESCRIBE = DESCRIBE;
                    model.FILEADDRESS = FILEADDRESS;
                    model.BIDDINGTYPE = "标准项目";
                    model.DELAYTIMES = int.Parse(DELAYTIMES);
                    model.REMAINDELAYTIMES = int.Parse(DELAYTIMES);
                    model.BUDGET = int.Parse(BUDGET); ;
                    model.MINPRICE = int.Parse(MINPRICE);
                    model.MAXPRICE = int.Parse(MAXPRICE);
                    model.RANGEPRICE = int.Parse(RANGEPRICE);
                    
                    model.STARTTIME = Convert.ToDateTime(STARTTIME);
                    model.ENDTIME = Convert.ToDateTime(ENDTIME);
                    // model.XYZ = XYZ;
                    //  model.CHECKXYZ = CHECKXYZ;
                    // model.SPECS = SPECS;
                    //model.IFMIXING = IFMIXING;
                    DateTime  now= DateTime.Now;
                    if (now > Convert.ToDateTime(ENDTIME))
                    {
                        model.STATE = 1;
                    }
                    else
                    {
                        model.STATE = 0;
                    }
                   
                    model.CREATOR =HttpContextHelper.GetCurrentUserName();
                    model.CREATETIME = DateTime.Now;
                    model.LASTEDITOR =HttpContextHelper.GetCurrentUserName();
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
        public JsonResult EditProduct(string ID, string COMPANYNAME, string PROJECTCODE, string PROJECTNAME, string PROJECTTYPE, string UNITPRICE, string NUMBER, string DESCRIBE, string BUDGET, string MINPRICE, string MAXPRICE, string RANGEPRICE, string STARTTIME, string ENDTIME, string DELAYTIMES, string FILEADDRESS)
        {
            jc_bidding_project model = new jc_bidding_project();
            jc_bidding_project oldmodel = new jc_bidding_project();
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
                model.COMPANYNAME = COMPANYNAME;
                model.PROJECTCODE = PROJECTCODE;
                model.PROJECTNAME = PROJECTNAME;
                model.PROJECTTYPE = PROJECTTYPE;
                model.UNITPRICE = UNITPRICE;
                //model.COLUMNNUM = int.Parse(LOCATIONCODE.Substring(2, 2));
                model.NUMBER = int.Parse(NUMBER);
                model.DESCRIBE = DESCRIBE;
                model.BUDGET = int.Parse(BUDGET); ;
                model.MINPRICE = int.Parse(MINPRICE);
                model.MAXPRICE = int.Parse(MAXPRICE);
                model.RANGEPRICE = int.Parse(RANGEPRICE);
                model.FILEADDRESS = FILEADDRESS;
                model.DELAYTIMES = int.Parse(DELAYTIMES);
                model.REMAINDELAYTIMES = int.Parse(DELAYTIMES);
                model.STARTTIME = Convert.ToDateTime(STARTTIME);
                model.ENDTIME = Convert.ToDateTime(ENDTIME);
                DateTime now = DateTime.Now;
                if (now > Convert.ToDateTime(ENDTIME))
                {
                    model.STATE = 1;
                }
                else
                {
                    model.STATE = 0;
                }
                model.BIDDINGTYPE = oldmodel.BIDDINGTYPE;
                model.BIDPRICE= oldmodel.BIDPRICE;
                model.BIDTIME = oldmodel.BIDTIME;
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



        [HttpPost]
        public JsonResult DeleteProduct(string PROJECTCODE)
        {
           
            var result = true;
            try
            {

                result = bodyservice.DeleteProject(PROJECTCODE);
                return Json(new { result = result }, JsonRequestBehavior.AllowGet);
            

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// layui文件上传demo
        /// </summary>
        /// <returns></returns>
       
        public ActionResult Upload()
        {

            try
            {
                var file = Request.Files[0];
                var filecombin = file.FileName.Split('.');
                if (file == null || String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0 || filecombin.Length < 2)
                {
                    return Json(new
                    {
                        fileid = 0,
                        src = "",
                        name = "",
                        msg = "上传出错 请检查文件名 或 文件内容"
                    });
                }
                ////定义本地路径位置
                string slocal = "";
                //var s = typebodyservice.Query(t => t.typecode == "SOP图片地址" & t.detailcode == "imgurl");
                //if (s.Count > 0)
                //{
                //    slocal = s.FirstOrDefault().detailname;
                //}
                string local = "Upload\\";
                string filePathName = string.Empty;
                string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, local);

                var tmpName = Server.MapPath("~/Upload/");
                var tmp = file.FileName;
                //var tmpIndex = 0;
                //判断是否存在相同文件名的文件 存在则删除源文件
                while (System.IO.File.Exists(tmpName + tmp))
                {
                    System.IO.File.Delete(tmpName + tmp);
                    //tmp = filecombin[0] + "_" + ++tmpIndex + "." + filecombin[1];
                }

                //不带路径的最终文件名
                filePathName = tmp;

                if (!System.IO.Directory.Exists(localPath))
                    System.IO.Directory.CreateDirectory(localPath);
                string localURL = Path.Combine(local, filePathName);
                file.SaveAs(Path.Combine(localPath, filePathName));   //保存图片（文件夹）
                
                return Json(new
                {
                    src = (slocal + localURL).Trim().Replace("\\", "/"),
                    name = Path.GetFileName(file.FileName),   // 获取文件名不含后缀名
                    msg = "上传成功",
                    fileName = "/" + filePathName//最终返回的路径及文件名称

                });
            }
            catch (Exception ee)
            {
                return Json(new
                {
                    src = "",
                    name = "",   // 获取文件名不含后缀名
                    msg = "上传出错",
                    fileName = ""
                });
            }


        }

    }
}