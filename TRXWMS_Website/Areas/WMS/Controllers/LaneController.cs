using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TR.ORM;
using TRXWMS.BILL.DAL;

namespace TRXWMS_Website.Areas.WMS.Controllers
{
    public class LaneController : Controller
    {
        public static DbContext dbcontext = new DbContext();
        // GET: WMS/Lane
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetLane()
        {

            string sql = @"
                    SELECT 
                        TUNNELNUM lane,
                        MIN(ENABLED) enabled
                    FROM wms_base_location
                    GROUP BY TUNNELNUM
                    ORDER BY lane
                ";


            var list = dbcontext.Db.Ado.SqlQuery<dynamic>(sql);


            return Json(new
            {
                success = true,
                data = list
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<JsonResult> updateLaneState(string lane, int enabled)
        {

            try
            {
                Console.WriteLine("开始执行，修改巷道状态");
                HttpClient client = new HttpClient();
                int enabled1 = enabled == 1 ? 0 : 1;


                string url = $"http://172.20.63.181:9902/api/wcs/WMSToWcsLangEnable" + $"?wmsLangNum={lane}&wmsLangEnabled={enabled1}";

                string json = await client.GetStringAsync(url);

                //string result2 = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(json);

                bool success = obj["IsSuccess"]?.Value<bool>() ?? false;
                Console.WriteLine("调用WCS接口，反馈："+ success);
                if (!success)
                {
                    return Json(new
                    {
                        success = false,
                        message = "WCS返回失败"
                    });
                    throw new Exception(
                        $"接口WCS调用失败:{json},返回:{obj}");
                }
                Console.WriteLine("调用WCS接口成功，开始修改数据库状态");
                string sql = "UPDATE wms_base_location SET ENABLED=@enabled WHERE TUNNELNUM=@Lane";
                int rows = SqlSugarClientFactory.ExecuteWithoutRequestSynchronizationContext(() =>
                    dbcontext.Db.Ado.ExecuteCommand(sql, new List<SugarParameter>()
                    {
                        new SugarParameter("@enabled", enabled),
                        new SugarParameter("@Lane", lane),
                    }));
                Console.WriteLine($"修改数据库状态成功，影响行数：{rows}");
                bool result = rows > 0 ? true : false;
                return Json(new
                {
                    success = result,
                    message = result ? "成功" : "失败"
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = ex.Message
                });

            }

        }
    }
}