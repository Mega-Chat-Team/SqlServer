using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Timers;
using MicrosoftSqlServer.TableManagement;
using Timer = System.Timers.Timer;

namespace MicrosoftSqlServer.Client
{
    public delegate void LogEvent(string message);

    public abstract class SqlClient
    {
        public string SqlConnectionString { get; protected set; }

        public abstract SqlConnection SqlConnection
        {
            get;
        }

        public TableManager CreateTableManager(string TableName)
        {
            TableManager table = new TableManager(this, TableName);

            return table;
        }

        public SqlCommand CreateCommand(string CommandQuery)
        {
            SqlCommand sqlCommand = new SqlCommand(CommandQuery, SqlConnection);

            return sqlCommand;
        }

        public bool TableExists(string TableName)
        {
            string query = $"IF OBJECT_ID(N'dbo.{TableName}', N'U') IS NOT NULL SELECT 1 ELSE SELECT 0";

            using (SqlCommand command = CreateCommand(query))
            {
                return (int)command.ExecuteScalar() == 1;
            }
        }

        public event LogEvent OnLogMessage;

        protected void LogMessage(string message)
        {
            //DateTime dateTime = new DateTime();

            OnLogMessage?.Invoke(message);
        }
    }
}