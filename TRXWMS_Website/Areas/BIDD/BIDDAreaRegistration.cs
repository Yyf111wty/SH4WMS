using System.Web.Mvc;

namespace TRXWMS_Website.Areas.BIDD
{
    public class BIDDAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BIDD";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BIDD_default",
                "BIDD/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}