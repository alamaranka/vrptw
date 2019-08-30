using System;
using System.Data.SqlClient;
using System.Text;
using VRPTW.Configuration;

namespace VRPTW.Data
{
    public class DBConnManager
    {
        private SqlConnection _conn = null;
        private static DBConnManager _connInstance = new DBConnManager();

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
            var config = new Config().GetConnectionString();
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append("Data Source=").Append(config.DNS).Append(",").Append(config.Port).Append(";");
            connectionString.Append("Initial Catalog=").Append(config.DBName).Append(";");
            connectionString.Append("User Id=").Append(config.Username).Append(";");
            connectionString.Append("Password=").Append(config.Password).Append(";");
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