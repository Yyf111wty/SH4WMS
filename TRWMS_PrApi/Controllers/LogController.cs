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
    public class LogController
    {
        public static List<Log> TRWCS_GetActionLogApi()
        {
            try
            {
                List<Log> actionLogList = new List<Log>();
                try
                {
                    actionLogList = Da_log.GetActionLogList();
                }
                catch (Exception e)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！操作日志列表获取失败，" + e.Message)),
                        ReasonPhrase = "操作日志列表获取失败"
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
                #region  日志写入               
                Log log = new Log();
                log.Type = "日志管理";
                log.Result = "成功";
                log.Message = "获取所有操作日志数据";
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return actionLogList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static List<ExceptionDatabase> TRWCS_GetExceptionLogApi()
        {
            try
            {
                List<ExceptionDatabase> exceptionLogList = new List<ExceptionDatabase>();
                try
                {
                    exceptionLogList = Da_log.GetExceptionLogList();
                }
                catch (Exception e)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent(string.Format("数据库操作失败！异常日志列表获取失败，" + e.Message)),
                        ReasonPhrase = "异常日志列表获取失败"
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
                #region  日志写入               
                Log log = new Log();
                log.Type = "日志管理";
                log.Result = "成功";
                log.Message = "获取所有异常日志数据";
                log.CreateTime = DateTime.Now.ToString();
                int logCount = Da_log.insertLog(log);
                #endregion
                return exceptionLogList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
