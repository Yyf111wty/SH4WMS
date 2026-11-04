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
    public class PermissionController : Controller
    {
        private static List<Rows> permissionList;

       // [CheckLoginFilter]
        public ActionResult Index()
        {
            using (var client = new HttpClient())
            {
                var result = TRWMS_PrApi.Controllers.PermissionController.TRWCS_GetPermissionsDataApi("");
                permissionList = JsonConvert.DeserializeObject<List<Rows>>(JsonConvert.SerializeObject(result));
                if (permissionList.Count > 0)
                {
                    foreach (var item in permissionList)
                    {                    
                        item.state = "open";
                    }
                }
            }
            return View();
        }
       // [CheckLoginFilter]
        public ActionResult ModularAddOrEdit()
        {           
            return View();
        }
        //[CheckLoginFilter]
        public ActionResult PageAddOrEdit()
        {
            return View();
        }
       // [CheckLoginFilter]
        public ActionResult ButtonAddOrEdit()
        {
            return View();
        }

        /// <summary>
        /// 获取treetable数据
        /// </summary>
        /// <returns></returns>
        public string GetRolesTreeTableData(List<Rows> prows)
        {

            var data = "";          
            string sbs = string.Empty;
            if (prows.Count == 0)
            {
                sbs = "[]";
                data = sbs.ToString();              
                         
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (prows.Count > 0)
            {
                for (int i = 0; i < prows.Count; i++)
                {
                    builder.Append("{");
                    builder.Append("\"id\":" + prows[i].NodeId + "");
                    builder.Append(",");
                    builder.Append("\"pid\":" + prows[i]._parentId + "");
                    builder.Append(",");
                    builder.Append("\"NodeName\":\"" + prows[i].NodeName + "\"");
                    builder.Append(",");
                    builder.Append("\"Type\":\"" + prows[i].Type + "\"");
                    builder.Append(",");
                    builder.Append("\"NodeType\":\"" + prows[i].NodeType + "\"");
                    builder.Append(",");
                    builder.Append("\"Linkurl\":\"" + prows[i].Linkurl + "\"");
                    builder.Append(",");
                    builder.Append("\"ControllerName\":\"" + prows[i].ControllerName + "\"");
                    builder.Append(",");
                    builder.Append("\"ActionName\":\"" + prows[i].ActionName + "\"");
                    builder.Append(",");
                    builder.Append("\"SortNo\":\"" + prows[i].SortNo + "\"");
                    builder.Append(",");
                    builder.Append("\"rowid\":\"" + prows[i].Id + "\"");
                    builder.Append(",");
                    builder.Append("\"IsEnabled\":\"" + prows[i].IsEnabled + "\"");
                    if (i == prows.Count - 1)
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
            return data;
        }
        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param name="typeFlag"></param>
        /// <param name="parentid"></param>
        /// <returns></returns>
        public JsonResult GetModularList(int typeFlag, int parentid = 0)
        {
            PermissionViewModel model = new PermissionViewModel();
            if (typeFlag == 0)
            {
                List<Rows> parentRows = new List<Rows>();
                parentRows = permissionList.Where(x => x.Type != "按钮").ToList();
                var data = GetRolesTreeTableData(parentRows);
                return Json(new { data=data}, JsonRequestBehavior.AllowGet);
            }
            else if (typeFlag == 1)
            {
                List<Rows> parentRows = new List<Rows>();
                foreach (var item in permissionList)
                {
                    if (item.Type == "模块")
                    {
                        parentRows.Add(item);
                    }
                }
                var data = GetRolesTreeTableData(parentRows);
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<Rows> childList = new List<Rows>();
                foreach (var item in permissionList)
                {
                    if (item.Type == "按钮")
                    {
                        if (item._parentId == parentid)
                        {
                            childList.Add(item);
                        }
                    }
                }
                var data = GetRolesTreeTableData(childList);
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param name="typeFlag"></param>
        /// <param name="parentid"></param>
        /// <returns></returns>
        public JsonResult GetModularButtonList(int typeFlag, int parentid = 0)
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数     
            PermissionViewModel model = new PermissionViewModel();
            if (typeFlag == 0)
            {
                List<Rows> parentRows = new List<Rows>();
                parentRows = permissionList.Where(x => x.Type != "按钮").ToList();         
                return Json(PagingHelper.GetPagingData(parentRows, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
            else if (typeFlag == 1)
            {
                List<Rows> parentRows = new List<Rows>();
                foreach (var item in permissionList)
                {
                    if (item.Type == "模块")
                    {
                        parentRows.Add(item);
                    }
                }
                return Json(PagingHelper.GetPagingData(parentRows, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<Rows> childList = new List<Rows>();
                foreach (var item in permissionList)
                {
                    if (item.Type == "按钮")
                    {
                        if (item._parentId == parentid)
                        {
                            childList.Add(item);
                        }
                    }
                }
                return Json(PagingHelper.GetPagingData(childList, pageSize, pageId), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 新增模块
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="IsEnabled"></param>
        /// <param name="ImageUrl"></param>
        /// <param name="Icon"></param>
        /// <param name="Type"></param>
        /// <param name="SortNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddModular(string NodeName, int IsEnabled, string ImageUrl, string Icon, string NodeType)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    NodeType = NodeType,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };

                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_AddModularApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 编辑模块
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="NodeName"></param>
        /// <param name="IsEnabled"></param>
        /// <param name="ImageUrl"></param>
        /// <param name="Icon"></param>
        /// <param name="Type"></param>
        /// <param name="SortNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditModular(int Id, string NodeName, int IsEnabled, string ImageUrl, string Icon, string NodeType)

        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    NodeType = NodeType,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_EditModularApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult AddPage(string NodeName, int IsEnabled, string ImageUrl, string Icon, string LinkUrl, string ControllerName, string ActionName, int ParentId, string ParentName, string NodeType)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    LinkUrl = LinkUrl,
                    ControllerName = ControllerName,
                    ActionName = ActionName,
                    _parentId = ParentId,
                    ParentName = ParentName,
                    NodeType = NodeType,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_AddPageApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 编辑页面
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="NodeName"></param>
        /// <param name="IsEnabled"></param>
        /// <param name="ImageUrl"></param>
        /// <param name="Icon"></param>
        /// <param name="LinkUrl"></param>
        /// <param name="ControllerName"></param>
        /// <param name="ActionName"></param>
        /// <param name="ParentId"></param>
        /// <param name="ParentName"></param>
        /// <param name="SortNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditPage(int Id, string NodeName, int IsEnabled, string ImageUrl, string Icon, string LinkUrl, string ControllerName, string ActionName,string NodeType)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    LinkUrl = LinkUrl,
                    ControllerName = ControllerName,
                    ActionName = ActionName,
                    NodeType = NodeType,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_EditPageApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新增按钮
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="IsEnabled"></param>
        /// <param name="KeyName"></param>
        /// <param name="ImageUrl"></param>
        /// <param name="Icon"></param>
        /// <param name="ParentId"></param>
        /// <param name="ParentName"></param>
        /// <param name="SortNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddBtn(string NodeName, int IsEnabled, string KeyName, string ImageUrl, string Icon, int ParentId, string ParentName,string NodeType)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    KeyName = KeyName,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    _parentId = ParentId,
                    ParentName = ParentName,
                    NodeType = NodeType,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_AddBtnApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 编辑按钮
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="NodeName"></param>
        /// <param name="IsEnabled"></param>
        /// <param name="KeyName"></param>
        /// <param name="ImageUrl"></param>
        /// <param name="Icon"></param>
        /// <param name="ParentId"></param>
        /// <param name="ParentName"></param>
        /// <param name="SortNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditBtn(int Id, string NodeName, int IsEnabled, string KeyName, string ImageUrl, string Icon, int ParentId, string NodeType)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    NodeName = NodeName,
                    IsEnabled = IsEnabled,
                    KeyName = KeyName,
                    ImageUrl = ImageUrl,
                    icon = Icon,
                    _parentId = ParentId,
                    NodeType = NodeType,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_EditBtnApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(int Id)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    CreateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_DeleteByIdApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取模块和页面归属的图标列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImageList()
        {
            List<BaseModel> imageList = new List<BaseModel>();
            imageList.Add(new BaseModel { Text = "/images/baseinfo.png", Value = "icon-baseinfo" });
            imageList.Add(new BaseModel { Text = "/images/log.png", Value = "icon-log" });
            imageList.Add(new BaseModel { Text = "/images/package.png", Value = "icon-package" });
            imageList.Add(new BaseModel { Text = "/images/permission.png", Value = "icon-file" });
            imageList.Add(new BaseModel { Text = "/images/role-permission.png", Value = "icon-tool" });
            imageList.Add(new BaseModel { Text = "/images/role.png", Value = "icon-role" });
            imageList.Add(new BaseModel { Text = "/images/sys-rbac.png", Value = "icon-book" });
            imageList.Add(new BaseModel { Text = "/images/user.png", Value = "icon-users" });
            imageList.Add(new BaseModel { Text = "/images/user_group.png", Value = "icon-group" });
            imageList.Add(new BaseModel { Text = "/images/email.png", Value = "icon-email" });
            imageList.Add(new BaseModel { Text = "/images/report.png", Value = "icon-report" });
            imageList.Add(new BaseModel { Text = "/images/chart.png", Value = "icon-chart" });
            return Json(imageList, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 获取按钮归属的图标列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetButtonList()
        {
            List<BaseModel> imageList = new List<BaseModel>();
            imageList.Add(new BaseModel { Text = "/Content/icons/edit_add.png", Value = "icon-add" });
            imageList.Add(new BaseModel { Text = "/Content/icons/pencil.png", Value = "icon-edit" });
            imageList.Add(new BaseModel { Text = "/Content/icons/clear.png", Value = "icon-clear" });
            imageList.Add(new BaseModel { Text = "/Content/icons/edit_remove.png", Value = "icon-remove" });
            imageList.Add(new BaseModel { Text = "/Content/icons/filesave.png", Value = "icon-save" });
            imageList.Add(new BaseModel { Text = "/Content/icons/cut.png", Value = "icon-cut" });
            imageList.Add(new BaseModel { Text = "/Content/icons/ok.png", Value = "icon-ok" });
            imageList.Add(new BaseModel { Text = "/Content/icons/no.png", Value = "icon-no" });
            imageList.Add(new BaseModel { Text = "/Content/icons/cancel.png", Value = "icon-cancel" });
            imageList.Add(new BaseModel { Text = "/Content/icons/reload.png", Value = "icon-reload" });
            imageList.Add(new BaseModel { Text = "/Content/icons/search.png", Value = "icon-search" });
            imageList.Add(new BaseModel { Text = "/Content/icons/print.png", Value = "icon-print" });
            imageList.Add(new BaseModel { Text = "/Content/icons/help.png", Value = "icon-help" });
            imageList.Add(new BaseModel { Text = "/Content/icons/undo.png", Value = "icon-undo" });
            imageList.Add(new BaseModel { Text = "/Content/icons/redo.png", Value = "icon-redo" });
            imageList.Add(new BaseModel { Text = "/Content/icons/back.png", Value = "icon-back" });
            imageList.Add(new BaseModel { Text = "/Content/icons/sum.png", Value = "icon-sum" });
            imageList.Add(new BaseModel { Text = "/Content/icons/tip.png", Value = "icon-tip" });
            imageList.Add(new BaseModel { Text = "/Content/icons/filter.png", Value = "icon-filter" });
            imageList.Add(new BaseModel { Text = "/Content/icons/man.png", Value = "icon-man" });
            imageList.Add(new BaseModel { Text = "/Content/icons/lock.png", Value = "icon-lock" });
            imageList.Add(new BaseModel { Text = "/Content/icons/more.png", Value = "icon-more" });
            imageList.Add(new BaseModel { Text = "/Content/icons/up.png", Value = "icon-up" });
            imageList.Add(new BaseModel { Text = "/Content/icons/down.png", Value = "icon-down" });
            imageList.Add(new BaseModel { Text = "/Content/icons/export.png", Value = "icon-export" });
            imageList.Add(new BaseModel { Text = "/Content/icons/tick_shield.png", Value = "icon-tick_shield" });
            imageList.Add(new BaseModel { Text = "/Content/icons/edit-clear.png", Value = "icon-edit-clear" });
            return Json(imageList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPlatformList()
        {
            var Platforms = "<option value=''></option><option value='Permission'>权限系统</option><option value='MES'>MES</option><option value='WMS'>WMS</option><option value='WCS'>WCS</option><option value='竞标系统'>竞标系统</option>";
            return Json(Platforms, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Type"></param>
        /// <param name="SortNo"></param>
        /// <param name="_parentId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PositionUp(int Id, string Type, int SortNo, int _parentId)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    Type = Type,
                    SortNo = SortNo,
                    _parentId = _parentId,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_PositionUp(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Type"></param>
        /// <param name="SortNo"></param>
        /// <param name="_parentId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PositionDown(int Id, string Type, int SortNo, int _parentId)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = Id,
                    Type = Type,
                    SortNo = SortNo,
                    _parentId = _parentId,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.PermissionController.TRWCS_PositionDown(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}