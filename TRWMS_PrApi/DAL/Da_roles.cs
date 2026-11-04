using MySqlConnector;
using System;
using System.Collections.Generic;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class Da_roles
    {
        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        public static List<Roles> GetRolesList()
        {
            try
            {
                List<Roles> roleList = new List<Roles>();
                string sql = "select * from sys_Roles order by Id asc";
                roleList = SqlHelper.Query<Roles>(sql, null);
                return roleList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据角色名称获取角色列表
        /// </summary>
        /// <param name="RoleName"></param>
        /// <returns></returns>
        public static List<Roles> GetRoleByRoleName(string RoleName)
        {
            try
            {
                List<Roles> roleList = new List<Roles>();
                string sql = "select * from sys_Roles where RoleName= '"+RoleName+"' order by Id asc";
                roleList = SqlHelper.Query<Roles>(sql, null);
                return roleList;
            }
            catch (Exception e)
            { throw e; }
        }
        public static List<Roles> GetRoleByRoleLevel(string roleLevel)
        {
            try
            {
                List<Roles> roleList = new List<Roles>();
                string sql = "select * from sys_Roles where RoleLevel= '" + roleLevel + "' order by Id asc";
                roleList = SqlHelper.Query<Roles>(sql, null);
                return roleList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 插入角色数据行
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int InsertRole(Roles value)
        {
            try
            {
                value.RoleId = NewRoleId();
                string sql = "insert into sys_Roles (RoleId,RoleName,IsEnabled,Describes,RoleLevel,CreateTime,CreateOperator) " +
                             "values (@RoleId,@RoleName,@IsEnabled,@Describes,@RoleLevel,@CreateTime,@CreateOperator)";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                    new MySqlParameter("@RoleId", MySqlDbType.Int32){ Value = value.RoleId},
                    new MySqlParameter("@RoleName", MySqlDbType.String,50) { Value= value.RoleName},
                    new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = value.IsEnabled},
                    new MySqlParameter("@Describes", MySqlDbType.String,50) { Value= value.Describes},
                    new MySqlParameter("@RoleLevel", MySqlDbType.Int32){ Value = value.RoleLevel},
                    new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                    new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = value.CreateOperator},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        public static int NewRoleId()
        {
            try
            {
                string sqlMaxRoleId = "select max(RoleId) as  id from sys_Roles";
                List<MaxId> maxRoleIdList = SqlHelper.Query<MaxId>(sqlMaxRoleId, null);
                int maxRoleId = maxRoleIdList[0].id;
                int newRoleId = maxRoleId + 1;
                return newRoleId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 修改角色
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int EditRole(Roles value)
        {
            try
            {
                string sql = "update sys_Roles SET " +
             "RoleName=@RoleName,IsEnabled=@IsEnabled,Describes=@Describes,RoleLevel=@RoleLevel,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where Id=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= value.Id},
                   new MySqlParameter("@RoleName", MySqlDbType.String,50){ Value = value.RoleName},
                   new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = value.IsEnabled},
                   new MySqlParameter("@Describes", MySqlDbType.String,200){ Value = value.Describes},
                   new MySqlParameter("@RoleLevel", MySqlDbType.Int32){ Value = value.RoleLevel},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =value.UpdateOperator},
                 };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数              
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据RoleId查找角色
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Roles> GetRolesByRoleId(int roleId)
        {
            try
            {
                List<Roles> roleList = new List<Roles>();
                string sql = "select * from sys_Roles where RoleId='" + roleId + "' order by Id asc";
                roleList = SqlHelper.Query<Roles>(sql, null);
                return roleList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据Id查找角色
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Roles> GetRolesById(int Id)
        {
            try
            {
                List<Roles> roleList = new List<Roles>();
                string sql = "select * from sys_Roles where Id='" + Id + "' order by Id asc";
                roleList = SqlHelper.Query<Roles>(sql, null);
                return roleList;
            }
            catch (Exception e)
            { throw e; }
        }
        public static List<UserRole> GetUserByRoleId(int roleId)
        {
            try
            {
                List<UserRole> userRoleList = new List<UserRole>();
                string sql = "select * from sys_UserRole where RoleId='"+roleId+"' order by Id asc";
                userRoleList = SqlHelper.Query<UserRole>(sql, null);
                return userRoleList;
            }
            catch (Exception e)
            { throw e; }
        }
        public static int DeleteRole(int Id,int roleId)
        {
            try
            {
                string sql = "delete from sys_Roles where Id=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= Id},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数
                string sql0 = "delete from sys_RolePermission where RoleId=@roleId";
                MySqlParameter[] sqlparamter0 = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@roleId", MySqlDbType.Int32) { Value= roleId},
                };
                var result0 = SqlHelper.ExecuteNonQuery(sql0, sqlparamter0);//返回受影响的函数
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
    }
    
}