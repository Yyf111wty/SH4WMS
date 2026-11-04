using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using TRWMS_PrApi.Models;
using TRWMS_PrApi.sqlhelper;

namespace TRWMS_PrApi.DAL
{
    public class Da_permission
    {
        /// <summary>
        /// 新增模块、页面和按钮
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int InsertPermission(Permission permission)
        {
            try
            {
                permission.NodeId = NewNodeId();
                string sql = "insert into sys_Permissions (NodeId,NodeName,IsEnabled,Type,Icon,ImageUrl,LinkUrl,ControllerName,ActionName,KeyName,ParentId,SortNo,CreateTime,CreateOperator,NodeType) " +
                             "values (@NodeId,@NodeName,@IsEnabled,@Type,@Icon,@ImageUrl,@LinkUrl,@ControllerName,@ActionName,@KeyName,@ParentId,@SortNo,@CreateTime,@CreateOperator,@NodeType)";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                {
                    new MySqlParameter("@NodeId", MySqlDbType.Int32) { Value=permission.NodeId},
                    new MySqlParameter("@NodeName", MySqlDbType.String,50) { Value=permission.NodeName},
                    new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = permission.IsEnabled},
                    new MySqlParameter("@Type", MySqlDbType.String,20) { Value= permission.Type},
                    new MySqlParameter("@Icon", MySqlDbType.String,20) { Value= permission.Icon},
                    new MySqlParameter("@ImageUrl", MySqlDbType.String,50) { Value=permission.ImageUrl},
                    new MySqlParameter("@LinkUrl", MySqlDbType.String,50) { Value=permission.LinkUrl},
                    new MySqlParameter("@ControllerName", MySqlDbType.String,50) { Value=permission.ControllerName},
                    new MySqlParameter("@ActionName", MySqlDbType.String,50) { Value=permission.ActionName},
                    new MySqlParameter("@ParentId", MySqlDbType.Int32) { Value=permission._parentId},
                    new MySqlParameter("@SortNo", MySqlDbType.Int32) { Value=permission.SortNo},
                    new MySqlParameter("@KeyName", MySqlDbType.String,20) { Value=permission.KeyName},
                    new MySqlParameter("@CreateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                    new MySqlParameter("@CreateOperator", MySqlDbType.String,20){ Value = permission.CreateOperator},
                    new MySqlParameter("@NodeType", MySqlDbType.String,20) { Value=permission.NodeType},
                };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数               
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 新增权限的NodeId
        /// </summary>
        /// <returns></returns>
        public static int NewNodeId()
        {
            try
            {
                string sqlMaxNodeId = "select max(NodeId) as id from sys_Permissions";
                List<MaxId> maxNodeIdList = SqlHelper.Query<MaxId>(sqlMaxNodeId, null);
                int maxNodeId = maxNodeIdList[0].id;
                int newNodeId = maxNodeId + 1;
                return newNodeId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 获取所有权限列表
        /// </summary>
        /// <returns></returns>
        public static List<Permission> GetPermissionList(string NodeType)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                //string  sql = "select Id,NodeId,NodeName,IsEnabled,Type,Icon,ImageUrl,LinkUrl,ControllerName,ActionName,KeyName,ParentId _parentId,SortNo,CreateTime,CreateOperator,State,NodeType" +
                //        " from sys_Permissions where isnull(Nodetype,'')='" + NodeType + "' or isnull(Nodetype,'')='Permission'  order by SortNo asc";
                string sql = "select Id,NodeId,NodeName,IsEnabled,Type,Icon,ImageUrl,LinkUrl,ControllerName,ActionName,KeyName,ParentId _parentId,SortNo,CreateTime,CreateOperator,State,NodeType" +
                       " from sys_Permissions  order by SortNo asc";
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
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据Node ID获取权限
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissionByParentId(int Id)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select * from sys_Permissions  where NodeId='"+Id+"'";
                perList = SqlHelper.Query<Permission>(sql, null);
                return perList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissionById(int Id)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select * from sys_Permissions  where NodeId='" + Id + "'";
                perList = SqlHelper.Query<Permission>(sql, null);
                return perList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static List<Permission> GetDelPermissionById(int Id)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select * from sys_Permissions  where Id='" + Id + "'";
                perList = SqlHelper.Query<Permission>(sql, null);
                return perList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 按权限名称和平台查找权限列表
        /// </summary>
        /// <param name="NodeName"></param>
        ///  /// <param name="NodeType"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissionListByName(string NodeName,string NodeType)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select * from sys_Permissions where NodeName= '" + NodeName + "'  and  NodeType='"+ NodeType + "' order by Id asc";
                perList = SqlHelper.Query<Permission>(sql, null);
                return perList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取权限模块列表
        /// </summary>
        /// <returns></returns>
        public static int  GetPermissionModularList()
        {
            try
            {
                string perList = "0";
                string sql = "select ifnull(MAX(SortNo),100) as SortNo from sys_Permissions where Type='模块'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据父组ID获取权限页面
        /// </summary>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static int  GetPermissionPageListByS(int ParentId)
        {
            try
            {
                string perList = "0";
                string sql = "select ifnull(MAX(SortNo),100) as SortNo from sys_Permissions where ParentId='" + ParentId + "' and Type='页面'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据父组ID获取权限按钮
        /// </summary>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static int GetPermissionBtnListByS(int ParentId)
        {
            try
            {
                string perList = "0";
                string sql = "select ifnull(MAX(SortNo),1) as SortNo from sys_Permissions where ParentId='" + ParentId + "' and Type='按钮'";
                perList = SqlHelper.GetMaxORMin(sql);
                return  int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据名称及父节点Id查找满足条件的权限列表
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissionListByNandPI(string NodeName, string NodeType,  int ParentId)
        {
            try
            {
                List<Permission> btnList = new List<Permission>();
                string sql = "select * from sys_Permissions where NodeName= '" + NodeName + "' and NodeType='" + NodeType + "'  and ParentId='"+ParentId+"' order by Id asc";
                btnList = SqlHelper.Query<Permission>(sql, null);
                return btnList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 根据Node Name获取对应的权限数据行
        /// </summary>
        /// <param name="NodeName"></param>
        /// <returns></returns>
        public static List<Permission> GetPermissionByNodeName(string NodeName,int  id)
        {
            try
            {
                List<Permission> perList = new List<Permission>();
                string sql = "select * from sys_Permissions where NodeName= '" + NodeName + "'  and  NodeId!=" + id+"   order by Id asc";
                perList = SqlHelper.Query<Permission>(sql, null);
                return perList;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 编辑模块
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int EditPermissionModular(Permission permission)
        {
            try
            {
                string sql = "update sys_Permissions SET " +
             "NodeName=@NodeName,IsEnabled=@IsEnabled,Icon=@icon,ImageUrl=@ImageUrl,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator ,NodeType=@NodeType " +
            "where NodeId=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= permission.Id},
                   new MySqlParameter("@NodeName", MySqlDbType.String,50){ Value = permission.NodeName},
                   new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = permission.IsEnabled},
                   new MySqlParameter("@icon", MySqlDbType.String,20){ Value = permission.Icon},
                   new MySqlParameter("@ImageUrl", MySqlDbType.String,50){ Value = permission.ImageUrl},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                   new MySqlParameter("@NodeType", MySqlDbType.String,20){ Value = permission.NodeType},
                 };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数              
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 页面编辑
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int EditPermissionPage(Permission permission)
        {
            try
            {
                string sql = "update sys_Permissions SET " +
             "NodeName=@NodeName,IsEnabled=@IsEnabled,Icon=@icon,ImageUrl=@ImageUrl,LinkUrl=@LinkUrl,ControllerName=@ControllerName,ActionName=@ActionName,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator ,NodeType=@NodeType " +
            "where NodeId=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= permission.Id},
                   new MySqlParameter("@NodeName", MySqlDbType.String,50){ Value = permission.NodeName},
                   new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = permission.IsEnabled},
                   new MySqlParameter("@icon", MySqlDbType.String,20){ Value = permission.Icon},
                   new MySqlParameter("@ImageUrl", MySqlDbType.String,50){ Value = permission.ImageUrl},
                   new MySqlParameter("@LinkUrl", MySqlDbType.String,50) { Value=permission.LinkUrl},
                   new MySqlParameter("@ControllerName", MySqlDbType.String,50) { Value=permission.ControllerName},
                   new MySqlParameter("@ActionName", MySqlDbType.String,50) { Value=permission.ActionName},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                   new MySqlParameter("@NodeType", MySqlDbType.String,20){ Value = permission.NodeType},
                 };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数              
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 按钮编辑
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int EditPermissionBtn(Permission permission)
        {
            try
            {
                string sql = "update sys_Permissions SET " +
             "NodeName=@NodeName,IsEnabled=@IsEnabled,Icon=@icon,ImageUrl=@ImageUrl,KeyName=@KeyName,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator ,NodeType=@NodeType " +
            "where NodeId=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= permission.Id},
                   new MySqlParameter("@NodeName", MySqlDbType.String,50){ Value = permission.NodeName},
                   new MySqlParameter("@IsEnabled", MySqlDbType.Int32){ Value = permission.IsEnabled},
                   new MySqlParameter("@icon", MySqlDbType.String,20){ Value = permission.Icon},
                   new MySqlParameter("@ImageUrl", MySqlDbType.String,50){ Value = permission.ImageUrl},
                   new MySqlParameter("@KeyName", MySqlDbType.String,50) { Value=permission.KeyName},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                   new MySqlParameter("@NodeType", MySqlDbType.String,20){ Value = permission.NodeType},
                 };
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数              
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 删除模块、页面和按钮
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int DeletePermission(int Id,int nodeId)
        {
            try
            {
                string sql = "delete from sys_Permissions where Id=@Id";
                string sql2 = "delete from sys_RolePermission where PermissionId=@Id";
                MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= Id},
                 };
                MySqlParameter[] sqlparamter2 = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= nodeId},
                 };              
                var result = SqlHelper.ExecuteNonQuery(sql, sqlparamter);//返回受影响的函数
                var result2 = SqlHelper.ExecuteNonQuery(sql2, sqlparamter2);//返回受影响的函数
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 删除页面权限
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int DeletePagePermission(int Id,int nodeId)
        {
            try
            {
                //获取页面及页面下按钮的ID列表
                string sqlSelect = "select Id from sys_Permissions where ParentId='"+ nodeId + "'";
                string sqlSelect1 = "select NodeId from sys_Permissions where ParentId='" + nodeId + "'";
                //SqlParameter[] sqlparamterSelect = new SqlParameter[]//参数数组
                // {
                //   new SqlParameter("@Id", SqlDbType.Int) { Value= Id},
                // };

                List<Permission> childIdList = SqlHelper.Query<Permission>(sqlSelect, null);
                List<Permission> childNodeIdList = SqlHelper.Query<Permission>(sqlSelect1, null);
                Permission p = new Permission();
                p.Id = Id; 
                p.NodeId = nodeId;
                childIdList.Add(p);
                childNodeIdList.Add(p);
                //删除角色权限表中与权限相关的角色权限
                for (int i=0;i<childNodeIdList.Count;i++)
                {
                    string sqlDelete3 = "delete from sys_RolePermission where PermissionId=@Id";
                    MySqlParameter[] sqlparamter3 = new MySqlParameter[]//参数数组
                    {
                       new MySqlParameter("@Id", MySqlDbType.Int32) { Value= childNodeIdList[i].NodeId},
                    };
                    var result3 = SqlHelper.ExecuteNonQuery(sqlDelete3, sqlparamter3);//返回受影响的函数
                }
                //删除页面及页面下按钮的权限
                string sqlDelete1 = "delete from sys_Permissions where Id=@Id";
                int result = 0;
                int result1=0;
                for (int i = 0; i < childIdList.Count; i++)
                {
                    MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                     {
                         new MySqlParameter("@Id", MySqlDbType.Int32) { Value= childIdList[i].Id},
                     };
                     result1 = SqlHelper.ExecuteNonQuery(sqlDelete1, sqlparamter);//返回受影响的函数
                    result = result + result1;
                }
                if (result != childIdList.Count)
                {
                    result = 0;
                }
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 删除模块权限
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int DeleteModularPermission(int Id,int nodeId)
        {
            try
            {
                //获取模块及模块下页面及按钮的ID列表
                string sqlSelect = "select Id from sys_Permissions where ParentId='"+nodeId+"'";
                List<Permission> childIdList = SqlHelper.Query<Permission>(sqlSelect, null).ToList();
                string sqlSelect1 = "select NodeId from sys_Permissions where ParentId='" + nodeId + "'";
                List<Permission> childNodeIdList = SqlHelper.Query<Permission>(sqlSelect1, null).ToList();
                List<Permission> pageList = childNodeIdList;
                for (int i = 0; i < pageList.Count; i++)
                {
                    string sqlSelect2 = "select Id from sys_Permissions where ParentId='"+ pageList[i].NodeId + "'";
                    List<Permission> btnIdList = SqlHelper.Query<Permission>(sqlSelect2, null).ToList();
                    string sqlSelect4 = "select NodeId from sys_Permissions where ParentId='" + pageList[i].NodeId + "'";
                    List<Permission> btnNodeIdList = SqlHelper.Query<Permission>(sqlSelect4, null).ToList();
                    for (int j = 0; j < btnIdList.Count; j++)
                    {
                        childIdList.Add(btnIdList[j]);
                        childNodeIdList.Add(btnNodeIdList[j]);
                    }
                }
                Permission p = new Permission();
                p.Id = Id;
                p.NodeId = nodeId;
                childIdList.Add(p);
                childNodeIdList.Add(p);
                ////删除角色权限表中与权限相关的角色权限
                int result1 = 0;                
                for (int i = 0; i < childIdList.Count; i++)
                {
                    //删除角色权限表中与权限相关的角色权限
                    string sqlDelete3 = "delete from sys_RolePermission where PermissionId=@Id";
                    MySqlParameter[] sqlparamter3 = new MySqlParameter[]//参数数组
                    {
                       new MySqlParameter("@Id", MySqlDbType.Int32) { Value= childNodeIdList[i].NodeId},
                    };
                    var result3 = SqlHelper.ExecuteNonQuery(sqlDelete3, sqlparamter3);//返回受影响的函数
                    //删除模块、页面及页面下按钮的权限
                    string sqlDelete1 = "delete from sys_Permissions where Id=@Id";
                    MySqlParameter[] sqlparamter = new MySqlParameter[]//参数数组
                    {
                       new MySqlParameter("@Id", MySqlDbType.Int32) { Value= childIdList[i].Id},
                     };
                     var resultpermission = SqlHelper.ExecuteNonQuery(sqlDelete1, sqlparamter);//返回受影响的函数
                    if (resultpermission == 1)
                    { result1 = result1 + 1; }                   
                }
                int result = result1;
                if (result1 != childIdList.Count)
                {
                    result = 0;
                }
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取模块的最小sortNo
        /// </summary>
        /// <returns></returns>
        public static int GetMinModularSortNo()
        {
            try
            {
                string perList = "0";
                string sql = "select MIN(SortNo) as SortNo from sys_Permissions where Type='模块'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取模块的最大sortNo
        /// </summary>
        /// <returns></returns>
        public static int GetMaxModularSortNo()
        {
            try
            {
                string perList = "0";
                string sql = "select MAX(SortNo) as SortNo from sys_Permissions where Type='模块'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取页面的最小SortNo
        /// </summary>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static int GetMinPageSortNo(int ParentId)
        {
            try
            {
                string perList = "0";
                string sql = "select MIN(SortNo) as SortNo from sys_Permissions where ParentId='" + ParentId + "'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取页面的最大sortNo
        /// </summary>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static int GetMaxPageSortNo(int ParentId)
        {
            try
            {
                string perList = "0";
                string sql = "select MAX(SortNo) as SortNo from sys_Permissions where ParentId='" + ParentId + "'";
                perList = SqlHelper.GetMaxORMin(sql);
                return int.Parse(perList);
            }
            catch (Exception e)
            { throw e; }
        } 
        /// <summary>
        /// 获取权限的上一个数据行
        /// </summary>
        /// <param name="sortNo"></param>
        /// <returns></returns>
        public static Permission GetLastPermission(int sortNo)
        {
            try
            {
                Permission lastPermission=new Permission();
                List<Permission> modularList = new List<Permission>();
                string sql = "select * from sys_Permissions where Type='模块' order by SortNo asc";
                modularList = SqlHelper.Query<Permission>(sql, null);
                for (int i = 0; i<=modularList.Count; i++)
                {
                    if (modularList[i].SortNo == sortNo)
                    {
                        lastPermission = modularList[i - 1];
                        break;
                    }
                }
                return lastPermission;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取权限的下一个数据行
        /// </summary>
        /// <param name="sortNo"></param>
        /// <returns></returns>
        public static Permission GetNextPermission(int sortNo)
        {
            try
            {
                Permission nextPermission = new Permission();
                List<Permission> modularList = new List<Permission>();
                string sql = "select * from sys_Permissions where Type='模块' order by SortNo asc";
                modularList = SqlHelper.Query<Permission>(sql, null);
                for (int i = 0; i <= modularList.Count; i++)
                {
                    if (modularList[i].SortNo == sortNo)
                    {
                        nextPermission = modularList[i+1];
                        break;
                    }
                }
                return nextPermission;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取页面的上一个页面数据行
        /// </summary>
        /// <param name="sortNo"></param>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static Permission GetLastPagePermission(int sortNo,int ParentId)
        {
            try
            {
                Permission lastPermission = new Permission();
                List<Permission> modularList = new List<Permission>();
                string sql = "select * from sys_Permissions where ParentId='"+ ParentId + "' order by SortNo asc";
                modularList = SqlHelper.Query<Permission>(sql, null);
                for (int i = 0; i <= modularList.Count; i++)
                {
                    if (modularList[i].SortNo == sortNo)
                    {
                        lastPermission = modularList[i - 1];
                        break;
                    }
                }
                return lastPermission;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 获取页面的下一个页面数据行
        /// </summary>
        /// <param name="sortNo"></param>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        public static Permission GetNextPagePermission(int sortNo, int ParentId)
        {
            try
            {
                Permission nextPermission = new Permission();
                List<Permission> modularList = new List<Permission>();
                string sql = "select * from sys_Permissions where ParentId='" + ParentId + "' order by SortNo asc";
                modularList = SqlHelper.Query<Permission>(sql, null);
                for (int i = 0; i <= modularList.Count; i++)
                {
                    if (modularList[i].SortNo == sortNo)
                    {
                        nextPermission = modularList[i+1];
                        break;
                    }
                }
                return nextPermission;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 改变两个模块的sortNo
        /// </summary>
        /// <param name="lastper"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int ChangeModularPermission(Permission lastper, Permission permission)
        {
            try
            {
                //将要上移的模块下的页面的sortNo值-100，完成页面上移
                string sqlPageDownToUp = "update sys_Permissions SET " +
             "SortNo=SortNo-@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where ParentId=@ParentId";
                MySqlParameter[] sqlparamterPageDownToUp = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@ParentId", MySqlDbType.Int32) { Value= permission.Id},
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= permission.SortNo-lastper.SortNo},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                 };
                var resultsqlPageDownToUp = SqlHelper.ExecuteNonQuery(sqlPageDownToUp, sqlparamterPageDownToUp);//返回受影响的函数 
                //将要下移的模块下的页面sortNo值+100，完成页面下移
                string sqlPageUpToDown = "update sys_Permissions SET " +
                                         "SortNo=SortNo+@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
                                         "where ParentId=@ParentId";
                MySqlParameter[] sqlparamterPageUpToDown = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@ParentId", MySqlDbType.Int32) { Value= lastper.Id},//要下移的页面的父组ID
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= permission.SortNo-lastper.SortNo},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                 };
                var resultsqlPageUpToDown = SqlHelper.ExecuteNonQuery(sqlPageUpToDown, sqlparamterPageUpToDown);
                //交换两个模块的sortNo值
                string sqlModularDownToUp = "update sys_Permissions SET " +
             "SortNo=SortNo-@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where Id=@Id";
                MySqlParameter[] sqlparamterModularDownToUp = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= permission.Id},//要上移的模块的ID
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= permission.SortNo-lastper.SortNo},
                    new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                 };
                var resultsqlModularDownToUp = SqlHelper.ExecuteNonQuery(sqlModularDownToUp, sqlparamterModularDownToUp);

                string sqlModularUpToDown = "update sys_Permissions SET " +
             "SortNo=SortNo+@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where Id=@Id";
                MySqlParameter[] sqlparamterModularUpToDown = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= lastper.Id},//要上移的模块的ID
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= permission.SortNo-lastper.SortNo},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =permission.UpdateOperator},
                 };
                var resultsqlModularUpToDown = SqlHelper.ExecuteNonQuery(sqlModularUpToDown, sqlparamterModularUpToDown);
                int result = resultsqlModularDownToUp+ resultsqlModularUpToDown;
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
        /// <summary>
        /// 改变两个页面的sortNo
        /// </summary>
        /// <param name="nextId"></param>
        /// <param name="lastId"></param>
        /// <param name="gapSortNo"></param>
        /// <param name="updateOperator"></param>
        /// <returns></returns>
        public static int ChangePagePermission(int nextId, int lastId,int gapSortNo,string updateOperator)
        {
            try
            {
                //交换两个页面的sortNo值
                string sqlPageDownToUp = "update sys_Permissions SET " +
             "SortNo=SortNo-@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where Id=@Id";
                MySqlParameter[] sqlparamterPageDownToUp = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= nextId},//要上移页面的ID
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= gapSortNo},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =updateOperator},
                 };
                var resultsqlPageDownToUp = SqlHelper.ExecuteNonQuery(sqlPageDownToUp, sqlparamterPageDownToUp);

                string sqlPageUpToDown = "update sys_Permissions SET " +
             "SortNo=SortNo+@gapSortNo,UpdateTime=@UpdateTime,UpdateOperator=@UpdateOperator " +
            "where Id=@Id";
                MySqlParameter[] sqlparamterPageUpToDown = new MySqlParameter[]//参数数组
                 {
                   new MySqlParameter("@Id", MySqlDbType.Int32) { Value= lastId},//要下移的页面ID
                   new MySqlParameter("@gapSortNo", MySqlDbType.Int32) { Value= gapSortNo},
                   new MySqlParameter("@UpdateTime", MySqlDbType.DateTime){ Value = DateTime.Now.ToString()},
                   new MySqlParameter("@UpdateOperator", MySqlDbType.String,20){ Value =updateOperator},
                 };
                var resultsqlPageUpToDown = SqlHelper.ExecuteNonQuery(sqlPageUpToDown, sqlparamterPageUpToDown);
                int result = resultsqlPageDownToUp + resultsqlPageUpToDown;
                return result;
            }
            catch (Exception e)
            { throw e; }
        }
    }
}