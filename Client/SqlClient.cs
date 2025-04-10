using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Timers;
using SqlServer.TableManagement;

namespace SqlServer.Client
{
    public delegate void LogEvent(string message);

    public class SqlClient : IDisposable
    {
        private SqlConnection sqlConnection;

        public SqlConnection SqlConnection
        {
            get
            {
                if (!CheckConnection())
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();

                    while (sqlConnection.State != ConnectionState.Open)
                    {
                        TryConnect();

                        Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();

                        if (stopwatch.Elapsed.TotalSeconds > 10)
                        {
                            throw new Exception("Connection is closed.");
                        }
                    }
                }

                return sqlConnection;
            }

            private set
            {
                sqlConnection = value;
            }
        }

        private bool Connecting = false;

        private void TryConnect()
        {
            if (!Connecting && sqlConnection.State != ConnectionState.Open)
            {
                Connecting = true;

                LogMessage("Connecting...");

                try
                {
                    sqlConnection.Open();

                    LogMessage("Connect Sucsessful...");
                }
                catch
                {
                    LogMessage("Connect Error...");
                }

                Connecting = false;
            }
        }

        private bool CheckConnection()
        {
            using SqlCommand command = CreateCommand("SELECT 1");

            try
            {
                command.ExecuteScalar();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public SqlClient(string ServerName, string DataBaseName, string UserId, string UserPassword)
        {
            string SqlConnectionString = $"Server={ServerName};Database={DataBaseName};User Id={UserId};Password={UserPassword};";

            SqlConnection = new SqlConnection(SqlConnectionString);
        }

        public SqlClient(string SqlConnectionString)
        {
            SqlConnection = new SqlConnection(SqlConnectionString);
        }

        public TableManager CreateTableManager(string TableName)
        {
            TableManager table = new(this, TableName);

            return table;
        }

        public SqlCommand CreateCommand(string CommandQuery)
        {
            SqlCommand sqlCommand = new(CommandQuery, SqlConnection);

            return sqlCommand;
        }

        public bool TableExists(string TableName)
        {
            string query = $"IF OBJECT_ID(N'dbo.{TableName}', N'U') IS NOT NULL SELECT 1 ELSE SELECT 0";

            using SqlCommand command = CreateCommand(query);

            return (int)command.ExecuteScalar() == 1;
        }

        public event LogEvent OnLogMessage;

        protected void LogMessage(string message)
        {
            //DateTime dateTime = new DateTime();

            OnLogMessage?.Invoke(message);
        }

        public void Dispose()
        {
            sqlConnection?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}