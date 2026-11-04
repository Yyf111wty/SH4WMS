using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRWCS_WebAPI.Common;
using TRWMS_PrApi.DAL;
using TRWMS_PrApi.Models;

namespace TRWMS_PrApi.Controllers
{
    public class PermissionController
    {
        ///// <summary>
        ///// 权限列表获取
        ///// </summary>
        ///// <returns></returns>
        public static List<Permission> TRWCS_GetPermissionsDataApi(string NodeType)
        {
            Return r = new Return();
            try
            {
                List<Permission> perList = new List<Permission>();
                perList = Da_permission.GetPermissionList(NodeType);
                return perList;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("数据库操作失败！权限列表获取失败，" + e.Message)),
                    ReasonPhrase = "权限列表获取失败"
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
        }      
        /// <summary>
        /// 新增模块
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddModularApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                if (value.NodeName != "" && value.ImageUrl != "" && value.Icon != "")
                {
                    List<Permission> perList = new List<Permission>();
                    try
                    {
                        perList = Da_permission.GetPermissionListByName(value.NodeName,value.NodeType);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据模块名称获取模块列表失败，" + e.Message;
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
                    }//根据权限名称获取权限列表
                    if (perList.Count == 0)
                    {
                        Permission permission = new Permission();
                        permission.NodeName = value.NodeName;
                        permission.IsEnabled = value.IsEnabled;
                        permission.Type = "模块";
                        permission.Icon = value.Icon;
                        permission.ImageUrl = value.ImageUrl;
                        permission.LinkUrl = "#";
                        permission.ControllerName = "";
                        permission.ActionName = "";
                        permission.NodeType = value.NodeType;
                        int maxsortno = 0;
                        try
                        {
                            maxsortno = Da_permission.GetPermissionModularList();
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!获取模块列表失败，" + e.Message;
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
                        }//获取模块列表
                        if (maxsortno == 0)
                        {
                            permission.SortNo = 100;
                        }
                        else
                        {
                            permission.SortNo = maxsortno + 100;
                        }
                        permission.CreateTime = DateTime.Now;
                        permission.CreateOperator = value.CreateOperator;
                        int insertModularCount = 0;
                        try
                        {
                            insertModularCount = Da_permission.InsertPermission(permission);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!模块新增出错，" + e.Message;
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
                        }//模块新增
                        if (insertModularCount == 1)
                        {
                            r.result = true;
                            r.message = "模块添加成功!";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "模块添加失败!数据库操作失败！";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "模块添加失败!同平台下该模块名称已存在！";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "模块添加失败!模块名称不能为空！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = value.NodeName+","+r.message;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "模块添加失败!请检查输入参数！";
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
        /// 新增页面
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddPageApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                if (RegularFil.RegularL(value.ControllerName) == 0)//控制器过滤
                {
                    r.result = false;
                    r.message = "控制器名必须由字母组成！";
                    #region  日志写入 
                    Log log1 = new Log();
                    log1.Type = "权限管理";
                    log1.Result = "失败";
                    log1.Message = "页面" + value.NodeName + "添加失败，" + r.message;
                    log1.Creator = value.CreateOperator;
                    log1.CreateTime = DateTime.Now.ToString();
                    int logCount1 = Da_log.insertLog(log1);
                    #endregion
                    return r;
                }
                if (RegularFil.RegularL(value.ActionName) == 0)//动作过滤
                {
                    r.result = false;
                    r.message = "动作名必须由字母组成！";
                    #region  日志写入 
                    Log log1 = new Log();
                    log1.Type = "权限管理";
                    log1.Result = "失败";
                    log1.Message = "页面" + value.NodeName + "添加失败，" + r.message;
                    log1.Creator = value.CreateOperator;
                    log1.CreateTime = DateTime.Now.ToString();
                    int logCount1 = Da_log.insertLog(log1);
                    #endregion
                    return r;
                }
                List<Permission> pageList = new List<Permission>();
                try
                {
                    pageList = Da_permission.GetPermissionListByName(value.NodeName,value.NodeType);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据页面名称获取页面列表失败，" + e.Message;
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
                }//根据页面名称获取页面列表
                if (pageList.Count == 0)
                {
                    Permission permission = new Permission();
                    permission.NodeName = value.NodeName;
                    permission.IsEnabled = value.IsEnabled;
                    permission.Type = "页面";
                    permission.Icon = value.Icon;
                    permission.ImageUrl = value.ImageUrl;
                    permission.LinkUrl = value.LinkUrl;
                    permission.ControllerName = value.ControllerName;
                    permission.ActionName = value.ActionName;
                    permission._parentId = value._parentId;
                    permission.NodeType = value.NodeType;
                    int maxsortno = 0;
                    try
                    {
                        maxsortno = Da_permission.GetPermissionPageListByS(value._parentId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据父权限Id获取页面列表失败，" + e.Message;
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
                    }//根据父权限Id获取页面列表
                    if (maxsortno == 0)
                    {
                        List<Permission> modularSortNoList = new List<Permission>();
                        try
                        {
                           modularSortNoList = Da_permission.GetPermissionByParentId(value._parentId);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!根据权限名称获取页面父模块数据失败，" + e.Message;
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
                        }//根据权限名称获取页面父模块数据
                        int modularSortNo = modularSortNoList[0].SortNo;//父权限sortNo获取
                        permission.SortNo = modularSortNo + 1;
                    }
                    else
                    {
                        permission.SortNo = maxsortno + 1;
                    }
                    permission.CreateTime = DateTime.Now;
                    permission.CreateOperator = value.CreateOperator;
                    int insertModularCount =0;
                    try
                    {
                        insertModularCount = Da_permission.InsertPermission(permission);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!页面新增失败，" + e.Message;
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
                    }//页面新增
                    if (insertModularCount == 1)
                    {
                        r.result = true;
                        r.message = "页面添加成功!";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "页面添加失败!数据库操作失败！";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "页面添加失败!该页面名称已存在！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = value.NodeName + r.message;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "页面添加失败!请检查输入参数！";
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
        /// 新增按钮
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddBtnApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Permission> btnList = new List<Permission>();
                try
                {
                    btnList = Da_permission.GetPermissionListByNandPI(value.NodeName,value.NodeType, value._parentId);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据按钮名称和所属页面获取按钮列表失败，" + e.Message;
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
                }//根据按钮名称和所属页面获取按钮列表
                if (btnList.Count == 0)
                {
                    Permission permission = new Permission();
                    permission.NodeName = value.NodeName;
                    permission.IsEnabled = value.IsEnabled;
                    permission.Type = "按钮";
                    permission.Icon = value.Icon;
                    permission.ImageUrl = value.ImageUrl;
                    permission._parentId = value._parentId;
                    permission.KeyName = value.KeyName;
                    permission.NodeType = value.NodeType;
                    int maxsortno = 0;
                    try
                    {
                        maxsortno = Da_permission.GetPermissionBtnListByS(value._parentId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据页面Id获取按钮列表失败，" + e.Message;
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
                    }//根据页面Id获取按钮列表
                    if (maxsortno == 0)
                    {
                        permission.SortNo = 1;
                    }
                    else
                    {
                        permission.SortNo = maxsortno + 1;
                    }
                    permission.CreateTime = DateTime.Now;
                    permission.CreateOperator = value.CreateOperator;
                    int insertModularCount = 0;
                    try
                    {
                        insertModularCount = Da_permission.InsertPermission(permission);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!按钮新增失败，" + e.Message;
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
                    }//按钮新增
                    if (insertModularCount == 1)
                    {
                        r.result = true;
                        r.message = "按钮添加成功!";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "按钮添加失败!数据库操作失败！";
                    }

                }
                else
                {
                    r.result = false;
                    r.message = "按钮添加失败!该页面下已存在该按钮！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = value.NodeName + r.message;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "按钮添加失败!请检查输入参数！";
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
        /// 编辑模块
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditModularApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Permission> perNow = new List<Permission>();//获取组别名称
                perNow = Da_permission.GetPermissionById(value.Id);
                string perNowName = perNow[0].NodeName;
                if (value.NodeName != "" && value.ImageUrl != "" && value.Icon != "")
                {
                    List<Permission> repetModularList = new List<Permission>();
                    try
                    {
                        repetModularList = Da_permission.GetPermissionByNodeName(value.NodeName,value.Id);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据模块名称获取模块列表失败，" + e.Message;
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
                    }//根据模块名称获取模块列表
                    int repetId = value.Id;
                    if (repetModularList.Count != 0)
                    {
                        repetId = repetModularList[0].Id;
                    }
                    if (value.Id == repetId)
                    {
                        int editPermissionCount = 0;
                        try
                        {
                            editPermissionCount = Da_permission.EditPermissionModular(value);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!模块编辑失败，" + e.Message;
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
                        }//模块编辑
                        if (editPermissionCount == 1)
                        {
                            r.result = true;
                            r.message = "模块修改成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "模块修改失败，失败原因：数据库操作失败！";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "模块编辑失败，失败原因：模块名重复！";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "模块编辑失败!模块名称和图标不能为空！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                {
                    log.Result = "成功";
                    log.Message = value.NodeName + "," + r.message + "模块Id:" + value.Id;
                }
                else
                {
                    log.Result = "失败";
                    log.Message = perNowName + "," + r.message + "模块Id:" + value.Id;
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
                r.message = "模块编辑失败!请检查输入参数！";
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
        /// 页面编辑
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditPageApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Permission> perNow = new List<Permission>();//获取组别名称
                perNow = Da_permission.GetPermissionById(value.Id);
                string perNowName = perNow[0].NodeName;
                List<Permission> repetPageList = new List<Permission>();
                try
                {
                    repetPageList = Da_permission.GetPermissionByNodeName(value.NodeName,value.Id);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据页面名称获取页面列表失败，" + e.Message;
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
                }//根据页面名称获取页面列表
                int repetId = value.Id;
                if (repetPageList.Count != 0)
                {
                    repetId = repetPageList[0].Id;
                }
                if (value.Id == repetId)
                {
                    int editPermissionCount = 0;
                    try
                    {
                        editPermissionCount = Da_permission.EditPermissionPage(value);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!页面编辑失败，" + e.Message;
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
                    }//页面编辑
                    if (editPermissionCount == 1)
                    {
                        r.result = true;
                        r.message = "页面修改成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "页面修改失败，失败原因：数据库操作失败！";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "页面编辑失败，失败原因：页面名重复！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                {
                    log.Result = "成功";
                    log.Message = value.NodeName + r.message + "页面Id:" + value.Id;
                }
                else
                {
                    log.Result = "失败";
                    log.Message = perNowName + r.message + "页面Id:" + value.Id;
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
                r.message = "页面编辑失败!请检查输入参数！";
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
        /// 编辑按钮
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditBtnApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Permission> perNow = new List<Permission>();//获取组别名称
                perNow = Da_permission.GetPermissionById(value.Id);
                string perNowName = perNow[0].NodeName;
                List<Permission> repetBtnList = new List<Permission>();
                try
                {
                    repetBtnList = Da_permission.GetPermissionListByNandPI(value.NodeName,value.NodeType, value._parentId);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!查询页面下的同名按钮失败，" + e.Message;
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
                }//查询页面下的同名按钮
                int repetId = value.Id;
                if (repetBtnList.Count != 0)
                {
                    repetId = repetBtnList[0].Id;
                }
                if (value.Id == repetId)
                {
                    int editPermissionCount = 0;
                    try
                    {
                        editPermissionCount = Da_permission.EditPermissionBtn(value);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!按钮编辑失败，" + e.Message;
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
                    }//按钮编辑
                    if (editPermissionCount == 1)
                    {
                        r.result = true;
                        r.message = "按钮修改成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "按钮修改失败，失败原因：数据库操作失败！";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "按钮编辑失败，失败原因：该页面下按钮名重复！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                {
                    log.Result = "成功";
                    log.Message = value.NodeName + r.message + "按钮Id:" + value.Id;
                }
                else
                {
                    log.Result = "失败";
                    log.Message = perNowName + r.message + "按钮Id:" + value.Id;
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
                r.message = "按钮编辑失败!请检查输入参数！";
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
        /// 删除模块、页面和按钮
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_DeleteByIdApi(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Permission> perNow = new List<Permission>();//获取组别名称
                perNow = Da_permission.GetDelPermissionById(value.Id);
                string perNowName = perNow[0].NodeName;
                int id = value.Id;
                string type = "";
                int nodeId;
                try
                {
                    Permission deletePer=Da_permission.GetDelPermissionById(value.Id)[0];
                    type = deletePer.Type;
                    nodeId = deletePer.NodeId;
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据ID获取权限数据失败，" + e.Message;
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
                }//根据ID获取权限数据
                if (type=="模块")
                {
                    int deletePermissionCount = 0;
                    try
                    {
                        deletePermissionCount = Da_permission.DeleteModularPermission(id,nodeId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!模块删除失败，" + e.Message;
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
                    }//模块删除
                    if (deletePermissionCount != 0)
                    {
                        r.result = true;
                        r.message = "模块删除成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "模块删除失败，失败原因：未找到模块!";
                    }
                }
                else
                {
                    if (type == "页面")
                    {
                        int deletePermissionCount = 0;
                        try
                        {
                            deletePermissionCount = Da_permission.DeletePagePermission(id,nodeId);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!页面删除失败，" + e.Message;
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
                        }//页面删除
                        if (deletePermissionCount != 0)
                        {
                            r.result = true;
                            r.message = "页面删除成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "页面删除失败，失败原因：未找到页面!";
                        }
                    }
                    else
                    {
                        int deletePermissionCount = 0;
                        try
                        {
                            deletePermissionCount = Da_permission.DeletePermission(id, nodeId);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!按钮删除失败，" + e.Message;
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
                        }//按钮删除
                        if (deletePermissionCount == 1)
                        {
                            r.result = true;
                            r.message = "按钮删除成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "按钮删除失败，失败原因：未找到按钮!";
                        }
                    }
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = perNowName + r.message + "Id:" + value.Id;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "删除失败!请检查输入参数！失败原因：" + e.Message;
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
        /// 模块或页面上移
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_PositionUp(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                if (value.Type=="模块")//模块的上移
                {
                    int minModularSortNo = 0;
                    try
                    {
                        minModularSortNo = Da_permission.GetMinModularSortNo();//判断模块的最小sortNo
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!模块的最小SortNo获取失败，" + e.Message;
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
                    }//模块的最小SortNo获取
                    if (minModularSortNo!=value.SortNo)
                    {
                        Permission lastper = new Permission();
                        try
                        {
                            lastper = Da_permission.GetLastPermission(value.SortNo);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!本权限上一个权限数据获取失败，" + e.Message;
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
                        }//本权限一个权限数据获取
                        int change = 0;
                        try
                        {
                            change = Da_permission.ChangeModularPermission(lastper, value);//模块及模块下的页面的sortNo都要交换
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败，"+value.NodeName + "上移操作失败，" + e.Message;
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
                        }//模块上移操作
                        if (change==2)//两个模块的sortNo值的修改
                        {
                            r.result = true;
                            r.message = value.NodeName + "上移成功!";
                        }
                        else
                        {
                            r.result = false;
                            r.message = value.NodeName+"上移失败!数据库操作不成功！";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "上移失败！该模块已经是最顶层。";
                    }
                }
                else//页面的上移
                {
                    int minPageSortNo = 0;
                    try
                    {
                        minPageSortNo = Da_permission.GetMinPageSortNo(value._parentId);//判断模块下页面的最小sortNo
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message =  "模块下页面最小的SortNo获取失败，" + e.Message;
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
                    }//模块下页面最小的SortNo获取
                    if (minPageSortNo != value.SortNo)
                    {
                        Permission lastper = new Permission();
                        try
                        {
                            lastper = Da_permission.GetLastPagePermission(value.SortNo, value._parentId);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "上一个页面数据获取失败，" + e.Message;
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
                        }//上一个页面数据获取
                        int gapSortNo = value.SortNo - lastper.SortNo;
                        int change = 0;
                        try
                        {
                            change = Da_permission.ChangePagePermission(value.Id, lastper.Id, gapSortNo, value.UpdateOperator);//模块及模块下的页面的sortNo都要交换
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败，" + value.NodeName + "上移操作失败，" + e.Message;
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
                        }//页面上移
                        if (change == 2)
                        {
                            r.result = true;
                            r.message = value.NodeName + "上移成功!";
                        }
                        else
                        {
                            r.result = false;
                            r.message = value.NodeName + "上移失败!数据库操作失败";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "上移失败！该模块已经是最顶层。";
                    }
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = r.message + "Id:" + value.Id;
                log.Creator = value.UpdateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = value.NodeName + "上移失败!请检查输入参数！失败原因：" + e.Message;
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
        /// 模块或页面下移
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_PositionDown(JObject json)
        {
            Return r = new Return();
            Permission value = JsonConvert.DeserializeObject<Permission>(JsonConvert.SerializeObject(json));
            try
            {
                if (value.Type == "模块")//模块的下移
                {
                    int maxModularSortNo = 0;
                    try
                    {
                        maxModularSortNo = Da_permission.GetMaxModularSortNo();//判断模块的最大sortNo
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败！获取模块最大SortNo值失败，" + e.Message;
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
                    }//获取模块最大SortNo值
                    if (maxModularSortNo != value.SortNo)
                    {
                        Permission nextPer = new Permission();
                        try
                        {
                            nextPer = Da_permission.GetNextPermission(value.SortNo);//获取下一个权限模块的列
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败！下一个模块数据获取失败，" + e.Message;
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
                        }//下一个模块数据获取
                        int change = 0;
                        try
                        {
                            change = Da_permission.ChangeModularPermission(value, nextPer);//模块及模块下的页面的sortNo都要交换
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败！模块下移操作失败，" + e.Message;
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
                        }//模块下移操作
                        if (change == 2)//四个数分别代表：两个模块下页面的sortNo操作成功，两个模块sortNo互换
                        {
                            r.result = true;
                            r.message = value.NodeName + "下移成功!";
                        }
                        else
                        {
                            r.result = false;
                            r.message = value.NodeName + "下移失败!数据库操作失败";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "下移失败！该模块已经是最底层。";
                    }
                }
                else//页面的下移
                {
                    int maxPageSortNo = 0;
                    try
                    {
                        maxPageSortNo = Da_permission.GetMaxPageSortNo(value._parentId);//判断模块下页面的最大sortNo
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败！模块下页面的最大SortNo值获取失败，" + e.Message;
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
                    }//模块下页面的最大SortNo值获取
                    if (maxPageSortNo != value.SortNo)
                    {
                        Permission nextPer = new Permission();
                        try
                        {
                            nextPer = Da_permission.GetNextPagePermission(value.SortNo, value._parentId);//获取下一个页面的列o
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败！下一个页面数据获取失败，" + e.Message;
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
                        }//下一个页面数据获取
                        int gapSortNo = nextPer.SortNo-value.SortNo;
                        int change = 0;
                        try
                        {
                            change = Da_permission.ChangePagePermission(nextPer.Id, value.Id, gapSortNo, value.UpdateOperator);//页面的sortNo交换
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败！页面下移操作失败，" + e.Message;
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
                        }//页面下移操作
                        if (change == 2)//四个数分别代表：两个模块下页面的sortNo操作成功，两个模块sortNo互换
                        {
                            r.result = true;
                            r.message = value.NodeName + "下移成功!";
                        }
                        else
                        {
                            r.result = false;
                            r.message = value.NodeName + "下移失败!数据库操作失败";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "下移失败！该模块已经是最底层。";
                    }
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "权限管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = r.message + "Id:" + value.Id;
                log.Creator = value.UpdateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = value.NodeName + "下移失败!请检查输入参数！失败原因：" + e.Message;
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
    }
}
