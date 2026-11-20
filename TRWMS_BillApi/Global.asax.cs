using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using TR.ORM;

namespace TRXWMS
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SqlSugarClientFactory.ScopeStoreResolver = () =>
                HttpContext.Current == null ? null : HttpContext.Current.Items;

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_EndRequest()
        {
            SqlSugarClientFactory.DisposeCurrent();
        }
    }
}
