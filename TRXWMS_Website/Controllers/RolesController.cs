using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using TR.Common;
using TR.Model;
using TRWMS_Website.Filters;
using TRWMS_Website.Lib;
using TRWMS_Website.ViewModels;
using TRXWMS_Website.Helper;

namespace TRWMS_Website.Controllers
{
    public class RolesController : Controller
    {
        public static List<RolesViewModel> roleList;
        public static List<UsersViewModel> userRolesList;

        //[CheckLoginFilter]
        public ActionResult Index()
        {

            using (var client = new HttpClient())
            {
                var rolesResult = TRWMS_PrApi.Controllers.RoleController.TRWCS_GetRolesDataApi();
                roleList = JsonConvert.DeserializeObject<List<RolesViewModel>>(JsonConvert.SerializeObject(rolesResult));
                var userRolesResult = TRWMS_PrApi.Controllers.UserController.TRWCS_GetUserRoleApi();
                userRolesList = JsonConvert.DeserializeObject<List<UsersViewModel>>(JsonConvert.SerializeObject(userRolesResult));
            }
            return View();
        }
        /// <summary>
        /// 新增或编辑
        /// </summary>
        /// <returns></returns>
       // [CheckLoginFilter]
        public ActionResult AddOrEdit()
        {
            return View();
        }
        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRoles()
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            //int pageSize = Request["rows"] == "NaN" ? 20 : int.Parse(Request["rows"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数            
            return Json(PagingHelper.GetPagingData(roleList, pageSize, pageId), JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 根据角色获取用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public JsonResult GetUsers(int roleid)
        {
            List<UsersViewModel> childuserList = new List<UsersViewModel>();
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数    
            foreach (var item in userRolesList)
            {
                if (item.RoleId == roleid)
                {
                    childuserList.Add(item);
                }
            }
            return Json(PagingHelper.GetPagingData(childuserList, pageSize, pageId), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="describe"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddRole(string roleName, string describe, int isEnabled, string roleLevel)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    RoleName = roleName,
                    RoleLevel = roleLevel,
                    IsEnabled = isEnabled,
                    Describes = describe,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.RoleController.TRWCS_AddRolesApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <param name="describe"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditRole(int roleId, string roleName, string describe, int isEnabled, string roleLevel)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = roleId,
                    RoleName = roleName,
                    RoleLevel = roleLevel,
                    IsEnabled = isEnabled,
                    Describes = describe,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.RoleController.TRWCS_EditRolesApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteRole(int roleId)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = roleId,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.RoleController.TRWCS_DeleteRolesApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}