using System;
using System.Web.Mvc;
using TR.ORM;

namespace TRXWMS_Website.Controllers
{
    /// <summary>
    /// 运维探活与数据库客户端诊断。不参与登录校验，供健康检查脚本调用。
    /// </summary>
    public class HealthController : Controller
    {
        [HttpGet]
        public ActionResult Ready()
        {
            try
            {
                var db = SugarContext.GetInstance();
                int ok = db.Ado.GetInt("SELECT 1");
                return Content(ok == 1 ? "ok" : "fail");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 503;
                Response.TrySkipIisCustomErrors = true;
                return Content("db unavailable: " + ex.Message);
            }
        }

        [HttpGet]
        public ActionResult Db()
        {
            return Content(SqlSugarClientFactory.GetDiagnostics());
        }
    }
}
