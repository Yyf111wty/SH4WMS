using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using MySqlConnector;

namespace TRWMS_PrApi.sqlhelper
{
    
    public class SqlHelper
    {
        protected static MySqlConnection conn;
        public static MySqlCommand cmd = null;
        public static string connString = System.Configuration.ConfigurationManager.ConnectionStrings["TRWCSConnection"].ConnectionString;
        private static MySqlConnection GetConnection()
        {
            MySqlConnection sqlConnection = new MySqlConnection();
            sqlConnection.ConnectionString = connString;
            try
            {
                sqlConnection.Open();
            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw ex;
            }
            return sqlConnection;
        }

        public static List<T> Query<T>(string sql, object obj = null, CommandType commandType = CommandType.Text)
        {
            MySqlConnection sqlConnection = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, sqlConnection);
            try
            {
                DataSet dt = new DataSet();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);              
                sqlConnection.Close();             
                return ConvertTo <T>(dt.Tables[0]);

            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw new ApplicationException("查询数据异常" + ex.Message);
            }
        }
        public static T ExecuteScalar<T>(string sql, object obj)
        {
            MySqlConnection sqlConnection = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, sqlConnection);
            try
            {
                DataSet dt = new DataSet();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);

                sqlConnection.Close();

                return ConvertTo<T>(dt.Tables[0]).FirstOrDefault();

            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw new ApplicationException("查询数据异常" + ex.Message);
            }

        }

        public static string GetMaxORMin(string sql)
        {
            MySqlConnection sqlConnection = GetConnection();
            MySqlCommand cmd = new MySqlCommand(sql, sqlConnection);
            try
            {
                DataSet dt = new DataSet();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(dt);

                sqlConnection.Close();

                return dt.Tables[0].Rows[0][0].ToString();

            }
            catch (Exception ex)
            {
                sqlConnection.Close();
                throw new ApplicationException("查询数据异常" + ex.Message);
            }
        }
        /// <summary>
        /// 执行SQL语句，不返回任何对象。
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        public static int ExecuteNonQuery(string commandText, MySqlParameter[] commandParm)
        {
            MySqlConnection sqlConnection = GetConnection();
            MySqlCommand sqlCommand = new MySqlCommand(commandText, sqlConnection);
            PrepareCommand(sqlCommand, commandParm);
            int rowCount = 0;
            try
            {
                rowCount = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
            catch (Exception e)
            {
                rowCount = -1;
                sqlConnection.Close();
                throw e;
            }
            return rowCount;
        }

        public static DataTable GetDataTable(string cmdText, CommandType cmdType, MySqlParameter[] cmdParms)
        {
            MySqlDataReader reader;
            DataTable dt = new DataTable();
            try
            {
                using (conn = GetConnection())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd = new MySqlCommand();
                    SetCommand(cmd, cmdText, cmdType, cmdParms);
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    dt.Load(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return dt;
        }

        private static void SetCommand(MySqlCommand cmd, string cmdText, CommandType cmdType, MySqlParameter[] cmdParms)//增加数据
        {
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                cmd.Parameters.AddRange(cmdParms);
            }
        }

        private static void PrepareCommand(MySqlCommand sqlCommand, MySqlParameter[] commandParms)
        {
            if (commandParms != null)
            {
                foreach (MySqlParameter parameter in commandParms)
                {
                    if (parameter.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                    sqlCommand.Parameters.Add(parameter);
                }
            }
        }

        private static List<T> ConvertTo<T>(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            List<DataRow> rows = new List<DataRow>();

            foreach (DataRow row in table.Rows)
            {
                rows.Add(row);
            }

            return ConvertTo<T>(rows);
        }

        private static List<T> ConvertTo<T>(List<DataRow> rows)
        {
            List<T> list = null;

            if (rows != null)
            {
                list = new List<T>();

                foreach (DataRow row in rows)
                {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }

        private static T CreateItem<T>(DataRow row)
        {
            T obj = default(T);
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
                    try
                    {
                        object value = row[column.ColumnName];
                        prop.SetValue(obj, value, null);
                    }
                    catch
                    {      
                    }
                }
            }

            return obj;
        }
    }
}