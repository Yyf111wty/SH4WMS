using MySqlConnector;
using System;
using System.Collections.Generic;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class Da_rolePermission
    {
        /// <summary>
        /// 获取所有用户的所有权限
        /// </summary>
        /// <returns></returns>
        public static List<Permission> GetRolePermissionById()
        {
            List<Permission> perList = new List<Permission>();
            string sql = "select b.RoleId,a.Id,a.NodeId,a.NodeName,a.IsEnabled,a.ImageUrl,a.Icon,a.Type,a.LinkUrl,a.ControllerName,a.ActionName,a.KeyName,a.ParentId as _parentId,a.SortNo,a.CreateTime,a.CreateOperator,a.UpdateTime,a.UpdateOperator,a.State,a.NodeType " +
                "from sys_Permissions a right join sys_RolePermission b on b.PermissionId=a.NodeId ";
            perList = SqlHelper.Query<Permission>(sql, null);
            for (int i = 0; i < perList.Count; i++)
            {
                for (int j = 0; j < perList.Count; j++)
                {
                    if (perList[j].Id == perList[i]._parentId)
                    {
                        perList[i].ParentName = perList[j].NodeName;
                        break;
                    }
                }
            }
            return perList;
        }
        /// <summary>
        /// 删除角色的所有权限
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public static int DeleteRolePermission(int RoleId)
        {
            string sql = "delete from sys_RolePermission where RoleId=@RoleId";
            MySqlParameter[] sqlparamter = new MySqlParameter[]
             {
               new MySqlParameter("@RoleId", MySqlDbType.Int32) { Value=RoleId},
             };
            int deleteCount = SqlHelper.ExecuteNonQuery(sql, sqlparamter);
            return deleteCount;
        }
        /// <summary>
        /// 根据角色ID和权限ID插入角色权限数据
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="PermissionId"></param>
        /// <returns></returns>
        public static int InsertRolePermission(int RoleId,int PermissionId,string CreateOperator)
        {
            try
            {
                string sql = "insert into sys_RolePermission (RoleId,PermissionId,CreateTime,CreateOperator) " +
                    "values (@RoleId,@PermissionId,@CreateTime,@CreateOperator)";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@PermissionId", MySqlDbType.Int32){ Value =PermissionId},
                   new MySqlParameter("@RoleId", MySqlDbType.Int32){ Value =RoleId},
                   new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = CreateOperator},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
    }
}