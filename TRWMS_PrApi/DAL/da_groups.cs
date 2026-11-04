using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class da_groups
    {
        /// <summary>
        /// 获取全部组别数据
        /// </summary>
        /// <returns></returns>
        public static List<Group> GetGroupsList()
        {
            try
            {
                List<Group> groupList = new List<Group>();
                string sql = "select * from sys_Groups order by Id asc";
                groupList = SqlHelper.Query<Group>(sql, null);
                for (int i=0;i<groupList.Count;i++)
                {
                    for (int j=0;j<groupList.Count;j++)
                    {
                        if (groupList[j].GroupId==groupList[i].ParentGroupId)
                        {
                            groupList[i].ParentGroupName =groupList[j].GroupName;
                            break;
                        }
                    }                   
                }
                return groupList;
            }
            catch (Exception e)
            { throw e; }
        }

        /// <summary>
        /// 获取作业班组别数据
        /// </summary>
        /// <returns></returns>
        public static List<Group> GetClasssList()
        {
            try
            {
                List<Group> groupList = new List<Group>();
                string sql = "select * from sys_Groups  where ParentGroupId=1 order by Id asc";
                groupList = SqlHelper.Query<Group>(sql, null);
                for (int i = 0; i < groupList.Count; i++)
                {
                    for (int j = 0; j < groupList.Count; j++)
                    {
                        if (groupList[j].GroupId == groupList[i].ParentGroupId)
                        {
                            groupList[i].ParentGroupName = groupList[j].GroupName;
                            break;
                        }
                    }
                }
                return groupList;
            }
            catch (Exception e)
            { throw e; }
        }

        /// <summary>
        /// 获取作业组数据
        /// </summary>
        /// <returns></returns>
        public static List<Group> GetGroupByPidList(string  pid)
        {
            try
            {
                List<Group> groupList = new List<Group>();
                string sql = "select * from sys_Groups  where ParentGroupId="+int.Parse(pid)+" order by Id asc";
                groupList = SqlHelper.Query<Group>(sql, null);
                for (int i = 0; i < groupList.Count; i++)
                {
                    for (int j = 0; j < groupList.Count; j++)
                    {
                        if (groupList[j].GroupId == groupList[i].ParentGroupId)
                        {
                            groupList[i].ParentGroupName = groupList[j].GroupName;
                            break;
                        }
                    }
                }
                return groupList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据Id获取组别列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Group> GetGroupsListById(int id)
        {
            try
            {
                List<Group> groupList = new List<Group>();
                string sql = "select * from sys_Groups where Id='"+id+"' order by Id asc";
                groupList = SqlHelper.Query<Group>(sql, null);
                return groupList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据组别名及父组ID获取组别列表
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="parentGroupId"></param>
        /// <returns></returns>
        public static List<Group> GetGroupsListByNandP(string groupName,int parentGroupId)
        {
            try
            {
                List<Group> groupList = new List<Group>();
                string sql = "select * from sys_Groups where GroupName='"+groupName+"' and ParentGroupId='"+parentGroupId+"'";
                groupList = SqlHelper.Query<Group>(sql,null);              
                return groupList;
            }
            catch (Exception e)
            { throw e; }
        }

        /// <summary>
        /// 向sys_Groups表中插入一行数据,返回值-1：总公司已存在，0：0行受影响（操作失败），1：一行受影响（操作成功）
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static int InsertNewGroup(Group group)
        {
            try
            {
                int groupParentCount=0;
                if (group.ParentGroupId==0)
                {
                    string sqlByPId = "select * from sys_Groups where ParentGroupId=0";
                    groupParentCount = SqlHelper.Query<Group>(sqlByPId, null).Count;
                }
                if (group.ParentGroupId == 0 && groupParentCount != 0)
                {
                    return -1;
                }
                else
                {
                   // group.GroupId = NewGroupId();
                    string sql = "insert into sys_Groups (GroupId,GroupName,ParentGroupId,IsEnabled,Describes,CreateTime,CreateOperator,State) values (@GroupId,@GroupName,@ParentGroupId,@IsEnabled,@Describes,@CreateTime,@CreateOperator,@State)";
                    MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                    {
                            //new MySqlParameter("@Id", MySqlDbType.Int32){ Value =NewId()},
                            new MySqlParameter("@GroupId", MySqlDbType.Int32){ Value =NewGroupId()},
                            new MySqlParameter("@GroupName", MySqlDbType.String,50) { Value= group.GroupName},
                            new MySqlParameter("@ParentGroupId", MySqlDbType.Int32){ Value =group.ParentGroupId},
                            new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = group.IsEnabled},
                            new MySqlParameter("@Describes", MySqlDbType.String,200){ Value = group.Describes},
                            new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = group.CreateTime},
                            new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = group.CreateOperator},
                            new MySqlParameter("@State", MySqlDbType.String,50){ Value = group.State},
                    };
                    var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                    return result;
                }
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("数据库插入数据失败！" + e.Message),
                    ReasonPhrase = "数据库操作失败"
                };
                throw new HttpResponseException(resp);
            }
        }
        /// <summary>
        /// 新增组别的Id
        /// </summary>
        /// <returns></returns>
        public static int NewId()
        {
            try
            {
                string sqlMaxGroupId = "select max(Id) as id from sys_Groups  ";
                List<MaxId> maxGroupIdList = SqlHelper.Query<MaxId>(sqlMaxGroupId, null);
                int maxGroupId = maxGroupIdList[0].id;
                int newGroupId = maxGroupId + 1;
                return newGroupId;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("最大Id获取失败！" + e.Message),
                    ReasonPhrase = "数据库操作失败"
                };
                throw new HttpResponseException(resp);
            }
        }
        /// <summary>
        /// 新增组别的GroupId
        /// </summary>
        /// <returns></returns>
        public static int NewGroupId()
        {
            try
            {
                string sqlMaxGroupId = "select max(GroupId) as id from sys_Groups  ";
                List<MaxId> maxGroupIdList = SqlHelper.Query<MaxId>(sqlMaxGroupId, null);
                int maxGroupId = maxGroupIdList[0].id;
                int newGroupId=maxGroupId+1;
                return newGroupId;
            }
            catch (Exception e)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("最大GroupId获取失败！" + e.Message),
                    ReasonPhrase = "数据库操作失败"
                };
                throw new HttpResponseException(resp);
            }
        }
        /// <summary>
        /// 更新组别数据行
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static int EditGroup(Group group)
        {
            try
            {
                string sql = "update sys_Groups SET " +
                             "GroupName=@GroupName,ParentGroupId=@ParentGroupId,IsEnabled=@IsEnabled,Describes=@Describes,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
                            "where Id=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= group.Id},
                   new MySqlParameter("@GroupName", MySqlDbType.String,50) { Value= group.GroupName},
                   new MySqlParameter("@ParentGroupId", MySqlDbType.Int32){ Value =group.ParentGroupId},
                   new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = group.IsEnabled},
                   new MySqlParameter("@Describes", MySqlDbType.String,200){ Value = group.Describes},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = group.UpdateTime},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value = group.UpdateOperator},
                 };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数              
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 删除组别数据行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int DeleteGroup(int id)
        {
            try
            {
                string sql = "delete from sys_Groups where Id=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= id},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
    }
}