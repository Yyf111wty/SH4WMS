using SqlSugar;
using TR.ORM;

namespace TRXWMS.BILL.DAL
{
    public class DbContext
    {
        public SqlSugarClient Db { get; private set; }

        public DbContext()
        {
            Db = SugarContext.GetInstance();
        }
    }
}
