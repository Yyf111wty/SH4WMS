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
    public class RoleController
    {
        /// <summary>
        /// 角色列表查询
        /// </summary>
        /// <returns></returns>
        public static List<Roles> TRWCS_GetRolesDataApi()
        {
            Return r = new Return();
            try
            {
                List<Roles> rolesList = new List<Roles>();
                rolesList = Da_roles.GetRolesList();
                //#region  日志写入               
                //Log log = new Log();
                //log.Type = "角色管理";
                //log.Result = "成功";
                //log.Message = "获取角色列表数据";
                //log.CreateTime = DateTime.Now.ToString();
                //int logCount = Da_log.insertLog(log);
                //#endregion
                return rolesList;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("数据库操作失败！角色列表获取失败，" + e.Message)),
                    ReasonPhrase = "角色列表获取失败"
                };
                //#region  异常写入
                //ExceptionDatabase exception = new ExceptionDatabase();
                //exception.Source = e.Source;
                //exception.Message = e.Message;
                //exception.StackTrace = e.StackTrace;
                //exception.TargetSite = e.TargetSite.ToString();
                //exception.Data = e.Data.ToString();
                //exception.CreateTime = DateTime.Now.ToString();
                //int ExceptionCount = Da_log.insertException(exception);
                //#endregion
                throw new HttpResponseException(resp);
            }
        }
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddRolesApi(JObject json)
        {
            Return r = new Return();
            Roles value = JsonConvert.DeserializeObject<Roles>(JsonConvert.SerializeObject(json));
            try
            {
                if (value.RoleName != "")
                {
                    if (RegularFil.RegularLorC(value.RoleName) == 0)
                    {
                        r.result = false;
                        r.message = "角色名称只能由汉字或字母组成！";
                        #region  日志写入               
                        Log log1 = new Log();
                        log1.Type = "角色管理";
                        log1.Result = "失败";
                        log1.Message ="角色"+value.RoleName+"添加失败，"+r.message;
                        log1.Creator =value.CreateOperator;
                        log1.CreateTime = DateTime.Now.ToString();
                        int logCount1 = Da_log.insertLog(log1);
                        #endregion
                        return r;
                    }//角色名称过滤
                    if (RegularFil.RegularInt(value.RoleLevel.ToString()) == 0)
                    {
                        r.result = false;
                        r.message = "角色等级必须为正整数！";
                        #region  日志写入               
                        Log log2 = new Log();
                        log2.Type = "角色管理";
                        log2.Result = "失败";
                        log2.Message = "角色" + value.RoleName + "添加失败，" + r.message;
                        log2.Creator = value.CreateOperator;
                        log2.CreateTime = DateTime.Now.ToString();
                        int logCount2 = Da_log.insertLog(log2);
                        #endregion
                        return r;
                    }//角色等级过滤
                    List<Roles> repetRoleName = new List<Roles>();
                    try
                    {
                        repetRoleName = Da_roles.GetRoleByRoleName(value.RoleName);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据角色名获取角色列表失败，" + e.Message;
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
                    }//根据角色名称获取角色列表                   
                    if (repetRoleName.Count==0)
                    {
                        List<Roles> repetRoleLevel = new List<Roles>();
                        try
                        {
                            repetRoleLevel = Da_roles.GetRoleByRoleLevel(value.RoleLevel.ToString());
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!根据角色等级获取角色列表失败，" + e.Message;
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
                        }//根据角色等级获取角色列表
                        if (repetRoleLevel.Count==0)
                        {
                            int insertCount = 0;
                            try
                            {
                                insertCount = Da_roles.InsertRole(value);
                            }
                            catch (Exception e)
                            {
                                r.result = false;
                                r.message = "数据库操作失败!新增角色失败，" + e.Message;
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
                            }//新增角色
                            if (insertCount == 1)
                            {
                                r.result = true;
                                r.message = "角色添加成功！";
                            }
                            else
                            {
                                r.result = false;
                                r.message = "角色添加失败!失败原因：数据库操作失败！";
                            }
                        }
                        else
                        {
                            r.result = false;
                            r.message = "角色添加失败!失败原因：角色等级已存在！";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "角色添加失败!失败原因：角色名重复！";
                    }
                    #region  日志写入               
                    Log log = new Log();
                    log.Type = "角色管理";
                    if (r.result==true)
                    { log.Result = "成功"; }
                    else
                    { log.Result = "失败"; }
                    log.Message =  value.RoleName + r.message;
                    log.Creator = value.CreateOperator;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
                else
                {
                    r.result = false;
                    r.message = "角色添加失败!失败原因：角色名不能为空！";
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "角色管理";
                    log.Result = "失败"; 
                    log.Message = value.RoleName + r.message;
                    log.Creator = value.CreateOperator;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "角色添加失败!请检查输入参数！";
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
        /// 修改角色
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditRolesApi(JObject json)
        {
            Return r = new Return();
            Roles value = JsonConvert.DeserializeObject<Roles>(JsonConvert.SerializeObject(json));
            try
            {
                List<Roles> roleNow = new List<Roles>();//获取角色名称
                roleNow = Da_roles.GetRolesById(value.Id);
                string roleNowName = roleNow[0].RoleName;
                if (value.RoleName != "")
                {
                    if (RegularFil.RegularLorC(value.RoleName) == 0)
                    {
                        r.result = false;
                        r.message = "角色名称只能由汉字或字母组成！";
                        #region  日志写入 
                        Log log1 = new Log();
                        log1.Type = "角色管理";
                        log1.Result = "失败";
                        log1.Message = "角色" + value.RoleName + "修改失败，" + r.message;
                        log1.Creator = value.UpdateOperator;
                        log1.CreateTime = DateTime.Now.ToString();
                        int logCount1 = Da_log.insertLog(log1);
                        #endregion
                        return r;
                    }//角色名称过滤
                    if (RegularFil.RegularInt(value.RoleLevel.ToString()) == 0)
                    {
                        r.result = false;
                        r.message = "角色等级必须为正整数！";
                        #region  日志写入 
                        Log log2 = new Log();
                        log2.Type = "角色管理";
                        log2.Result = "失败";
                        log2.Message = "角色" + value.RoleName + "修改失败，" + r.message;
                        log2.Creator = value.UpdateOperator;
                        log2.CreateTime = DateTime.Now.ToString();
                        int logCount2 = Da_log.insertLog(log2);
                        #endregion
                        return r;
                    }//角色等级过滤
                    List<Roles> repetRoleList = new List<Roles>();
                    try
                    {
                        repetRoleList = Da_roles.GetRoleByRoleName(value.RoleName);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据角色名获取角色列表失败，" + e.Message;
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
                    }//根据角色名称获取角色列表
                    int repetId = value.Id;
                    if (repetRoleList.Count != 0)
                    {
                        repetId = repetRoleList[0].Id;
                    }
                    if (value.Id == repetId)
                    {
                        List<Roles> repetRoleLevel = new List<Roles>();
                        try
                        {
                            repetRoleLevel = Da_roles.GetRoleByRoleLevel(value.RoleLevel.ToString());
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!根据角色等级获取角色列表失败，" + e.Message;
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
                        }//根据角色等级获取角色列表
                        if (repetRoleLevel.Count == 0 || repetRoleLevel[0].Id==value.Id)
                        {
                            int editRoleCount = 0;
                            try
                            {
                                editRoleCount = Da_roles.EditRole(value);
                            }
                            catch (Exception e)
                            {
                                r.result = false;
                                r.message = "数据库操作失败!角色编辑失败，" + e.Message;
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
                            }//编辑角色
                            if (editRoleCount == 1)
                            {
                                r.result = true;
                                r.message = "角色修改成功";
                            }
                            else
                            {
                                r.result = false;
                                r.message = "角色修改失败，失败原因：数据库操作失败！";
                            }
                        }
                        else
                        {
                            r.result = false;
                            r.message = "角色添加失败!失败原因：角色等级已存在！";
                        }
                    }
                    else
                    {
                        r.result = false;
                        r.message = "角色修改失败，失败原因：角色名重复！";
                    }
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "角色管理";
                    if (r.result == true)
                    {
                        log.Result = "成功";
                        log.Message = value.RoleName + r.message + ",角色Id：" + value.Id;
                    }
                    else
                    {
                        log.Result = "失败";
                        log.Message = roleNowName + r.message + ",角色Id：" + value.Id;
                    }
                    log.Creator = value.UpdateOperator;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
                else
                {
                    r.result = false;
                    r.message = "角色编辑失败!失败原因：角色名不能为空！";
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "角色管理";
                    log.Result = "失败";
                    log.Message = value.RoleName + r.message;
                    log.Creator = value.UpdateOperator;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "角色编辑失败!请检查输入参数！";
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
        /// 角色删除
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_DeleteRolesApi(JObject json)
        {
            Return r = new Return();
            Roles value = JsonConvert.DeserializeObject<Roles>(JsonConvert.SerializeObject(json));
            try
            {
                List<Roles> roleNow = new List<Roles>();//获取角色名称
                roleNow = Da_roles.GetRolesById(value.Id);
                string roleNowName = roleNow[0].RoleName;
                List<UserRole> userRoleExist = new List<UserRole>();
                try
                {
                    userRoleExist = Da_roles.GetUserByRoleId(roleNow[0].RoleId);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据角色Id获取用户列表失败，" + e.Message;
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
                }//根据角色Id获取用户列表
                if (userRoleExist.Count==0)
                {
                    int deleteCount = 0;
                    try
                    {
                        deleteCount = Da_roles.DeleteRole(value.Id, roleNow[0].RoleId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!角色删除失败，" + e.Message;
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
                    }//角色删除
                    if (deleteCount == 1)
                    {
                        r.result = true;
                        r.message = "角色删除成功!";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "角色删除失败!失败原因：数据库操作失败！可能原因：数据库中不存在改角色。";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "角色删除失败!该角色下有关联用户，请先修改用户角色！";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "角色管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = roleNowName + r.message+"角色Id："+value.Id;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "角色删除失败!失败原因："+e.Message;
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
