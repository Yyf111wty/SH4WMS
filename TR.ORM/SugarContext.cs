using SqlSugar;

namespace TR.ORM
{
    public class SugarContext
    {
        public static string DB_ConnectionString
        {
            get { return SqlSugarClientFactory.DB_ConnectionString; }
            set { SqlSugarClientFactory.DB_ConnectionString = value; }
        }

        public static SqlSugarClient GetInstance()
        {
            return SqlSugarClientFactory.GetInstance();
        }
    }
}
