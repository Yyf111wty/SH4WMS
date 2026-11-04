using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TR.IDAL
{
    public partial interface IBaseDal<T> 
    {

        List<T> Query(Expression<Func<T, bool>> whereExpression);
        int InsertReturnIdentity(T entity);
        bool Update(T entity);
        bool Update(Expression<Func<T, bool>> updateExpression, Expression<Func<T, bool>> whereExpression);
        bool Delete(T entity);
        List<T> QueryPage(
          Expression<Func<T, bool>> whereExpression,
          ref int intTotalCount,
          int intPageIndex = 0,
          int intPageSize = 20,
          string strOrderByFileds = null);
    }
}
