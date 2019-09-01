using System;
using System.Data.SqlClient;
using System.Text;
using VRPTW.Configuration;

namespace VRPTW.Data
{
    public class DBManager
    {
        private SqlConnection _conn = null;
        private static DBManager _connInstance = new DBManager();

        public static DBManager GetInstance()
        {
            if (_connInstance == null)
            {
                _connInstance = new DBManager();
            }
            return _connInstance;
        }

        private string GenerateConnectionString()
        {
            StringBuilder connectionString = new StringBuilder();
            connectionString.Append("Data Source=").Append(Config.GetConnectionString().DNS).Append(",")
                                                   .Append(Config.GetConnectionString().Port).Append(";");
            connectionString.Append("Initial Catalog=").Append(Config.GetConnectionString().DBName).Append(";");
            connectionString.Append("User Id=").Append(Config.GetConnectionString().Username).Append(";");
            connectionString.Append("Password=").Append(Config.GetConnectionString().Password).Append(";");
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