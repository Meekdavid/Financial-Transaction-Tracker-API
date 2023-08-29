using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Common
{
    public class ConnectionManager
    {
        private static IDbConnection _connection;

        public static void openSQLConnection(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(connectionString);
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public static SqlConnection SqlDatabaseCreateConnection(string connectionString, bool openConnection = false)
        {
            var conn = new SqlConnection(connectionString);

            if (openConnection)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
            }

            return conn;
        }

        public static OracleConnection OracleDatabaseCreateConnection(string connectionString, bool openConnection = false)
        {
            try
            {
                var conn = new OracleConnection(connectionString);

                if (openConnection)
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                }

                return conn;
            }
            catch (Exception ex)
            {
                var messa = ex.Message;
                return null;
            }
        }

        public static void openOracleConnection(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new OracleConnection(connectionString);
            }
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public static void closeConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        public static void closeOpenedConnection1(IDbConnection openedConnection)
        {
            if (openedConnection != null)
            {
                openedConnection.Close();
                openedConnection.Dispose();
            }
        }

        public static void closeOpenedConnection2(IDbConnection openedConnection)
        {
            if (openedConnection != null)
            {
                openedConnection.Close();
                openedConnection.Dispose();
            }
        }
    }
}
