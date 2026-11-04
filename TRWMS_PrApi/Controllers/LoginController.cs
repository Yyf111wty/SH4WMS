using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.DAL;
using System.Web;
using System.Web.Http.Results;
using System.Web.Security;
using TRWCS_WebAPI.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TRWMS_PrApi.Controllers
{
    public class LoginController 
    {
        public static LoginReturn TRWCS_LoginApi(JObject json)
        {
            List<Users> userList = new List<Users>();
            Login_get value = JsonConvert.DeserializeObject<Login_get>( JsonConvert.SerializeObject(json));
            LoginReturn r = new LoginReturn();
            try
            {
                #region 判断
                if (String.IsNullOrEmpty(value.Sign))
                {
                    r = new LoginReturn();
                    r.result = false;
                    r.message = "Sign不能为空！";
                    return r;
                }
                if (String.IsNullOrEmpty(value.UnixTime))
                {
                    r = new LoginReturn();
                    r.result = false;
                    r.message = "调用时间戳不能为空！";
                    return r;
                }
                else
                {
                    long unixtimetmp = long.Parse(value.UnixTime);
                    if (ConvertDateTimeInt(new DateTime()) - unixtimetmp > 500 || unixtimetmp - ConvertDateTimeInt(new DateTime()) > 500)
                    {
                        r = new LoginReturn();
                        r.result = false;
                        r.message = "请求已过期！";
                        return r;
                    }
                }

                bool result = value.Sign.Equals(MD5Encrypt.Encrypt(value.UserName + value.Password + value.UnixTime));
                if (result == false)
                {
                    r = new LoginReturn();
                    r.result = false;
                    r.message = "签名不正确！";
                    return r;
                }

                #endregion
                string password = MD5Encrypt.Encrypt(value.Password);
                try
                {
                    userList = da_users.GetCountByNandP(value.UserName, password);
                }
                catch (Exception e)
                {
                    r.result = false;
                    r.message = "登录失败！失败原因：数据库操作失败!" + e.Message;
                    return r;
                }//数据库操作
                if (userList.Count>= 1)
                {
                    r.result = true;
                    r.message = "登录成功";
                    r.trueName = userList[0].TrueName;
                    r.UserGroupId = userList[0].UserGroupId.ToString();
                    r.UserId = userList[0].UserId.ToString();
                    #region  日志写入               
                    Log log = new Log();
                    log.Type = "登录";
                    log.Result = "成功";
                    log.Message =  value.UserName+r.message;
                    log.Creator = value.UserName;
                    log.CreateTime = DateTime.Now.ToString();
                    int logCount = Da_log.insertLog(log);
                    #endregion
                    return r;
                }
                else
                {
                    r.result = false;
                    r.message = "登录失败！失败原因：查不到该用户，账号或密码错误";
                    return r;
                }
            }
            catch (Exception e)
            {
                r.result = false;
                r.message = "登录失败！失败原因：接口程序执行失败!"+e.Message;
                return r;
            }
        }
        public static long ConvertDateTimeInt(System.DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
        public static List<Permission> TRWCS_GetUserPermissionDataApi(string UserName,string NodeType)
        {
            int database = 0;
            List<Permission> userPermissions = new List<Permission>();
            try
            {
                try
                {
                    userPermissions = da_users.GetUserPermissionByNandNT(UserName,NodeType);
                }
                catch (Exception e)
                {
                    database = 1;
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！" + e.Message)),
                        ReasonPhrase = "通过用户名获取用户权限失败！"
                    };
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.Creator = UserName;
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    throw new HttpResponseException(resp);
                }
                return userPermissions;
            }
            catch (Exception e)
            {
                if (database == 1)
                {
                    throw e;
                }
                else
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("接口程序执行失败！" + e.Message)),
                        ReasonPhrase = "非数据库错误"
                    };
                    #region  异常写入
                    ExceptionDatabase exception = new ExceptionDatabase();
                    exception.Source = e.Source;
                    exception.Message = e.Message;
                    exception.StackTrace = e.StackTrace;
                    exception.TargetSite = e.TargetSite.ToString();
                    exception.Data = e.Data.ToString();
                    exception.Creator = UserName;
                    exception.CreateTime = DateTime.Now.ToString();
                    int ExceptionCount = Da_log.insertException(exception);
                    #endregion
                    throw new HttpResponseException(resp);
                }
            }
        }
    }
}