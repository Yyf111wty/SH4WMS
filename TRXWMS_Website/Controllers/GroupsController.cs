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
    public class GroupsController : Controller
    {
        public static List<GroupsViewModel> listGroups;
        PagingHelper paginghelper = new PagingHelper();
        //[CheckLoginFilter]
        public ActionResult Index()
        {

            using (var client = new HttpClient())
            {
                var result = TRWMS_PrApi.Controllers.GroupsController.TRWCS_GetGroupsDataApi();
                listGroups = JsonConvert.DeserializeObject<List<GroupsViewModel>>(JsonConvert.SerializeObject(result));
            }
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
            return View();
        }

        //[CheckLoginFilter]
        public ActionResult AddOrEdit()
        {     
            return View();
        }
       
        /// <summary>
        /// 根据选中节点获取所属组别信息
        /// </summary>
        /// <param name="selectid"></param>
        /// <returns></returns>
        public JsonResult GetGroupsList(int selectid)
        {
            int pageSize = Request["limit"] == "NaN" ? 20 : int.Parse(Request["limit"]);//每一页显示的记录数
            int pageId = Request["page"] == "NaN" ? 1 : int.Parse(Request["page"]);//页数        
            List<GroupsViewModel> gridList = new List<GroupsViewModel>();
            try
            {
                if (listGroups.Count > 0)
                {
                    if (selectid == 0)
                    {
                        return Json(PagingHelper.GetPagingData(listGroups, pageSize, pageId), JsonRequestBehavior.AllowGet);
                    }
                    var model = listGroups.Find(t => t.GroupId == selectid);
                    var newmodel = listGroups.Find(t => t.ParentGroupId == selectid);
                    if (model.ParentGroupId == 0)
                    {
                        return Json(PagingHelper.GetPagingData(listGroups, pageSize, pageId), JsonRequestBehavior.AllowGet);
                    }
                    else if (newmodel != null)
                    {
                        foreach (var item in listGroups)
                        {
                            if (item.ParentGroupId == selectid)
                            {
                                gridList.Add(item);
                            }
                        }
                        return Json(PagingHelper.GetPagingData(gridList, pageSize, pageId), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        gridList.Add(listGroups.Find(t => t.GroupId == selectid));
                        return Json(PagingHelper.GetPagingData(gridList, pageSize, pageId), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 获取树型组织架构数据源
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public JsonResult GetTreeData(string parentId)
        {
            List<GroupsTreeViewModel> treeList = new List<GroupsTreeViewModel>();
            GroupsTreeViewModel tree = new GroupsTreeViewModel();
            if (listGroups.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(parentId))
                {
                    var ss = listGroups.Find(t => t.ParentGroupId == 0);
                    tree = new GroupsTreeViewModel { id=ss.GroupId.ToString(), title = ss.GroupName,  level = ss.GroupLevlel.ToString(), field = ss.GroupId.ToString(), lazy=true  };
                    treeList.Add(tree);
                    return Json(treeList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    foreach (var item in listGroups)
                    {
                        if (item.ParentGroupId == int.Parse(parentId))
                        {
                            tree = new GroupsTreeViewModel { id = item.GroupId.ToString(), title = item.GroupName, level = item.GroupLevlel.ToString() , field =item.GroupId.ToString() };
                            treeList.Add(tree);
                        }
                    }
                    return Json(treeList, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
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
        /// <summary>
        /// 新增组别
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="parentGroupName"></param>
        /// <param name="describe"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddGroups(string groupName, int parentGroupId, string describe, bool isEnabled)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    groupName = groupName,
                    parentGroupId = parentGroupId,
                    describes = describe,
                    isEnabled = (isEnabled==true)?1:0,
                    createOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.GroupsController.TRWCS_AddGroupsApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 编辑组别
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupName"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="parentGroupName"></param>
        /// <param name="describe"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditGroups(int id, string groupName, int parentGroupId, string describe, bool isEnabled)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    Id = id,
                    groupName = groupName,
                    parentGroupId = parentGroupId,
                    describes = describe,
                    isEnabled = (isEnabled == true) ? 1 : 0,
                    UpdateOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.GroupsController.TRWCS_EditGroupsApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除组别
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteGroups(int id)
        {
            HttpResponseModel hm = new HttpResponseModel();
            try
            {
                var content = new
                {
                    id = id,
                    createOperator =HttpContextHelper.GetCurrentUserName()
                };
                JObject contents = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content));
                hm = JsonConvert.DeserializeObject<HttpResponseModel>(JsonConvert.SerializeObject(TRWMS_PrApi.Controllers.GroupsController.TRWCS_DeleteGroupsApi(contents)));
                return Json(hm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}