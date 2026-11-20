using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TRXWMS;
using TRXWMS_Website.Helper;
using TR.ORM;

namespace TRXWMS_Website
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string RequestTimerKey = "__TR_RequestTimer";
        private const string RequestExceptionKey = "__TR_RequestException";

        protected void Application_Start()
        {
            SqlSugarClientFactory.ScopeStoreResolver = () =>
                HttpContext.Current == null ? null : HttpContext.Current.Items;

            int dbCommandTimeout;
            if (int.TryParse(ConfigurationManager.AppSettings["DbCommandTimeoutSeconds"], out dbCommandTimeout)
                && dbCommandTimeout > 0)
            {
                SqlSugarClientFactory.CommandTimeoutSeconds = dbCommandTimeout;
            }

            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RegisterOptionalMenus();

            // 启动阶段会访问数据库，释放掉启动线程上的客户端，避免它被后续请求复用
            SqlSugarClientFactory.DisposeCurrent();

            // 后台预热视图与数据库：把冷启动的 8~20 秒成本从"第一个用户"转移到"应用启动后台"
            ColdStartWarmer.WarmUpInBackground();
        }

        /// <summary>
        /// 菜单注册只是兜底补齐，失败不能影响网站启动。
        /// </summary>
        private static void RegisterOptionalMenus()
        {
            try
            {
                LocationMapMenuRegistrar.Register();
            }
            catch
            {
            }

            try
            {
                BdjInventoryMenuRegistrar.Register();
            }
            catch
            {
            }

            try
            {
                SmallDrillWorkOrderMenuRegistrar.Register();
            }
            catch
            {
            }

            try
            {
                SystemLogMenuRegistrar.Register();
            }
            catch
            {
            }
        }

        protected void Application_BeginRequest()
        {
            // 便于用浏览器/抓包直接看到服务端耗时（X-Server-Elapsed-Ms），定位是服务端慢还是网络慢
            Context.Items[RequestTimerKey] = Stopwatch.StartNew();
        }

        protected void Application_Error()
        {
            try
            {
                Context.Items[RequestExceptionKey] = Server.GetLastError();
            }
            catch
            {
            }
        }

        protected void Application_EndRequest()
        {
            var timer = Context.Items[RequestTimerKey] as Stopwatch;
            if (timer != null)
            {
                timer.Stop();
                try
                {
                    Response.AddHeader("X-Server-Elapsed-Ms", timer.ElapsedMilliseconds.ToString());
                }
                catch
                {
                }

                SlowRequestLogger.Write(
                    Context,
                    timer.ElapsedMilliseconds,
                    Context.Items[RequestExceptionKey] as Exception);
            }

            SqlSugarClientFactory.DisposeCurrent();
        }
    }
}
