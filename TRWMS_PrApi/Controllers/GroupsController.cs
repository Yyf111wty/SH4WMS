using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.DAL;
using TRWCS_WebAPI.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TRWMS_PrApi.Controllers
{
    public class GroupsController
    {
        /// <summary>
        /// 获取组别列表（ALL）
        /// </summary>
        /// <returns></returns>
        public static List<Group> TRWCS_GetGroupsDataApi()
        {
            try
            {
                List<Group> groupsList = new List<Group>();
                try
                {                   
                    groupsList = da_groups.GetGroupsList();
                }
                catch (Exception e)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！组别列表获取失败，" + e.Message)),
                        ReasonPhrase = "组别列表获取失败"
                    };
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    throw new HttpResponseException(resp);
                }
                return groupsList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// 获取所有班
        /// </summary>
        /// <returns></returns>
        public static List<Group> TRWCS_GetClasssApi()
        {
            try
            {
                List<Group> groupsList = new List<Group>();
                try
                {
                    groupsList = da_groups.GetClasssList();
                }
                catch (Exception e)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！组别列表获取失败，" + e.Message)),
                        ReasonPhrase = "组别列表获取失败"
                    };
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    throw new HttpResponseException(resp);
                }
                return groupsList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// 获取班组
        /// </summary>
        /// <returns></returns> 
        public static List<Group> TRWCS_GetGroupByPidApi(string  pid)
        {
            try
            {
                List<Group> groupsList = new List<Group>();
                try
                {
                    groupsList = da_groups.GetGroupByPidList( pid);
                }
                catch (Exception e)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！组别列表获取失败，" + e.Message)),
                        ReasonPhrase = "组别列表获取失败"
                    };
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    throw new HttpResponseException(resp);
                }
                return groupsList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// sys_Groups表的新增数据行操作方法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddGroupsApi(JObject json)
        {
            Return r = new Return();
            Group newGroup = new Group();
            Group value = JsonConvert.DeserializeObject<Group>(JsonConvert.SerializeObject(json));
            try
            {
                if (RegularFil.RegularCorLorN(value.GroupName) == 0)
                {
                    r.result = false;
                    r.message = "组别名称必须由汉字、字母或数字组成！";
                    #region  日志写入               
                    Log log1 = new Log();
                    log1.Type = "组别管理";
                    log1.Result = "失败";
                    log1.Message = r.message;
                    log1.Creator = value.CreateOperator;
                    log1.CreateTime = DateTime.Now.ToString();
                    int logCount1 = Da_log.insertLog(log1);
                    #endregion
                    return r;
                }
                if (value.ParentGroupId ==0)
                {
                    int insertCount = 0;
                    newGroup.GroupName = value.GroupName;
                    newGroup.ParentGroupId = 0;
                    newGroup.IsEnabled = value.IsEnabled;
                    newGroup.Describes = value.Describes;
                    newGroup.CreateTime = DateTime.Now;
                    newGroup.CreateOperator = value.CreateOperator;
                    newGroup.State = null;
                    try//添加总公司到数据库
                    {
                        insertCount = da_groups.InsertNewGroup(newGroup);
                    }
                    catch(Exception e)
                    {
                        r.result = false;
                        r.message = "总公司添加失败!" + e.Message;
                        #region  异常写入
                        ExceptionDatabase exception = new ExceptionDatabase();
                        exception.Source = e.Source;
                        exception.Message = e.Message;
                        exception.StackTrace = e.StackTrace;
                        exception.TargetSite = e.TargetSite.ToString();
                        exception.Data = e.Data.ToString();
                        exception.Creator = value.CreateOperator;
                        exception.CreateTime = DateTime.Now.ToString();
                        int ExceptionCount = Da_log.insertException(exception);
                        #endregion
                        return r;
                    }//添加总公司到数据库
                    if (insertCount == -1)
                    {
                        r.result = false;
                        r.message = "总公司已存在，无法重复添加！请选择父组名称！";
                    }
                    else
                    {
                        if (insertCount == 1)
                        {
                            r.result = true;
                            r.message = "总公司添加成功";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "总公司添加失败，失败原因：数据库操作失败！";
                        }
                    }
                }
                else
                {
                    int repetGroupCount = 0;
                    try
                    {
                        repetGroupCount = da_groups.GetGroupsListByNandP(value.GroupName, value.ParentGroupId).Count;
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!无法根据组别名称和父组Id获取组别列表，" + e.Message;
                        #region  异常写入
                        ExceptionDatabase exception = new ExceptionDatabase();
                        exception.Source = e.Source;
                        exception.Message = e.Message;
                        exception.StackTrace = e.StackTrace;
                        exception.TargetSite = e.TargetSite.ToString();
                        exception.Data = e.Data.ToString();
                        exception.Creator = value.CreateOperator;
                        exception.CreateTime = DateTime.Now.ToString();
                        int ExceptionCount = Da_log.insertException(exception);
                        #endregion
                        return r;
                    }//获取同一父组下与新增组别的组别名重复的组别个数
                    if (repetGroupCount != 0)
                    {
                        r.result = false;
                        r.message = "组别添加失败，失败原因：组别重复！";
                    }
                    else
                    {                        
                        newGroup.GroupName = value.GroupName;
                        newGroup.ParentGroupId = value.ParentGroupId;
                        newGroup.IsEnabled = value.IsEnabled;
                        newGroup.Describes = value.Describes;
                        newGroup.CreateTime = DateTime.Now;
                        newGroup.CreateOperator = value.CreateOperator;
                        newGroup.State = null;
                        int insertCount = 0;
                        try
                        {
                           insertCount = da_groups.InsertNewGroup(newGroup);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!无法新增组别，" + e.Message;
                            #region  异常写入
                            ExceptionDatabase exception = new ExceptionDatabase();
                            exception.Source = e.Source;
                            exception.Message = e.Message;
                            exception.StackTrace = e.StackTrace;
                            exception.TargetSite = e.TargetSite.ToString();
                            exception.Data = e.Data.ToString();
                            exception.Creator = value.CreateOperator;
                            exception.CreateTime = DateTime.Now.ToString();
                            int ExceptionCount = Da_log.insertException(exception);
                            #endregion
                            return r;
                        }//新增组别                       
                        if (insertCount == 1)
                        {
                            r.result = true;
                            r.message = "组别添加成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "组别添加失败，失败原因：数据库操作失败！";
                        }
                    }
                }
                #region  日志写入               
                Log log = new Log();
                log.Type = "组别管理";
                if (r.result==true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message =r.message+",组别名称："+ value.GroupName ;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "添加失败!失败原因：请检查输入参数！";
                #region  异常写入
                ExceptionDatabase exception = new ExceptionDatabase();
                exception.Source = e.Source;
                exception.Message = e.Message;
                exception.StackTrace = e.StackTrace;
                exception.TargetSite = e.TargetSite.ToString();
                exception.Data = e.Data.ToString();
                exception.Creator = value.CreateOperator;
                exception.CreateTime = DateTime.Now.ToString();
                int ExceptionCount = Da_log.insertException(exception);
                #endregion
                return r;
                throw e;
            }
        }
       
        /// <summary>
        /// 组别编辑接口
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditGroupsApi(JObject json)
        {

            Return r = new Return();
            Group newGroup = new Group();
            Group value = JsonConvert.DeserializeObject<Group>(JsonConvert.SerializeObject(json));
            try
            {
                if (RegularFil.RegularCorLorN(value.GroupName) == 0)
                {
                    r.result = false;
                    r.message = "组别名称必须由汉字、字母或数字组成！";
                    #region  日志写入               
                    Log log1 = new Log();
                    log1.Type = "组别管理";
                    log1.Result = "失败";
                    log1.Message = "组别修改失败，"+r.message;
                    log1.Creator = value.UpdateOperator;
                    log1.CreateTime = DateTime.Now.ToString();
                    int logCount1 = Da_log.insertLog(log1);
                    #endregion
                    return r;
                }
                List<Group> repetGroupList = new List<Group>();
                try
                {
                    repetGroupList = da_groups.GetGroupsListByNandP(value.GroupName, value.ParentGroupId);
                }
                catch(Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据组别名称与父组Id获取组别列表失败" + e.Message;
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.Creator = value.UpdateOperator;
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    return r;
                }
                int repetId= value.Id;
                if (repetGroupList.Count != 0)
                {
                    repetId = repetGroupList[0].Id;
                }
                if (value.Id != repetId)
                {                  
                    r.result = false;
                    r.message = "组别修改失败，失败原因：组别重复！";
                }
                else
                {
                    newGroup.Id = value.Id;
                    newGroup.GroupName = value.GroupName;
                    newGroup.ParentGroupId = value.ParentGroupId;
                    newGroup.IsEnabled = value.IsEnabled;
                    newGroup.Describes = value.Describes;
                    newGroup.UpdateTime = DateTime.Now;
                    newGroup.UpdateOperator = value.UpdateOperator;
                    int editGroupCount = 0;
                    try
                    {
                        editGroupCount = da_groups.EditGroup(newGroup);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!组别表数据更新失败，" + e.Message;
                        #region  异常写入
                        ExceptionDatabase exception = new ExceptionDatabase();
                        exception.Source = e.Source;
                        exception.Message = e.Message;
                        exception.StackTrace = e.StackTrace;
                        exception.TargetSite = e.TargetSite.ToString();
                        exception.Data = e.Data.ToString();
                        exception.Creator = value.UpdateOperator;
                        exception.CreateTime = DateTime.Now.ToString();
                        int ExceptionCount = Da_log.insertException(exception);
                        #endregion
                        return r;
                    }                   
                    if (editGroupCount == 1)
                    {
                        r.result = true;
                        r.message = "组别修改成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "组别修改失败，失败原因：数据库操作失败！";
                    }
                }
                List<Group> groupNow = new List<Group>();
                groupNow = da_groups.GetGroupsListById(value.Id);
                string groupNowName = groupNow[0].GroupName;
                #region  日志写入               
                Log log = new Log();
                log.Type = "组别管理";
                if (r.result == true)
                {
                    log.Result = "成功";
                    log.Message = value.GroupName + r.message + "Id：" + value.Id;
                }
                else
                {
                    log.Result = "失败";
                    log.Message = groupNowName + r.message + "Id：" + value.Id;
                }
                log.Creator = value.UpdateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "组别修改失败!请检查输入参数！";
                #region  异常写入
                ExceptionDatabase exception = new ExceptionDatabase();
                exception.Source = e.Source;
                exception.Message = e.Message;
                exception.StackTrace = e.StackTrace;
                exception.TargetSite = e.TargetSite.ToString();
                exception.Data = e.Data.ToString();
                exception.Creator = value.UpdateOperator;
                exception.CreateTime = DateTime.Now.ToString();
                int ExceptionCount = Da_log.insertException(exception);
                #endregion
                return r;
                throw e;
            }
        }
        
        /// <summary>
        /// 组别删除接口
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_DeleteGroupsApi(JObject json)
        {
            Return r = new Return();
            Group value = JsonConvert.DeserializeObject<Group>(JsonConvert.SerializeObject(json));
            try
            {
                List<Group> groupNow = new List<Group>();//获取组别名称
                groupNow = da_groups.GetGroupsListById(value.Id);
                string groupNowName = groupNow[0].GroupName;
                int userCount = 0;
                try
                {
                    userCount = da_users.GetUsersListByGI(groupNow[0].GroupId).Count;
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据组别Id获取用户列表失败，" + e.Message;
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.Creator = value.CreateOperator;
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    return r;
                }
                if (userCount!=0)
                {
                    r.result = false;
                    r.message = "组别删除失败，失败原因：该组别下有关联用户!";
                }
                else
                {
                    List<Group> childList = new List<Group>();
                    try
                    {
                        childList = da_groups.GetGroupsList();
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!无法获取组别列表，" + e.Message;
                        #region  异常写入
                        ExceptionDatabase exception = new ExceptionDatabase();
                        exception.Source = e.Source;
                        exception.Message = e.Message;
                        exception.StackTrace = e.StackTrace;
                        exception.TargetSite = e.TargetSite.ToString();
                        exception.Data = e.Data.ToString();
                        exception.Creator = value.CreateOperator; 
                        exception.CreateTime = DateTime.Now.ToString();
                        int ExceptionCount = Da_log.insertException(exception);
                        #endregion
                        return r;
                    }//获取组别列表
                    var childCount=childList.Find(t => t.ParentGroupId == value.GroupId);

                    if (childCount.GroupId>0)
                    {
                        int id = value.Id;
                        int deleteGroupCount = 0;
                        try
                        {
                            deleteGroupCount = da_groups.DeleteGroup(id);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!根据Id删除组别列表失败，" + e.Message;
                            #region  异常写入
                            ExceptionDatabase exception = new ExceptionDatabase();
                            exception.Source = e.Source;
                            exception.Message = e.Message;
                            exception.StackTrace = e.StackTrace;
                            exception.TargetSite = e.TargetSite.ToString();
                            exception.Data = e.Data.ToString();
                            exception.Creator = value.CreateOperator;
                            exception.CreateTime = DateTime.Now.ToString();
                            int ExceptionCount = Da_log.insertException(exception);
                            #endregion
                            return r;
                        }//根据Id删除组别列表
                        if (deleteGroupCount == 1)
                        {
                            r.result = true;
                            r.message = "组别删除成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "组别删除失败，失败原因：数据库中不存在选定组别!";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "组别删除失败，失败原因：该组别下有子组别!";
                    }
                }
                #region  日志写入               
                Log log = new Log();
                log.Type = "组别管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = groupNowName+r.message + "Id：" + value.Id;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "组别删除失败!失败原因："+e.Message;
                #region  异常写入
                ExceptionDatabase exception = new ExceptionDatabase();
                exception.Source = e.Source;
                exception.Message = e.Message;
                exception.StackTrace = e.StackTrace;
                exception.TargetSite = e.TargetSite.ToString();
                exception.Data = e.Data.ToString();
                exception.Creator = value.CreateOperator;
                exception.CreateTime = DateTime.Now.ToString();
                int ExceptionCount = Da_log.insertException(exception);
                #endregion
                return r;
                throw e;
            }
        }
    }
}