using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRWMS_PrApi.DAL;
using TRWMS_PrApi.Models;

namespace TRWMS_PrApi.Controllers
{
    public class RolePermissionController
    {
        /// <summary>
        /// 角色权限设置
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Return TRWCS_SetRolePermissionApi(JObject json)
        {
            Return r = new Return();
            RolePermission value = JsonConvert.DeserializeObject<RolePermission>(JsonConvert.SerializeObject(json));
            try
            {
                List<Roles> roleNow = new List<Roles>();//获取角色名称
                roleNow = Da_roles.GetRolesByRoleId(value.RoleId);//获取角色行数据
                string roleNowName = roleNow[0].RoleName;
                if (value.PermissionList.Count > 0)//角色原有权限删除
                {
                    int repetUserRoleCount = 0;
                    try
                    {
                        repetUserRoleCount = Da_rolePermission.DeleteRolePermission(value.RoleId);
                    }
                    catch (Exception e)
                    {
                        r.result = false;
                        r.message = "数据库操作失败!角色权限删除失败，" + e.Message;
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
                    }//角色权限删除   
                    int insertCount = 0;
                    for (int i = 0; i < value.PermissionList.Count; i++)
                    {
                        int insertUserRoleCount = 0;
                        try
                        {
                            insertUserRoleCount = Da_rolePermission.InsertRolePermission(value.RoleId, int.Parse(value.PermissionList[i]), value.CreateOperator);
                        }
                        catch (Exception e)
                        {
                            r.result = false;
                            r.message = "数据库操作失败!角色权限新增失败，" + e.Message;
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
                        }//角色权限新增
                        if (insertUserRoleCount == 1)
                        { insertCount = insertCount + 1; }
                    }
                    if (insertCount == value.PermissionList.Count)
                    {
                        r.result = true;
                        r.message = "角色权限设置成功！";
                    }
                    else
                    {
                        r.result = false;
                        r.message = "角色部分权限设置失败，失败原因：数据库操作失败!";
                    }
                }
                else
                {
                    r.result = false;
                    r.message = "请选择权限!";
                }
                #region  日志写入 
                Log log = new Log();
                log.Type = "角色权限";
                if (r.result == true)
                { log.Result = "成功"; }
                else
                { log.Result = "失败"; }
                log.Message = roleNowName + r.message + "角色Id：" + value.RoleId;
                log.Creator = value.CreateOperator;
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return r;
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "角色权限设置失败!请检查输入参数！失败原因：" + e.Message;
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
        /// 获取所有用户的所有权限
        /// </summary>
        /// <returns></returns>
        public static List<Permission> TRWCS_GetRolePermissionApi()
        {
            List<Permission> rolePermissions = new List<Permission>();
            try
            {
                rolePermissions = Da_rolePermission.GetRolePermissionById();
                return rolePermissions;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("数据库操作失败！角色权限列表获取失败，" + e.Message)),
                    ReasonPhrase = "角色权限列表获取失败"
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
