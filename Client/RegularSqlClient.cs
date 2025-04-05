using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Timers;
using MicrosoftSqlServer.TableManagement;
using Timer = System.Timers.Timer;

namespace MicrosoftSqlServer.Client
{
    public class RegularSqlClient : SqlClient
    {
        private SqlConnection sqlConnection;

        public override SqlConnection SqlConnection
        {
            get
            {
                //Проверяем статус подключения.
                if (LastCheckConnection.Elapsed.TotalSeconds > ConnectionCheckSeconds && !Connecting)
                {
                    bool connect = CheckConnection();

                    //Если подключение оборвано, то подключаемся.
                    if (!connect)
                    {
                        Connect();
                    }
                }

                //Ждём подключения если оно оборвано.
                if (sqlConnection.State != ConnectionState.Open)
                {
                    WaitConnection(ConnectionWaitSeconds);
                }

                return sqlConnection;
            }
        }

        //Время последней прверки подлючения
        private readonly Stopwatch LastCheckConnection = new Stopwatch();

        //Промежуток с последней проверки статуса подключения перед новой проверкой
        public int ConnectionCheckSeconds { get; set; } = 1;

        //Время, которое программа будет ожидать до установки соеденения
        public int ConnectionWaitSeconds { get; set; } = 10;

        private readonly Timer AutoConnectTimer = new Timer(100);

        private bool Connecting = false;

        public RegularSqlClient(string ServerName, string DataBaseName, string UserId, string UserPassword)
        {
            AutoConnectTimer.Elapsed += OnAutoConnectTimer;
            AutoConnectTimer.AutoReset = true;

            SwitchConnection(ServerName, DataBaseName, UserId, UserPassword);
        }

        public void SwitchConnection(string ServerName, string DataBaseName, string UserId, string UserPassword)
        {
            SqlConnectionString = $"Server={ServerName};Database={DataBaseName};User Id={UserId};Password={UserPassword};";

            sqlConnection = new SqlConnection(SqlConnectionString);
            sqlConnection.Open();

            LastCheckConnection.Restart();

            if (!AutoConnectTimer.Enabled)
            {
                AutoConnectTimer.Start();
            }
        }

        private void OnAutoConnectTimer(object source, ElapsedEventArgs arg)
        {
            Connect();
        }

        private void Connect()
        {
            if (!Connecting && sqlConnection.State != ConnectionState.Open)
            {
                Connecting = true;

                LogMessage("Connecting");

                try
                {
                    sqlConnection.Open();

                    LogMessage("Connect Sucsessful");
                }
                catch
                {
                    LogMessage("Connect Error");
                }

                Connecting = false;
            }
        }

        private bool CheckConnection()
        {
            using (SqlCommand command = new SqlCommand("SELECT 1", sqlConnection))
            {
                try
                {
                    command.ExecuteScalar();

                    LastCheckConnection.Restart();

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void WaitConnection(int WaitSeconds)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            while (sqlConnection.State != ConnectionState.Open)
            {
                Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();

                //Если время ожидания прошло, то прекращаем ожидание.
                if (stopwatch.Elapsed.TotalSeconds > WaitSeconds)
                {
                    throw new Exception("Connection is closed.");
                }
            }

            stopwatch.Reset();
        }

        public void Dispose()
        {
            AutoConnectTimer?.Dispose();
            sqlConnection?.Dispose();

            LastCheckConnection.Stop();

            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}