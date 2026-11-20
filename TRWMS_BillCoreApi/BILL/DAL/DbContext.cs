using SqlSugar;
using TR.ORM;

namespace TRXWMS.BILL.DAL
{
    public class DbContext
    {
        /// <summary>
        /// 每次访问都取当前请求的数据库客户端。
        /// 不要在构造函数里缓存，否则实例被静态字段引用时会跨请求共享连接与事务。
        /// </summary>
        public SqlSugarClient Db
        {
            get { return SugarContext.GetInstance(); }
        }
    }
}
