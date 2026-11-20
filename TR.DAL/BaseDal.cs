using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TR.ORM;

namespace TR.DAL
{
    public partial class BaseDal<T>
        where T : class, new()
    {
        /// <summary>
        /// 翻页时复用上一次统计总数的秒数。设为 0 则每次翻页都重新统计。
        /// 首屏（第 1 页）始终重新统计，保证查询按钮拿到的总数是最新的。
        /// </summary>
        public static int PageTotalCacheSeconds = 10;

        private static readonly ConcurrentDictionary<string, TotalCountEntry> TotalCountCache =
            new ConcurrentDictionary<string, TotalCountEntry>(StringComparer.Ordinal);

        private sealed class TotalCountEntry
        {
            public int Total;
            public DateTime ExpireAt;
        }

        /// <summary>
        /// 每次访问都取当前请求的数据库客户端，避免 Dal 实例长期持有其它请求的连接。
        /// </summary>
        protected SqlSugarClient db
        {
            get { return SugarContext.GetInstance(); }
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
            int pageIndex = intPageIndex <= 0 ? 1 : intPageIndex;
            int pageSize = intPageSize <= 0 ? 20 : intPageSize;

            int totalCount = intTotalCount;
            List<T> list = SqlSugarClientFactory.ExecuteWithRetry(() =>
            {
                int count = totalCount;
                var query = db.Queryable<T>()
                    .OrderByIF(!string.IsNullOrEmpty(strOrderByFileds), strOrderByFileds)
                    .WhereIF(whereExpression != null, whereExpression);

                string cacheKey = null;
                int cachedTotal;
                if (pageIndex > 1
                    && PageTotalCacheSeconds > 0
                    && TryGetCachedTotal(query, out cacheKey, out cachedTotal))
                {
                    count = cachedTotal;
                    return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }

                List<T> pageList = query.ToPageList(pageIndex, pageSize, ref count);
                SaveCachedTotal(query, cacheKey, count);
                return pageList;
            });
            intTotalCount = totalCount;
            return list;
        }

        #endregion

        #region 总数缓存

        private bool TryGetCachedTotal(ISugarQueryable<T> query, out string cacheKey, out int total)
        {
            cacheKey = null;
            total = 0;

            string key;
            if (!TryBuildTotalCacheKey(query, out key))
            {
                return false;
            }

            cacheKey = key;

            TotalCountEntry entry;
            if (!TotalCountCache.TryGetValue(key, out entry))
            {
                return false;
            }

            if (entry.ExpireAt <= DateTime.Now)
            {
                TotalCountCache.TryRemove(key, out entry);
                return false;
            }

            total = entry.Total;
            return true;
        }

        private void SaveCachedTotal(ISugarQueryable<T> query, string cacheKey, int total)
        {
            if (PageTotalCacheSeconds <= 0 || total < 0)
            {
                return;
            }

            string key = cacheKey;
            if (string.IsNullOrEmpty(key) && !TryBuildTotalCacheKey(query, out key))
            {
                return;
            }

            TotalCountCache[key] = new TotalCountEntry
            {
                Total = total,
                ExpireAt = DateTime.Now.AddSeconds(PageTotalCacheSeconds)
            };

            PruneExpiredTotalCache();
        }

        private static bool TryBuildTotalCacheKey(ISugarQueryable<T> query, out string key)
        {
            key = null;
            try
            {
                var sql = query.ToSql();
                if (sql.Key == null)
                {
                    return false;
                }

                var sb = new StringBuilder(256);
                sb.Append(typeof(T).FullName).Append('|').Append(sql.Key);
                if (sql.Value != null)
                {
                    foreach (var parameter in sql.Value)
                    {
                        sb.Append('|').Append(parameter.ParameterName).Append('=').Append(parameter.Value);
                    }
                }

                key = sb.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void PruneExpiredTotalCache()
        {
            if (TotalCountCache.Count < 512)
            {
                return;
            }

            DateTime now = DateTime.Now;
            foreach (var pair in TotalCountCache)
            {
                if (pair.Value.ExpireAt <= now)
                {
                    TotalCountCache.TryRemove(pair.Key, out _);
                }
            }
        }

        #endregion
    }
}
