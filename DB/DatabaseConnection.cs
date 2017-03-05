using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DBProjectComparer.DB
{
    static class DatabaseConnection
    {
        public static IDbConnection GetConnectionDB1()
        {
            var connStr = ConfigurationManager.AppSettings["db1"];
            var sqlConnection = new SqlConnection(connStr);
            return sqlConnection;
        }

        public static IDbConnection GetConnectionDB2()
        {
            var connStr = ConfigurationManager.AppSettings["db2"];
            var sqlConnection = new SqlConnection(connStr);
            return sqlConnection;
        }

    }

}
