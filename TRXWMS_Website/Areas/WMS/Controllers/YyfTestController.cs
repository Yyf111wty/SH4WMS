using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    public class YyfTestController: Controller
    {
        // GET: /YyfTest/Index
        public ActionResult Index()
        {
            ViewBag.Message = "页面渲染测试";
            return View();
        }

        // GET: /YyfTest/Detail/123
        public ActionResult Detail(string id)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}