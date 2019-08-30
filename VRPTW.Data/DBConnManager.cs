using System;
using System.Data.SqlClient;
using System.Text;

namespace VRPTW.Data
{
    public class DBConnManager
    {
        private SqlConnection _conn = null;
        private static DBConnManager _connInstance = new DBConnManager();

        private static readonly string DNS = "";
        private static readonly string PORT = "";
        private static readonly string DBNAME = "";
        private static readonly string USERNAME = "";
        private static readonly string PASSWORD = "";

        public static DBConnManager GetInstance()
        {
            if (_connInstance == null)
            {
                _connInstance = new DBConnManager();
            }
            return _connInstance;
        }

        private string GenerateConnectionString()
        {
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append("Data Source=").Append(DNS).Append(",").Append(PORT).Append(";");
            connectionString.Append("Initial Catalog=").Append(DBNAME).Append(";");
            connectionString.Append("User Id=").Append(USERNAME).Append(";");
            connectionString.Append("Password=").Append(PASSWORD).Append(";");
            return connectionString.ToString();
        }

        private bool OpenConnection()
        {
            _conn = new SqlConnection(GenerateConnectionString());

            try
            {
                _conn.Open();
                return true;
            }
            catch (SqlException e)
            {
                Console.Error.WriteLine(e);
                return false;
            }
        }

        public SqlConnection GetConnection()
        {
            if (_conn == null)
            {
                if (OpenConnection())
                {
                    Console.WriteLine("Connection opened!");
                    return _conn;
                }
                else
                {
                    return null;
                }
            }
            return _conn;
        }

        public void Close()
        {
            Console.WriteLine("Connection closed!");
            try
            {
                _conn.Close();
                _conn = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}