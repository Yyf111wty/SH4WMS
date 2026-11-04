using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRWMS_Website.Lib;
using TRWMS_Website.ViewModels;
using TRXWMS_Website.Helper;

namespace TRWMS_Website.Controllers
{
    //[LogExceptionAttribute]
    public class LoginController : Controller
    {
        public static bool getPermission = false;
        //stepService stepservice = new stepService();
        //machineService machineservice = new machineService();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        //[JsonException]
        public JsonResult GetLogin(string userName, string password)
        {
            HttpResponseModel result = new HttpResponseModel();
            string unixTime = ConvertDateTimeInt(System.DateTime.Now).ToString();
            string sign = MD5Encrypt.Encrypt(userName + password + unixTime);
            object content = 
            new {
                userName = userName,
                password = password,          
                sign = sign,
                unixTime = unixTime,
            };
            JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
            var resultJson = TRWMS_PrApi.Controllers.LoginController.TRWCS_LoginApi(contents);
            result = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(resultJson));
            //result = HttpRequestHelper.HttpPostMethod(InterfaceConfig.LoginApi_url, content);

            if (result.result == true)
            {
                var permissions = UserPermissionCache.LoadPermissions(Session, userName);
                getPermission = permissions != null && permissions.Count > 0;
               
                //string groupname = "";//组别
                //string groupid = "";//组别id
                //string gwname = "";//工位
                //string gwid = "";//工位id
                ////检测mac地址   组别  工位（设备 mac地址）   账号  姓名
                //if (!(userName == "admin" || userName == "superadmin"))
                //{
                //    //获取组别
                //    groupname = GetGroup(result.UserGroupId.ToString()).Split(',')[0];
                //    groupid = GetGroup(result.UserGroupId.ToString()).Split(',')[1];
                //    //根据账号 及ip地址获取工位
                //    string str = GetMac.GetMacByNetworkInterface();
                //    string mac = str;

                //    var results = stepservice.Query(t=>t.emp_id.Contains(result.UserId)).ToList();

                //    string machineids = "";
                //    foreach (var item in results)
                //    {
                //        machineids += item.machine_id + ",";
                //    }
                //    var gwstr = "";
                //    var gwidstr = "";
                //    if (machineids != "")
                //    {
                //        string[] strmc = machineids.Substring(0, machineids.Length - 1).Split(',');
                //        //10.29改动包含ip
                //        var mcget = machineservice.Query(t => strmc.Contains(t.machine_id.ToString()) & t.machine_code.Contains(mac)).ToList();

                //        if (mcget.Count == 1)
                //        {
                //            gwstr = mcget.FirstOrDefault().machine_name;
                //            gwidstr = mcget.FirstOrDefault().machine_id.ToString();
                //        }
                //        gwname = gwstr;
                //        gwid = gwidstr;
                //    }

                //}
                if (getPermission == true)
                {
                    // ====================== 新Token ======================
                    string token = TokenHelper.CreateToken(userName); // 替换这一行
                    result.token = token;

                    Session["username"] = userName;
                    Session["truename"] = result.trueName;
                    base.Session.Timeout = Convert.ToInt32(ConfigurationSettings.AppSettings["TimeOut"].ToString());
                    //Session["groupname"] = groupname;
                    //Session["gwname"] = gwname;
                    //Session["groupid"] = groupid;
                    //Session["gwid"] = gwid;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    //Session["username"] = "";
                    //Session["truename"] = "";
                    //Session["groupname"] = "";
                    //Session["gwname"] = "";
                    //Session["groupid"] = "";
                    //Session["gwid"] = "";
                    return Json(new { result = false, message = "该用户未分配角色，无法登录！" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public string GetGroup(string UserGroupId)
        {
            string result = "";
            using (var client = new HttpClient())
            {
                var s = TRWMS_PrApi.Controllers.GroupsController.TRWCS_GetGroupsDataApi();
                var  listGroups = JsonConvert.DeserializeObject<List<GroupsViewModel>>(JsonConvert.SerializeObject(s));
                result = listGroups.Where(t => t.GroupId.ToString() == UserGroupId).FirstOrDefault().GroupName+","+ listGroups.Where(t => t.GroupId.ToString() == UserGroupId).FirstOrDefault().GroupId;

            }
            return result;
        }
       
        /// <summary>
        ///根据用户获取权限列表
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool GetUserPermissions(string userName)
        {
            try
            {
                var permissions = UserPermissionCache.GetPermissions(Session, userName);
                return permissions != null && permissions.Count > 0;
            }
            catch
            {
                return false;
            }
        }
        public static long ConvertDateTimeInt(System.DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
    }
}