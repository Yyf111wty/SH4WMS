using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TRWMS_Website.Filters
{
	public class CheckLoginFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            #region Session判断
            //if (filterContext.HttpContext.Session["username"] == null && !filterContext.ActionDescriptor.ActionName.Contains("Login"))
            //{
            //    filterContext.Result = new RedirectResult("/Login/Index");
            //    return;
            //}
            //

            //base.OnActionExecuting(filterContext);

            //base.OnActionExecuting(filterContext);
            //if (filterContext.HttpContext.Session == null || filterContext.HttpContext.Session["username"] == null)
            //{
            //    // 如果Session不存在，表示用户未登录或登录已过期
            //    // 重定向到登录页面
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
            //    { "controller", "Login" },
            //    { "action", "Index" },
            //    { "area", "" }
            //});
            //}
            //var controller = filterContext.Controller as Controller;
            //if (controller != null)
            //{
            //    var session = controller.Session;
            //    if (session == null || session["username"] == null)
            //    {
            //        filterContext.Result = new RedirectResult("/Login/index");
            //        return;
            //        // 用户信息不存在，Session可能已过期
            //        //filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
            //        //{ "controller", "Login" },
            //        //{ "action", "Index" },
            //        //{ "area", "" }
            //        //});
            //    }
            //}

            //base.OnActionExecuting(filterContext);
            #endregion
            #region  老代码
            var urlHelper = new UrlHelper(filterContext.RequestContext);
            var url = urlHelper.Action("Index", "Login");
            var contentResult = new ContentResult
            {
                Content = "<script>window.opener=null; window.top.location='/Login/Index';</script>"
            };
            //判断是否登陆
            if (filterContext.HttpContext.Session == null || filterContext.HttpContext.Session["username"] == null) // || !IsTimeOut()
            {
                //if (filterContext.HttpContext.Request.IsAjaxRequest())
                //{
                //    filterContext.Result = contentResult;
                //    filterContext.HttpContext.Response.AppendHeader("sessionstatus", "timeout");
                //    filterContext.HttpContext.Response.End();
                //}
                //else
                //{
                //    filterContext.Result = contentResult;
                //}
                filterContext.Result = contentResult;
            }
            base.OnActionExecuting(filterContext);
            #endregion
            //if (filterContext.HttpContext != null)
            //{
            //    if (HttpContext.Session["UserId"] == null)
            //    {
            //        if (filterContext.HttpContext.Request.IsAjaxRequest())
            //        {
            //            Response.AppendHeader("sessionstatus", "timeout");
            //            Response.End();
            //        }
            //        else
            //        {
            //            filterContext.Result = RedirectToRoute("Default", new { Controller = "Login", Action = "Index" });
            //        }
            //        //filterContext.Result = new RedirectResult("/Login/Index");
            //    }
            //}
            //base.OnActionExecuting(filterContext);
            //HttpCookieCollection CookieCollect = System.Web.HttpContext.Current.Request.Cookies;
            //if (CookieCollect["username"] == null || CookieCollect["password"] == null)
            //{
            //    filterContext.Result = new RedirectResult("/Login/Index");
            //}
        }


        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    base.OnActionExecuted(filterContext);
        //}
    }
}