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
using TR.Service;
using TRWMS_Website.Filters;
using TRWMS_Website.Lib;
using TRWMS_Website.ViewModels;
using TRXWMS_Website.Helper;

namespace TRWMS_Website.Controllers
{
    [LogExceptionAttribute]
    //[CheckLoginFilter]
    public class HomeController : Controller
    {
        //stepService service = new stepService();
        public ActionResult Index()
        {
            //ViewData["trueName"] =HttpContextHelper.GetCurrentUserName();
            return View();
        }
       
        public ActionResult ChangPwd()
        {           
            return View();
        }

        //public JsonResult GetPermissionData()
        //{
        //    try
        //    {
        //        // 从请求头获取Token
        //        string token = Request.Headers["Authorization"]?.Replace("Bearer ", "")?.Trim();
        //        //string userName = string.Empty;
        //        string userName = string.IsNullOrEmpty(token) ? null : TokenHelper.GetUserName(token);

        //        // 解析Token获取用户名
        //        //if (!string.IsNullOrEmpty(token))
        //        //{
        //        //    userName = TokenHelper.GetUserName(token);
        //        //}

        //        // 无Token或Token无效时，返回空菜单数据，避免前端崩溃
        //        if (string.IsNullOrEmpty(userName))
        //        {
        //            return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);
        //        }

        //        // 重新加载当前用户权限
        //        //bool hasPermission = GetUserPermissions(userName);

        //        //// 权限为空时也返回空格式，不影响页面渲染
        //        //if (!hasPermission || UserPermissionConfig.listrow == null || UserPermissionConfig.listrow.Count == 0)
        //        //{
        //        //    return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);
        //        //}
        //        // 🔥 【关键】每次都重新获取当前用户权限，不使用静态变量！
        //        var userPermissions = GetUserPermissionsByUserName(userName);
        //        if (userPermissions == null || userPermissions.Count == 0)
        //            return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);

        //        // 构建前端需要的菜单数据
        //        var parentRows = userPermissions.Where(x => x._parentId == 0).ToList();
        //        //var parentRows = UserPermissionConfig.listrow.Where(x => x._parentId == 0).ToList();
        //        var moduList = new List<Modular>();

        //        foreach (var parent in parentRows)
        //        {
        //            var modu = new Modular
        //            {
        //                menuid = parent.NodeId.ToString(),
        //                menuname = parent.NodeName,
        //                icon = parent.icon,
        //                NodeType = parent.NodeType,
        //                menus = new List<Page>()
        //            };

        //            var childRows = userPermissions.Where(x => x._parentId == parent.NodeId).ToList();
        //            foreach (var child in childRows)
        //            {
        //                modu.menus.Add(new Page
        //                {
        //                    menuid = child.NodeId.ToString(),
        //                    menuname = child.NodeName,
        //                    icon = child.icon,
        //                    url = child.Linkurl,
        //                    NodeType = child.NodeType
        //                });
        //            }

        //            moduList.Add(modu);
        //        }

        //        // 强制返回 { menus: [...] } 格式，和前端代码完全匹配
        //        return Json(new { menus = moduList }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch
        //    {
        //        // 任何异常都返回空格式，绝不导致前端崩溃
        //        return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public JsonResult GetPermissionData()
        {
            try
            {
                // 取当前用户
                string token = Request.Headers["Authorization"]?.Replace("Bearer ", "")?.Trim();
                string userName = TokenHelper.GetUserName(token);
                if (string.IsNullOrEmpty(userName))
                    return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);

                // 登录后权限已写入 Session，此处直接读缓存
                var permissions = GetUserPermissionList(userName);

                // 构建菜单
                var menus = permissions
                    .Where(p => p._parentId == 0)
                    .Select(parent => new Modular
                    {
                        menuid = parent.NodeId.ToString(),
                        menuname = parent.NodeName,
                        icon = parent.icon,
                        NodeType = parent.NodeType,
                        menus = permissions
                            .Where(child => child._parentId == parent.NodeId)
                            .Select(child => new Page
                            {
                                menuid = child.NodeId.ToString(),
                                menuname = child.NodeName,
                                icon = child.icon,
                                url = child.Linkurl,
                                NodeType = child.NodeType
                            }).ToList()
                    }).ToList();

                return Json(new { menus }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { menus = new List<object>() }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 根据用户名重新获取权限（你原来的方法，我直接调用）
        /// </summary>
        private List<Rows> GetUserPermissionList(string userName)
        {
            try
            {
                return UserPermissionCache.GetPermissions(Session, userName);
            }
            catch
            {
                return new List<Rows>();
            }
        }

        /// <summary>
        ///根据用户获取权限列表
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool GetUserPermissions(string userName)
        {
            return false;
            //UserPermissionConfig.listrow = new List<Rows>();
            //using (var client = new HttpClient())
            //{
            //    var result = TRWMS_PrApi.Controllers.LoginController.TRWCS_GetUserPermissionDataApi(userName, "");
            //    UserPermissionConfig.listrow = JsonConvert.DeserializeObject<List<Rows>>(JsonConvert.SerializeObject(result));
            //    if (UserPermissionConfig.listrow.Count > 0) { return true; }
            //    else { return false; }
            //}
        }
        /// <summary>
        /// 根据页面名称获取角色权限内的按钮
        /// </summary>
        /// <param NodeName="pageName"></param>
        /// <returns></returns>
        public JsonResult GetPermissionBtn(int pageId)
        {
            try
            {

                string token = null;

                // 1. 优先从 header 取
                var auth = Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
                {
                    token = auth.Substring("Bearer ".Length);
                }

                // 2. 如果 header 没有，从 URL 参数取（兼容 iframe）
                if (string.IsNullOrEmpty(token))
                {
                    token = Request["token"];
                }
                string userName = TokenHelper.GetUserName(token);
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new List<Rows>(), JsonRequestBehavior.AllowGet);
                }
              
                // 从 Session 缓存读取，按 pageId 过滤按钮
                var permissions = GetUserPermissionList(userName);
                var buttons = permissions
                    .Where(x => x._parentId == pageId)
                    .OrderBy(x => x.SortNo)
                    .ToList();

                return Json(buttons, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<Rows>(), JsonRequestBehavior.AllowGet);
            }

            //List<Rows> detailRows = new List<Rows>();
            //foreach (var item in UserPermissionConfig.listrow)
            //{
            //    if (item._parentId == pageId)
            //    {
            //        detailRows.Add(item);
            //    }
            //}
            //detailRows = detailRows.OrderBy(x => x.SortNo).ToList();
            //return Json(detailRows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [JsonException]
        public JsonResult ResetPassword(string pass)
        {
            HttpResponseModel hm = new HttpResponseModel();
            var content = new
            {
               
                UserName =HttpContextHelper.GetCurrentUserName(),
                Password = pass
            };
            JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
            hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.UserController.TRWCS_ResetPassword(contents)));
            return Json(hm, JsonRequestBehavior.AllowGet);
        }

       
    }
}
