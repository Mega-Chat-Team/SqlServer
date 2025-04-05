using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftSqlServer.Client
{
    public class FastSqlClient : SqlClient
    {
        public override SqlConnection SqlConnection
        {
            get
            {
                SqlConnection sqlConnection = new SqlConnection(SqlConnectionString);
                sqlConnection.Open();

                return sqlConnection;
            }
        }

        public FastSqlClient(string ServerName, string DataBaseName, string UserId, string UserPassword)
        {
            SwitchConnection(ServerName, DataBaseName, UserId, UserPassword);
        }

        public void SwitchConnection(string ServerName, string DataBaseName, string UserId, string UserPassword)
        {
            SqlConnectionString = $"Server={ServerName};Database={DataBaseName};User Id={UserId};Password={UserPassword};";
        }
    }
}