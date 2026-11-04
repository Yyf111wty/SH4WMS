using MySqlConnector;
using System;
using System.Collections.Generic;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class Da_log
    {
        /// <summary>
        /// 日志写入
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static int insertLog(Log log)
        {
            if (log.Creator == null)
            {
                log.Creator ="system";
            }
            string sql = "insert into sys_Log (ID,Type,Result,Message,Creator,CreateTime) values (@ID,@Type,@Result,@Message,@Creator,@CreateTime)";
           MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
           {
              new MySqlParameter("@ID", MySqlDbType.Guid) { Value= Guid.NewGuid()},
              new MySqlParameter("@Type", MySqlDbType.String,50) { Value= log.Type},
              new MySqlParameter("@Result", MySqlDbType.String,20) { Value= log.Result},
              new MySqlParameter("@Message", MySqlDbType.String){ Value = log.Message},
              new MySqlParameter("@Creator", MySqlDbType.String,50){ Value = log.Creator},
              new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = log.CreateTime},
           };
           var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
           return result;
        }

        public static int insertException(ExceptionDatabase exception)
        {
            string sql = "insert into sys_Exception (Source,Message,StackTrace,TargetSite,Data,Creator,CreateTime) values (@Source,@Message,@StackTrace,@TargetSite,@Data,@Creator,@CreateTime)";
            MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
            {
              new MySqlParameter("@Source", MySqlDbType.String) { Value= exception.Source},
              new MySqlParameter("@Message", MySqlDbType.String) { Value= exception.Message},
              new MySqlParameter("@StackTrace", MySqlDbType.String){ Value =exception.StackTrace},
              new MySqlParameter("@TargetSite", MySqlDbType.String) { Value= exception.TargetSite},
              new MySqlParameter("@Data", MySqlDbType.String) { Value= exception.Data},
              new MySqlParameter("@Creator", MySqlDbType.String,50){ Value = exception.Creator},
              new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = exception.CreateTime},
            };
            var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
            return result;
        }
        public static List<Log> GetActionLogList()
        {
            try
            {
                List<Log> actionLogList = new List<Log>();
                string sql = "select * from sys_Log order by CreateTime desc";
                actionLogList = SqlHelper.Query<Log>(sql, null);
                return actionLogList;
            }
            catch (Exception e)
            { throw e; }
        }
        public static List<ExceptionDatabase> GetExceptionLogList()
        {
            try
            {
                List<ExceptionDatabase> exceptionLogList = new List<ExceptionDatabase>();
                string sql = "select * from sys_Exception order by CreateTime desc";
                exceptionLogList = SqlHelper.Query<ExceptionDatabase>(sql, null);
                return exceptionLogList;
            }
            catch (Exception e)
            { throw e; }
        }

    }
}