using System.Web;
using System.Web.Mvc;
using TRWMS_Website.Filters;

namespace TRXWMS_Website
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new WmsOperationLogFilterAttribute());
        }
    }
}
