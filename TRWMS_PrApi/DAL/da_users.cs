using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TRWCS_WebAPI.Common;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class da_users
    {
        /// <summary>
        /// 登录界面，用户名及密码查找
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static List<Users> GetCountByNandP(string userName,string password)
        {
            List<Users> userList = new List<Users>();
           string sql = "select * from sys_Users where UserName='"+userName+"' and Password='"+password+"'";
           // string sql = "select * from sys_Users ";
            userList = SqlHelper.Query<Users>(sql, null);
            return userList;
        }
        /// <summary>
        /// 获取用户信息列表
        /// </summary>
        /// <returns></returns>
        public static List<Users> GetUsersList()
        {
            try
            {
                List<Users> userList = new List<Users>();               
                string sql = "select sys_Users.Id ,sys_Users.SecretKeyType,sys_Users.SecretKeyValue,sys_Users.UserId,sys_Users.UserName,sys_Users.Password,sys_Users.TrueName,sys_Users.UserGroupId as UserGroupId,sys_Users.Mobile,sys_Users.Email, sys_Users.CreateTime,sys_Users.CreateOperator,sys_Users.UpdateTime,sys_Users.UpdateOperator,  sys_Groups.GroupName as UserGroupName  from sys_Users  left  join  sys_Groups  on  sys_Users.UserGroupId=sys_Groups.GroupId  order by sys_Users.Id asc";
                userList = SqlHelper.Query<Users>(sql, null);
                return userList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据组别ID查询用户列表
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<Users> GetUsersListByGI(int groupId)
        {
            try
            {
                List<Users> userList = new List<Users>();
                string sql = "select * from sys_Users where UserGroupId='"+groupId+"' order by Id asc";
                userList = SqlHelper.Query<Users>(sql, null);
                return userList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 通过查找Id获取用户行
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Users> GetUsersListById(int Id)
        {
            try
            {
                List<Users> userList = new List<Users>();
                string sql = "select * from sys_Users where Id='" + Id + "' order by Id asc";
                userList = SqlHelper.Query<Users>(sql, null);
                return userList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 通过查找UserId获取用户行
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static List<Users> GetUsersListByUserId(int UserId)
        {
            try
            {
                List<Users> userList = new List<Users>();
                string sql = "select * from sys_Users where UserId='" + UserId + "' order by Id asc";
                userList = SqlHelper.Query<Users>(sql, null);
                return userList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据用户名查询用户列表
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static List<Users> GetUsersByName(string userName)
        {
            try
            {
                List<Users> userList = new List<Users>();
                string sql = "select * from sys_Users where UserName='" + userName + "' order by Id asc";
                userList = SqlHelper.Query<Users>(sql, null);
                return userList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 新增用户数据
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static int InsertNewUser(Users users)
        {
            try
            {
                //查询秘钥是否重复

                users.UserId = NewUserId();
                string checksql = "select * from sys_Users  where SecretKeyValue='"+ users.SecretKeyValue+ "' and SecretKeyValue    is   not  null and SecretKeyValue!='' ";
                int checkresult = SqlHelper.Query<Users>(checksql,null,CommandType.Text).Count();//返回受影响的函数 
                if (checkresult > 0)
                {
                                
                    return 0;
                }
                string sql = "insert into sys_Users (UserId,UserName,Password,TrueName,UserGroupId,Mobile,Email,CreateTime,CreateOperator,SecretKeyType,SecretKeyValue) " +
                    "values (@UserId,@UserName,@Password,@TrueName,@UserGroupId,@Mobile,@Email,@CreateTime,@CreateOperator,@SecretKeyType,@SecretKeyValue)";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@UserId", MySqlDbType.Int32){ Value =users.UserId},
                   new MySqlParameter("@UserName", MySqlDbType.String,50) { Value= users.UserName},
                   new MySqlParameter("@Password", MySqlDbType.String,50) { Value= users.Password},
                   new MySqlParameter("@TrueName", MySqlDbType.String,50) { Value= users.TrueName},
                   new MySqlParameter("@UserGroupId", MySqlDbType.Int32){ Value =users.UserGroupId},
                   new MySqlParameter("@Mobile", MySqlDbType.String,50){ Value = users.Mobile},
                   new MySqlParameter("@Email", MySqlDbType.String,50){ Value = users.Email},
                   new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = users.CreateTime},
                   new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = users.CreateOperator},
                   new MySqlParameter("@SecretKeyType", MySqlDbType.String,100){ Value = users.SecretKeyType},
                   new MySqlParameter("@SecretKeyValue", MySqlDbType.String,200){ Value = users.SecretKeyValue}
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 新增用户的UserId
        /// </summary>
        /// <returns></returns>
        public static int NewUserId()
        {
            try
            {
                string sqlMaxUserId = "select max(UserId) as  id from sys_Users";
                List<MaxId> maxUserIdList = SqlHelper.Query<MaxId>(sqlMaxUserId, null);
                int maxUserId = maxUserIdList[0].id;
                int newUserId = maxUserId + 1;
                return newUserId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 用户数据修改
        /// </summary>
        /// <param name="editUser"></param>
        /// <returns></returns>
        public static int EditUser(Users editUser)
        {

            string checksql = "select  * from sys_Users  where SecretKeyValue='" + editUser.SecretKeyValue + "' and SecretKeyValue  is  not  null and SecretKeyValue!=''  and  Id!=" + editUser.Id;
            int checkresult = SqlHelper.Query<Users>(checksql, null, CommandType.Text).Count();//返回受影响的函数 
            if (checkresult > 0)
            {

                return 0;
            }
            string sql = "update sys_Users SET " +
             "UserName=@UserName,Password=@Password,TrueName=@TrueName,UserGroupId=@UserGroupId,Mobile=@Mobile,Email=@Email,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator,SecretKeyType=@SecretKeyType,SecretKeyValue=@SecretKeyValue  " +
            "where Id=@Id";
            MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
             {
                   new  MySqlParameter("@Id", MySqlDbType.Int32) { Value= editUser.Id},
                   new  MySqlParameter("@UserName", MySqlDbType.String,50) { Value= editUser.UserName},
                   new  MySqlParameter("@Password", MySqlDbType.String,50) { Value= MD5Encrypt.Encrypt(editUser.Password)},
                   new  MySqlParameter("@TrueName", MySqlDbType.String,50) { Value= editUser.TrueName},
                   new  MySqlParameter("@UserGroupId", MySqlDbType.Int32){ Value =editUser.UserGroupId},
                   new  MySqlParameter("@Mobile", MySqlDbType.String,50){ Value = editUser.Mobile},
                   new  MySqlParameter("@Email", MySqlDbType.String,50){ Value = editUser.Email},
                   new  MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new  MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value = editUser.UpdateOperator},
                    new MySqlParameter("@SecretKeyType", MySqlDbType.String,100){ Value = editUser.SecretKeyType},
                   new MySqlParameter("@SecretKeyValue", MySqlDbType.String,200){ Value = editUser.SecretKeyValue}
             };
            var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
            return result;
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int DeleteUser(int id,int userId)
        {
            try
            {
                string sql = "delete from sys_Users where Id=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= id},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数
                string sql0 = "delete from sys_UserRole where UserId=@Id";
                MySqlParameter[] sqlparamter0 = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= userId},
                };
                var result0 = SqlHelper.ExecuteNonQuery(sql0, sqlparamter0);//返回受影响的函数
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int DeleteUserRole(int userId)
        {
            string sql = "delete from sys_UserRole where UserId=@Id";
            MySqlParameter[] sqlparamter = new MySqlParameter[]
             {
               new MySqlParameter("@Id", MySqlDbType.Int32) { Value=userId},
             };
            int deleteCount = SqlHelper.ExecuteNonQuery(sql, sqlparamter);
            return deleteCount;
        }
        /// <summary>
        /// 插入用户的单个角色
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static int InsertUserRole(int userId, int roleId,string createOperator)
        {
            try
            {
                string sql = "insert into sys_UserRole (UserId,RoleId,CreateTime,CreateOperator) " +
                    "values (@UserId,@RoleId,@CreateTime,@CreateOperator)";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@UserId", MySqlDbType.Int32){ Value =userId},
                   new MySqlParameter("@RoleId", MySqlDbType.Int32){ Value =roleId},
                   new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = createOperator},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 更新用户密码
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static int EditPassword(Users users)
        {
            string sql = "update sys_Users SET Password=@Password,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator where UserName=@UserName";
            MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
             {
               
                new MySqlParameter("@UserName", MySqlDbType.String,50) { Value=users.UserName},
                new MySqlParameter("@Password", MySqlDbType.String,50) { Value= MD5Encrypt.Encrypt(users.Password)},
                new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value = users.UserName},
             };
            var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
            return result;
        }
        /// <summary>
        /// 关联查询用户的所有角色
        /// </summary>
        /// <returns></returns>
        public static List<Users> GetUsersRole()
        {
            List<Users> userList = new List<Users>();
            string sql = "select a.Id,a.UserId,a.UserName,a.TrueName,a.UserGroupId,a.Mobile,a.Email,c.RoleId,c.RoleName,c.Describes ,d.GroupName as UserGroupName from sys_Users a left join sys_UserRole b on a.UserId=b.UserId left join sys_Roles c on b.RoleId = c.RoleId  left  join sys_Groups  d  on  a.UserGroupId=d.GroupId ";
            userList = SqlHelper.Query<Users>(sql, null);
            return userList;
        }
        /// <summary>
        /// 通过用户名获取用户权限
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static List<Permission> GetUserPermissionByNandNT(string UserName,string NodeType)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select distinct  d.UserName,a.Id,a.NodeId,a.NodeName,a.IsEnabled,a.ImageUrl,a.Icon ,a.Type,a.LinkUrl,a.ControllerName,a.ActionName,a.KeyName,a.ParentId as _parentId,a.SortNo,a.CreateTime,a.CreateOperator,a.UpdateTime,a.UpdateOperator,a.State,a.NodeType " +
                   "from sys_Permissions a left join sys_RolePermission b on b.PermissionId=a.NodeId left join sys_UserRole c on c.RoleId=b.RoleId left join sys_Users d on d.UserId=c.UserId where d.UserName='" + UserName + "' and a.IsEnabled=1 order by a.SortNo";

                perList = SqlHelper.Query<Permission>(sql, null);
                var parentNameById = new Dictionary<int, string>();
                for (int i = 0; i < perList.Count; i++)
                {
                    parentNameById[perList[i].Id] = perList[i].NodeName;
                }
                for (int i = 0; i < perList.Count; i++)
                {
                    string parentName;
                    if (parentNameById.TryGetValue(perList[i]._parentId, out parentName))
                    {
                        perList[i].ParentName = parentName;
                    }
                }
                return perList;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}