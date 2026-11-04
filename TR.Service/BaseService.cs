using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TR.DalFactory;
using TR.IDAL;

namespace TR.Service
{
    public abstract partial class BaseService<T>
        where T:class
    {
        protected IDbSession dbSession = DbSessionFactory.GetDbSession();
        protected IBaseDal<T> curDal;
        protected abstract IBaseDal<T> GetDal();

        public BaseService()
        {
            curDal = GetDal();
        }


        /// <summary>
        /// 功能描述:查询数据列表      
        /// </summary>
        /// <param name="whereExpression">whereExpression</param>
        /// <returns>数据列表</returns>
        public List<T> Query(Expression<Func<T, bool>> whereExpression)
        {
            return curDal.Query(whereExpression);
        }

        /// <summary>
        /// 功能描述:插入数据，返回受影响数      
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>行数</returns>
        public int InsertReturnIdentity(T entity)
        {
            return curDal.InsertReturnIdentity(entity);
        }

        /// <summary>
        /// 修改对象
        /// </summary>
        /// <param name="entity">对象</param>
        /// <returns></returns>
        public bool Update(T entity)
        {
            return curDal.Update(entity);
        }
        /// <summary>
        /// 功能描述:更新数据列表      
        /// </summary>
        /// <param name="whereExpression">whereExpression</param>
        /// <returns>数据列表</returns>
        public bool Update(Expression<Func<T, bool>> updateExpression, Expression<Func<T, bool>> whereExpression)
        {
            return curDal.Update(updateExpression, whereExpression);
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="entity">对象</param>
        /// <returns></returns>
        public bool Delete(T entity)
        {
            return curDal.Delete(entity);
        }
        /// <summary>
        /// 功能描述:分页查询       
        /// </summary>
        /// <param name="strWhere">条件</param>
        /// <param name="intTotalCount">数据总数</param>
        /// <param name="intPageIndex">当前页</param>
        /// <param name="intPageSize">页大小</param>
        /// <param name="lstParameters">参数</param>
        /// <param name="strOrderByFileds">排序字段，如name asc,age desc</param>
        /// <returns>数据列表</returns>
        public List<T> QueryPage(
         Expression<Func<T, bool>> whereExpression,
         ref int intTotalCount,
         int intPageIndex = 0,
         int intPageSize = 20,
         string strOrderByFileds = null)
        {
            return curDal.QueryPage(whereExpression,ref intTotalCount, intPageIndex, intPageSize, strOrderByFileds);
        }

    }
}
