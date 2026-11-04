using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TR.ORM;

namespace TR.DAL
{
    public partial class BaseDal<T>
        where T : class, new()
    {
        protected SqlSugarClient db = null;

        public BaseDal()
        {
            db = SugarContext.GetInstance();
        }

        #region 基础访问

        /// <summary>
        /// 功能描述:查询数据列表
        /// </summary>
        public List<T> Query(Expression<Func<T, bool>> whereExpression)
        {
            return SqlSugarClientFactory.ExecuteWithRetry(() => db.Queryable<T>().Where(whereExpression).ToList());
        }

        /// <summary>
        /// 功能描述:插入数据，返回受影响数
        /// </summary>
        public int InsertReturnIdentity(T entity)
        {
            return SqlSugarClientFactory.ExecuteWithRetry(() => db.Insertable<T>(entity).ExecuteCommand());
        }

        /// <summary>
        /// 修改对象
        /// </summary>
        public bool Update(T entity)
        {
            try
            {
                return SqlSugarClientFactory.ExecuteWithRetry(() => db.Updateable<T>(entity).ExecuteCommand() > 0);
            }
            catch (Exception ex)
            {
                throw new Exception("修改对象失败", ex);
            }
        }

        /// <summary>
        /// 功能描述:更新数据列表
        /// </summary>
        public bool Update(Expression<Func<T, bool>> updateExpression, Expression<Func<T, bool>> whereExpression)
        {
            return SqlSugarClientFactory.ExecuteWithRetry(() =>
                db.Updateable<T>().SetColumns(updateExpression).Where(whereExpression).ExecuteCommand() >= 0);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        public bool Delete(T entity)
        {
            try
            {
                return SqlSugarClientFactory.ExecuteWithRetry(() => db.Deleteable<T>(entity).ExecuteCommand() > 0);
            }
            catch (Exception ex)
            {
                throw new Exception("删除对象失败", ex);
            }
        }

        /// <summary>
        /// 功能描述:分页查询
        /// </summary>
        public List<T> QueryPage(
         Expression<Func<T, bool>> whereExpression,
         ref int intTotalCount,
         int intPageIndex = 0,
         int intPageSize = 20,
         string strOrderByFileds = null)
        {
            int totalCount = intTotalCount;
            List<T> list = SqlSugarClientFactory.ExecuteWithRetry(() =>
            {
                int count = totalCount;
                List<T> pageList = db.Queryable<T>()
                    .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                    .WhereIF(whereExpression != null, whereExpression)
                    .ToPageList(intPageIndex, intPageSize, ref count);
                totalCount = count;
                return pageList;
            });
            intTotalCount = totalCount;
            return list;
        }

        #endregion
    }
}
