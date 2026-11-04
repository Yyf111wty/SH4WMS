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
using TR.ORM.Model;
using TR.Service;
using TRWMS_Website.Filters;
using TRWMS_Website.Lib;
using TRWMS_Website.ViewModels;
using TRXWMS_Website.Helper;

namespace TRWMS_Website.Controllers
{
    public class UsersController : Controller
    {
        sys_usersService service = new sys_usersService();
        //typebodyService codeservice = new typebodyService();
        private static List<GroupsViewModel> listGroups;
        private static List<UsersViewModel> userList;
        private static List<RolesViewModel> roleList;
        private static List<UsersViewModel> userRolesList;

        //[CheckLoginFilter]
        public ActionResult Index()
        {

            using (var client = new HttpClient())
            {
                //获取组别
                var result = TRWMS_PrApi.Controllers.GroupsController.TRWCS_GetGroupsDataApi();
                listGroups = JsonConvert.DeserializeObject<List<GroupsViewModel>>(JsonConvert.SerializeObject(result));
                if (listGroups.Count > 0)
                {
                    foreach (var item in listGroups)
                    {
                        var check = listGroups.Find(t => t.ParentGroupId == item.GroupId);
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
                //获取用户
                var userResult = TRWMS_PrApi.Controllers.UserController.TRWCS_GetUsersDataApi().OrderByDescending(t=>t.CreateTime);               
                userList = JsonConvert.DeserializeObject<List<UsersViewModel>>(JsonConvert.SerializeObject(userResult));
                //获取用户角色
                var userRolesResult = TRWMS_PrApi.Controllers.UserController.TRWCS_GetUserRoleApi();
                userRolesList = JsonConvert.DeserializeObject<List<UsersViewModel>>(JsonConvert.SerializeObject(userRolesResult));
                //获取角色
                var rolesResult = TRWMS_PrApi.Controllers.RoleController.TRWCS_GetRolesDataApi();     
                roleList = JsonConvert.DeserializeObject<List<RolesViewModel>>(JsonConvert.SerializeObject(rolesResult));
            }
            return View();
        }

        [HttpPost]
        public JsonResult Register(string UserName, string Password, string Mobile, string roleidStr)
        {
            //新增用户同时设置角色
            HttpResponseModel hm = new HttpResponseModel();
          
            try
            {
                var content = new
                {
                    UserName = UserName,
                    Password = Password,
                    TrueName = UserName,
                    UserGroupId = 5,
                    Mobile = Mobile,
                    Email = "",
                    SecretKeyType = "",
                    SecretKeyValue = "",
                    CreateOperator = UserName
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_AddUsersApi(contents)));

                //roleidStr = roleidStr.Substring(1, roleidStr.Length - 1).ToString();
                //List<string> roleidList = new List<string>(roleidStr.Split(','));
                //var  aa=   service.Query(x=>x.UserName==UserName).FirstOrDefault();
                //  var content1= new
                //{
                //    Userid = aa.UserId,
                //    RoleIdList = roleidList,
                //    CreateOperator = UserName
                //  };
                //JObject contents1 = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content1));
                //hm1 = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_SetUserRoleApi(contents1)));


                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

           
           
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="selectGroupid"></param>
        /// <returns></returns>
        public JsonResult GetUsers(int selectGroupid)
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数           
            List<UsersViewModel> childList = new List<UsersViewModel>();
            if (selectGroupid == 0)
            {
                return Json(PagingHelper.GetPagingData(userList, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
            else if (listGroups.Find(t => t.GroupId == selectGroupid).ParentGroupId == 0)
            {
                return Json(PagingHelper.GetPagingData(userList, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                childList = userList.Where(x => x.UserGroupId == selectGroupid).ToList();
                if (childList.Count > 0)
                {
                    return Json(PagingHelper.GetPagingData(childList, pageSize, pageId), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    foreach (var item in userList)
                    {
                        if (item.ParentGroupId == selectGroupid)
                        {
                            childList.Add(item);
                        }
                    }
                    return Json(PagingHelper.GetPagingData(childList, pageSize, pageId), JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 获取密码
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPWDByCode(string code)
        {
            string pwd = MD5Encrypt.Decrypt(code);
            return Json(pwd, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 根据用户获取角色信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public JsonResult GetUserRoles(string userName)
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数      
            List<UsersViewModel> childList = new List<UsersViewModel>();
            if (userRolesList.Count > 0)
            {
                foreach (var item in userRolesList)
                {
                    if (item.UserName == userName)
                    {
                        childList.Add(item);
                    }
                }
                return Json(PagingHelper.GetPagingData(childList, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRoles()
        {
            roleList = roleList.Where(x => x.IsEnabled == 1).ToList();
            return Json(roleList, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 角色列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRolesSelectData()
        {
            List<RolesViewModel> parentmodelList = new List<RolesViewModel>();
            parentmodelList = roleList.Where(x => x.IsEnabled == 1).ToList(); //初始化先获取父类id=0的数据集
            string sbs = string.Empty;
            if (parentmodelList.Count == 0)
            {
                sbs = "[]";
                var s = sbs.ToString();
                return Json(s, JsonRequestBehavior.AllowGet);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (parentmodelList.Count > 0)
            {
                for (int i = 0; i < parentmodelList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("name:\"" + parentmodelList[i].RoleName + "\"");
                    builder.Append(",");
                    builder.Append("value:" + parentmodelList[i].RoleId);
                    builder.Append("},");
                }

            }
            builder.Append("]");
            var result = builder.ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增或编辑
        /// </summary>
        /// <returns></returns>
        //[CheckLoginFilter]
        public ActionResult AddOrEdit()
        {
            return View();
        }
        /// <summary>
        /// 设置角色
        /// </summary>
        /// <returns></returns>
       // [CheckLoginFilter]
        public ActionResult RoleSet()
        {
            return View();
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
       // [CheckLoginFilter]
        public ActionResult PwdSet()
        {
            return View();
        }
        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="trueName"></param>
        /// <param name="userGroupId"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="SecretKeyType"></param>
        /// <param name="SecretKeyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddUsers(string userName, string password, string trueName, int userGroupId, string mobile, string email, string SecretKeyType, string SecretKeyValue)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var create = HttpContextHelper.GetCurrentUserName();
                var content = new
                {
                    UserName = userName,
                    Password = password,
                    TrueName = userName,
                    UserGroupId = userGroupId,
                    Mobile = mobile,
                    Email = email,
                    SecretKeyType = SecretKeyType,
                    SecretKeyValue = SecretKeyValue,
                    CreateOperator = create
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_AddUsersApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="trueName"></param>
        /// <param name="userGroupId"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="SecretKeyType"></param>
        /// <param name="SecretKeyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditUsers(int id, string userName, string password, string trueName, int userGroupId, string mobile, string email, string SecretKeyType, string SecretKeyValue)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var create = HttpContextHelper.GetCurrentUserName();
                var content = new
                {
                    Id = id,
                    UserName = userName,
                    Password = password,
                    TrueName = userName,
                    UserGroupId = userGroupId,
                    Mobile = mobile,
                    Email = email,
                    SecretKeyType = SecretKeyType,
                    SecretKeyValue = SecretKeyValue,
                    UpdateOperator = create
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_EditUsersApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteUsers(int id)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = id,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_DeleteUsersApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetPlatformList()
        {
            //var lists = codeservice.Query(t => t.typecode == "秘钥类型");
            var Platforms = "<option value=''>秘钥类型</option>";
            //foreach (var item in lists)
            //{
            //    Platforms += "<option value = '" + item.detailcode + "'> " + item.detailname + " </option> ";
            //}
            return Json(Platforms, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置用户角色
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="roleid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetUserRoles(int userid, string roleidStr)
        {
            roleidStr = roleidStr.Substring(1, roleidStr.Length - 1).ToString();
            List<string> roleidList = new List<string>(roleidStr.Split(','));
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Userid = userid,
                    RoleIdList = roleidList,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_SetUserRoleApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetPassword(string userName, string newpassword)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    UserName = userName,
                    Password = newpassword
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_ResetPassword(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 绑定组别下拉框
        /// </summary>
        /// <returns></returns>
        public string GetTreeSelectData()
        {
            List<GroupsViewModel> parentmodelList = new List<GroupsViewModel>();
            parentmodelList = listGroups.Where(x => x.ParentGroupId == 0).ToList();  //初始化先获取父类id=0的数据集
            string sbs = string.Empty;
            if (parentmodelList.Count == 0)
            {
                sbs = "[]";
                return sbs.ToString();
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (parentmodelList.Count > 0)
            {
                for (int i = 0; i < parentmodelList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + parentmodelList[i].GroupId);
                    builder.Append(",");
                    builder.Append("\"name\":" + "\"" + parentmodelList[i].GroupName.ToString() + "\"");
                    builder.Append(",");
                    //
                    builder.Append("\"open\":true");
                    builder.Append(",");
                    //
                    //调用递归方法
                    builder = InitDataSelectChild(builder, parentmodelList[i].GroupId, listGroups);
                    builder.Append("}");

                    if (i < (parentmodelList.Count - 1))
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("]");
            }
            return builder.ToString();
        }

        /// <summary>
        /// 递归子类
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="groupList"></param>
        /// <returns></returns>
        public StringBuilder InitDataSelectChild(StringBuilder builder, int parentGroupId, List<GroupsViewModel> groupList)
        {
            List<GroupsViewModel> childmodelList = new List<GroupsViewModel>();
            childmodelList = groupList.Where(x => x.ParentGroupId == parentGroupId).ToList();
            if (childmodelList.Count == 0)
            {
                builder = builder.Remove(builder.Length - 1, 1);
            }
            if (childmodelList.Count > 0)
            {
                builder.Append("\"children\":[");
                for (int i = 0; i < childmodelList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + childmodelList[i].GroupId);
                    builder.Append(",");
                    builder.Append("\"name\":" + "\"" + childmodelList[i].GroupName.ToString() + "\"");
                    builder.Append(",");
                    //
                    builder.Append("\"open\":true");
                    //
                    builder.Append(",");
                    builder = InitDataSelectChild(builder, childmodelList[i].GroupId, groupList);
                    builder.Append("}");
                    if (i < (childmodelList.Count - 1))
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("]");
            }
            return builder;
        }

        /// <summary>
        /// 绑定组别下拉框
        /// </summary>
        /// <returns></returns>
        public string GetDatatreeData()
        {
            List<GroupsViewModel> parentmodelList = new List<GroupsViewModel>();
            parentmodelList = listGroups.Where(x => x.ParentGroupId == 0).ToList();  //初始化先获取父类id=0的数据集
            string sbs = string.Empty;
            if (parentmodelList.Count == 0)
            {
                sbs = "[]";
                return sbs.ToString();
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (parentmodelList.Count > 0)
            {
                for (int i = 0; i < parentmodelList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + parentmodelList[i].GroupId);
                    builder.Append(",");
                    builder.Append("\"title\":" + "\"" + parentmodelList[i].GroupName.ToString() + "\"");
                    builder.Append(",");
                    //调用递归方法
                    builder = InitDataChild(builder, parentmodelList[i].GroupId, listGroups);
                    builder.Append("}");
                    if (i < (parentmodelList.Count - 1))
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("]");
            }
            return builder.ToString();
        }
        /// <summary>
        /// 递归子类
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="groupList"></param>
        /// <returns></returns>
        public StringBuilder InitDataChild(StringBuilder builder, int parentGroupId, List<GroupsViewModel> groupList)
        {
            List<GroupsViewModel> childmodelList = new List<GroupsViewModel>();
            childmodelList = groupList.Where(x => x.ParentGroupId == parentGroupId).ToList();
            if (childmodelList.Count == 0)
            {
                builder = builder.Remove(builder.Length - 1, 1);
            }
            if (childmodelList.Count > 0)
            {
                builder.Append("\"children\":[");
                for (int i = 0; i < childmodelList.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + childmodelList[i].GroupId);
                    builder.Append(",");
                    builder.Append("\"title\":" + "\"" + childmodelList[i].GroupName.ToString() + "\"");
                    builder.Append(",");
                    builder = InitDataChild(builder, childmodelList[i].GroupId, groupList);
                    builder.Append("}");
                    if (i < (childmodelList.Count - 1))
                    {
                        builder.Append(",");
                    }
                }
                builder.Append("]");
            }
            return builder;
        }

    }
}