using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.DAL;
using TRWCS_WebAPI.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TRWMS_PrApi.Controllers
{
    public class UserController
    {
        /// <summary>
        /// 用户数据列表查询
        /// </summary>
        /// <returns></returns>     
        public static List<Users> TRWCS_GetUsersDataApi()
        {
            Return r = new Return();
            try
            {
                List<Users> usersList = new List<Users>();
                usersList = da_users.GetUsersList();
                //#region  日志写入 
                //Log log = new Log();
                //log.Type = "用户管理";
                //log.Result = "成功";
                //log.Message =  "获取所有用户数据" ;
                //log.CreateTime = DateTime.Now.ToString();
                //int logCount = Da_log.insertLog(log);
                //#endregion
                return usersList;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("数据库操作失败！用户列表获取失败，" + e.Message)),
                    ReasonPhrase = "用户列表获取失败"
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
        /// 新增用户
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_AddUsersApi(JObject json)
        {
            Return r = new Return();
            Users value = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(json));
            try
            {
                if (value.UserName != "" && value.Password != "" && value.TrueName != "" && value.UserGroupId != 0)
                {
                    int repetUserCount = 0;
                    try
                    {
                        repetUserCount = da_users.GetUsersByName(value.UserName).Count;
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据用户名查找用户失败，" + e.Message;
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
                    }//用户查找
                    if (repetUserCount != 0)
                    {
                        r.result = false;
                        r.message = "用户添加失败，失败原因：用户名重复！";
                        #region  日志写入 
                        Log log = new Log();
                        log.Type = "用户管理";
                        log.Result = "失败";
                        log.Message =value.UserName+ r.message;
                        log.Creator = value.CreateOperator;
                        log.CreateTime = DateTime.Now.ToString();
                        int logCount = Da_log.insertLog(log);
                        #endregion
                        return r;
                    }
                    else
                    {
                        Users newUser = new Users();
                        newUser.UserName = value.UserName;
                        newUser.TrueName = value.TrueName;
                        //if (RegularFil.RegularLorN(value.UserName)==1)//用户名过滤
                        //{
                        //    newUser.UserName = value.UserName;
                        //}//用户名过滤
                        //else
                        //{
                        //    r.result = false;
                        //    r.message = "用户名只能由数字或字母组成！";
                        //    #region  日志写入 
                        //    Log log1 = new Log();
                        //    log1.Type = "用户管理";
                        //    log1.Result = "失败";
                        //    log1.Message = "用户"+value.UserName+"添加失败，" + r.message;
                        //    log1.Creator = value.CreateOperator;
                        //    log1.CreateTime = DateTime.Now.ToString();
                        //    int logCount1 = Da_log.insertLog(log1);
                        //    #endregion
                        //    return r;
                        //}
                        newUser.Password = MD5Encrypt.Encrypt(value.Password);
                        //if (RegularFil.RegularLorC(value.TrueName) == 1)//用户名过滤
                        //{
                        //    newUser.TrueName = value.TrueName;
                        //}//真实姓名过滤
                        //else
                        //{
                        //    r.result = false;
                        //    r.message = "真实姓名只能由汉字或字母组成！";
                        //    #region  日志写入 
                        //    Log log2 = new Log();
                        //    log2.Type = "用户管理";
                        //    log2.Result = "失败";
                        //    log2.Message = "用户" + value.UserName + "添加失败，" + r.message;
                        //    log2.Creator = value.CreateOperator;
                        //    log2.CreateTime = DateTime.Now.ToString();
                        //    int logCount2 = Da_log.insertLog(log2);
                        //    #endregion
                        //    return r;
                        //}
                        newUser.UserGroupId = value.UserGroupId;
                        if (value.Mobile != "")
                        {
                            if (RegularFil.RegularN(value.Mobile)==1)
                            {
                                newUser.Mobile = value.Mobile;
                            }
                            else
                            {
                                r.result = false;
                                r.message = "电话只能由数字组成！";
                                #region  日志写入 
                                Log log3 = new Log();
                                log3.Type = "用户管理";
                                log3.Result = "失败";
                                log3.Message = "用户" + value.UserName + "添加失败，" + r.message;
                                log3.Creator = value.CreateOperator;
                                log3.CreateTime = DateTime.Now.ToString();
                                int logCount3 = Da_log.insertLog(log3);
                                #endregion
                                return r;
                            }
                        }//电话号码过滤
                        else
                        {
                            newUser.Mobile = value.Mobile;
                        }
                        if (value.Email != "")
                        {
                            if (RegularFil.RegularEmail(value.Email)==1)
                            {
                                newUser.Email = value.Email;
                            }
                            else
                            {
                                r.result = false;
                                r.message = "Email格式不正确！";
                                #region  日志写入 
                                Log log4 = new Log();
                                log4.Type = "用户管理";
                                log4.Result = "失败";
                                log4.Message = "用户" + value.UserName + "添加失败，" + r.message;
                                log4.Creator = value.CreateOperator;
                                log4.CreateTime = DateTime.Now.ToString();
                                int logCount4 = Da_log.insertLog(log4);
                                #endregion
                                return r;
                            }
                        }//邮箱地址过滤
                        else
                        {
                            newUser.Email = value.Email;
                        }
                        newUser.CreateTime = DateTime.Now;
                        newUser.CreateOperator = value.CreateOperator;
                        newUser.SecretKeyValue = value.SecretKeyValue;
                        int insertCount = 0;
                        try
                        {
                            insertCount = da_users.InsertNewUser(newUser);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!用户新增失败，" + e.Message;
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
                        }//用户新增
                        if (insertCount == 1)
                        {
                            r.result = true;
                            r.message = "用户添加成功";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "用户添加失败，失败原因：数据库操作失败！请检查输入参数";
                        }
                        #region  日志写入 
                        Log log = new Log();
                        log.Type = "用户管理";
                        if (r.result==true)
                        { log.Result = "成功"; }
                        else
                        { log.Result = "失败"; }
                        log.Message = value.UserName+r.message;
                        log.Creator = value.CreateOperator;
                        log.CreateTime = DateTime.Now.ToString();
                        int logCount = Da_log.insertLog(log);
                        #endregion
                        return r;
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "添加失败!失败原因：用户名、真实姓名、密码与所属组别不能为空，请检查输入参数";
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "用户管理";
                    log.Result = "失败";
                    log.Message = "用户"+value.UserName + r.message;
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
                r.message = "添加失败!失败原因：接口程序执行失败"+e.Message;
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
        /// 用户数据修改
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_EditUsersApi(JObject json)
        {
            Return r = new Return();
            Users user = new Users();
            Users value = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(json));
            try
            {
                List<Users> userNow = new List<Users>();//获取角色名称
                userNow = da_users.GetUsersListById(value.Id);
                string userNowName = userNow[0].UserName;
                if (value.UserName != "" && value.Password != "" && value.TrueName != "" && value.UserGroupId != 0)
                {
                    List<Users> repetUserList = new List<Users>();
                    try
                    {
                        repetUserList = da_users.GetUsersByName(value.UserName);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!根据用户名查找用户失败，" + e.Message;
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
                    }//用户查找
                    int repetId = value.Id;
                    if (repetUserList.Count != 0)
                    {
                        repetId = repetUserList[0].Id;
                    }
                    if (value.Id != repetId)
                    {
                        r.result = false;
                        r.message = "用户修改失败，失败原因：用户名重复！";
                    }
                    else
                    {
                        //if (RegularFil.RegularLorN(value.UserName) == 0)//用户名过滤
                        //{
                        //    r.result = false;
                        //    r.message = "用户名只能由数字或字母组成！";
                        //    #region  日志写入 
                        //    Log log1 = new Log();
                        //    log1.Type = "用户管理";
                        //    log1.Result = "失败";
                        //    log1.Message = "用户"+ userNowName + "添加失败，"+r.message;
                        //    log1.Creator = value.UpdateOperator;
                        //    log1.CreateTime = DateTime.Now.ToString();
                        //    int logCount1 = Da_log.insertLog(log1);
                        //    #endregion
                        //    return r;
                        //}
                        //if (RegularFil.RegularLorC(value.TrueName) == 0)
                        //{
                        //    r.result = false;
                        //    r.message = "真实姓名只能由汉字或字母组成！";
                        //    #region  日志写入 
                        //    Log log2 = new Log();
                        //    log2.Type = "用户管理";
                        //    log2.Result = "失败";
                        //    log2.Message = "用户" + userNowName + "添加失败，" + r.message;
                        //    log2.Creator = value.UpdateOperator;
                        //    log2.CreateTime = DateTime.Now.ToString();
                        //    int logCount2 = Da_log.insertLog(log2);
                        //    #endregion
                        //    return r;
                        //}//真实姓名过滤
                        if (value.Mobile != "")
                        {
                            if (RegularFil.RegularN(value.Mobile) == 0)
                            {
                                r.result = false;
                                r.message = "电话只能由数字组成！";
                                #region  日志写入 
                                Log log3 = new Log();
                                log3.Type = "用户管理";
                                log3.Result = "失败";
                                log3.Message = "用户" + userNowName + "添加失败，" + r.message;
                                log3.Creator = value.UpdateOperator;
                                log3.CreateTime = DateTime.Now.ToString();
                                int logCount3 = Da_log.insertLog(log3);
                                #endregion
                                return r;
                            }
                        }//电话号码过滤
                        if (value.Email != "")
                        {
                            if (RegularFil.RegularEmail(value.Email) == 0)
                            {
                                r.result = false;
                                r.message = "Email格式不正确！";
                                #region  日志写入 
                                Log log4 = new Log();
                                log4.Type = "用户管理";
                                log4.Result = "失败";
                                log4.Message = "用户" + userNowName + "添加失败，" + r.message;
                                log4.Creator = value.UpdateOperator;
                                log4.CreateTime = DateTime.Now.ToString();
                                int logCount4 = Da_log.insertLog(log4);
                                #endregion
                                return r;
                            }
                        }//邮箱地址过滤
                        int editCount = 0;
                        try
                        {
                            editCount = da_users.EditUser(value);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!用户数据编辑失败，" + e.Message;
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
                        }//用户编辑
                        if (editCount == 1)
                        {
                            r.result = true;
                            r.message = "用户数据修改成功！";
                        }
                        else
                        {
                            r.result = false;
                            r.message = "用户数据修改失败，失败原因：数据库操作失败！请检查输入参数！";
                        }
                    }
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "用户管理";
                    if (r.result == true)
                    { log.Result = "成功"; }
                    else
                    { log.Result = "失败"; }
                    log.Message = userNowName + r.message+"用户Id:"+value.Id;
                    log.Creator = value.UpdateOperator;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
                else
                {
                    r.result = false;
                    r.message = "修改失败!失败原因：用户名、真实姓名、密码与所属组别不能为空，请检查输入参数";
                    #region  日志写入 
                    Log log = new Log();
                    log.Type = "用户管理";
                    log.Result = "失败";
                    log.Message = "用户"+userNowName + r.message;
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
                r.message = "添加失败!失败原因：接口程序执行失败，"+e.Message;
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
        /// 删除用户
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_DeleteUsersApi(JObject json)
        {
            Return r = new Return();
            Users value = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(json));
            try
            {
                List<Users> userNow = new List<Users>();//获取角色名称
                userNow = da_users.GetUsersListById(value.Id);
                string userNowName = userNow[0].UserName;
                value.UserId = userNow[0].UserId;
                int deleteUserCount = 0;
                try
                {
                    deleteUserCount = da_users.DeleteUser(value.Id,value.UserId);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "数据库操作失败!根据用户Id删除用户失败，" + e.Message;
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
                }//用户删除
                if (deleteUserCount == 1)
                {
                    r.result = true;
                    r.message = "用户删除成功！";
                }
                else
                {
                    r.result = false;
                    r.message = "用户删除失败，失败原因：数据库中不存在选定用户!";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "用户管理";
                if (r.result == true)
                {
                    log.Result = "成功";
                    log.Message = userNowName+ r.message + "用户Id:" + value.Id;
                }
                else
                {
                    log.Result = "失败";
                    log.Message = userNowName+r.message + "用户Id:" + value.Id;
                }
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "用户删除失败!请检查输入参数！失败原因：" + e.Message;
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
        /// 用户角色设置
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_SetUserRoleApi(JObject json)
        {
            Return r = new Return();
            Users value = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(json));
            try
            {
                List<Users> userNow = new List<Users>();//获取角色名称
                userNow = da_users.GetUsersListByUserId(value.UserId);
                string userNowName = userNow[0].UserName;
                if (value.RoleIdList.Count > 0)
                {
                    int repetUserRoleCount = 0;
                    try
                    {
                        repetUserRoleCount = da_users.DeleteUserRole(value.UserId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!用户角色删除失败，" + e.Message;
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
                    }//用户角色删除
                    int insertCount = 0;
                    for (int i = 0; i < value.RoleIdList.Count; i++)
                    {
                        int insertUserRoleCount = 0;
                        try
                        {
                            insertUserRoleCount = da_users.InsertUserRole(value.UserId, int.Parse(value.RoleIdList[i]), value.CreateOperator);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!用户角色新增失败，" + e.Message;
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
                        }//用户角色新增                        
                        if (insertUserRoleCount == 1)
                        { insertCount = insertCount + 1; }
                    }
                    if (insertCount == value.RoleIdList.Count)
                    {
                        r.result = true;
                        r.message = "用户角色设置成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "用户部分角色设置失败，失败原因：数据库操作失败!";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "请选择角色!";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "用户管理";
                if (r.result == true)
                {log.Result = "成功";}
                else
                { log.Result = "失败"; }
                log.Message = userNowName + r.message + "用户Id:" + value.Id;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "用户角色设置失败!失败原因：接口程序执行失败，" + e.Message;
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
        /// 重置用户密码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_ResetPassword(JObject json)
        {
            Return r = new Return();
            Users value = JsonConvert.DeserializeObject<Users>(JsonConvert.SerializeObject(json));
            try
            {           
                if (value.Password != "")
                {
                    int editPasswordCount = 0;
                    try
                    {
                        editPasswordCount = da_users.EditPassword(value);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!用户密码修改失败，" + e.Message;
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
                    }//密码重置
                    if (editPasswordCount == 1)
                    {
                        r.result = true;
                        r.message = "密码重置成功!";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "密码重置失败!";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "密码不能为空!";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "用户管理";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = "用户"+value.UserName+r.message + "用户Id:" + value.Id;
                log.Creator = value.UpdateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "密码重置失败!请检查输入参数！失败原因：接口程序执行失败!";
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
        /// 用户角色获取
        /// </summary>
        /// <returns></returns>
        public static List<Users> TRWCS_GetUserRoleApi()
        {
            try
            {
                List<Users> usersList = new List<Users>();
                usersList = da_users.GetUsersRole();
                return usersList;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("数据库操作失败！用户角色列表获取失败，" + e.Message)),
                    ReasonPhrase = "用户角色列表获取失败"
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
    }
}