using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    public class RolePermissionController : Controller
    {

        public static List<RolesViewModel> roleList = new List<RolesViewModel>();
        public static List<Rows> permissionList;
        public static List<Rows> rolesPermissionList;
        public static List<Rows> rolePermissionList;

       // [CheckLoginFilter]
        public ActionResult Index()
        {
            using (var client = new HttpClient())
            {
                //获取角色
                var rolesResult = TRWMS_PrApi.Controllers.RoleController.TRWCS_GetRolesDataApi();
                roleList = JsonConvert.DeserializeObject<List<RolesViewModel>>(JsonConvert.SerializeObject(rolesResult));
                //获取权限列表
                var permissionResult = TRWMS_PrApi.Controllers.PermissionController.TRWCS_GetPermissionsDataApi("");
                permissionList = JsonConvert.DeserializeObject<List<Rows>>(JsonConvert.SerializeObject(permissionResult));
                if (permissionList.Count > 0)
                {
                    foreach (var item in permissionList)
                    {
                        var check = permissionList.Find(t => t._parentId == item.NodeId);
                        if (check != null)
                        {
                            item.state = "closed";
                        }
                        else
                        {
                            item.state = "open";
                        }
                    }
                }
                //获取全部角色权限列表
                var result = TRWMS_PrApi.Controllers.RolePermissionController.TRWCS_GetRolePermissionApi();
                rolesPermissionList = JsonConvert.DeserializeObject<List<Rows>>(JsonConvert.SerializeObject(result));
            }
            return View();
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRoles()
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数            
            return Json(PagingHelper.GetPagingData(roleList, pageSize, pageId), JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 获取treegrid
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList()
        {

        PermissionViewModel model = new PermissionViewModel();
            model.total = permissionList.Count.ToString();
            model.rows = permissionList;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取treetable数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRolesTreeTableData()
        {

            var data = "";
           
            string sbs = string.Empty;
            if (permissionList.Count == 0)
            {
                sbs = "[]";
                data= sbs.ToString();              
                return Json(new { data=data}, JsonRequestBehavior.AllowGet);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (permissionList.Count > 0)
            {
                for (int i = 0; i < permissionList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + permissionList[i].NodeId + "");
                    builder.Append(",");
                    builder.Append("\"pid\":" + permissionList[i]._parentId + "");
                    builder.Append(",");
                    builder.Append("\"NodeName\":\"" + permissionList[i].NodeName + "\"");
                    builder.Append(",");
                    builder.Append("\"Type\":\"" + permissionList[i].Type + "\"");
                    builder.Append(",");
                    builder.Append("\"NodeType\":\"" + permissionList[i].NodeType + "\"");
                    builder.Append(",");
                    builder.Append("\"IsEnabled\":\"" + permissionList[i].IsEnabled + "\"");
                    if (i == permissionList.Count - 1)
                    {
                        builder.Append("}");
                    }
                    else
                    {
                        builder.Append("},");
                    }
                }
            }
            builder.Append("]");
            data = builder.ToString();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置角色权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetRolePermission(int roleId, string permissionList)
        {
            HttpResponseModel hm = new HttpResponseModel();
            permissionList = permissionList.Substring(0, permissionList.Length - 1).ToString();
            try
            {
                List<string> list = new List<string>(permissionList.Split(','));    //string转list
                var content = new
                {
                    RoleId = roleId,
                    PermissionList = list,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.RolePermissionController.TRWCS_SetRolePermissionApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 设置角色权限layui
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetRolePermissionFor(int roleId, string permissionList)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                List<string> list = new List<string>(permissionList.Split(','));
                var content = new
                {
                    RoleId = roleId,
                    PermissionList = list,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.RolePermissionController.TRWCS_SetRolePermissionApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据角色获取权限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRolePermission(int roleId)
        {
            List<int> list = new List<int>();
            if (rolesPermissionList.Count > 0)
            {
                foreach (var item in rolesPermissionList)
                {
                    if (item.RoleId == roleId)
                    {
                        list.Add(item.NodeId);
                    }
                }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}