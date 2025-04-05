using System.Data.SqlClient;
using MicrosoftSqlServer.Client;

namespace MicrosoftSqlServer.TableManagement
{
    public class TableManager
    {
        private readonly SqlClient sqlClient;

        private readonly string tableName;

        public TableManager(SqlClient Client, string TableName)
        {
            if (!Client.TableExists(TableName))
            {
                throw new Exception("Table not found.");
            }

            sqlClient = Client;

            tableName = TableName;
        }

        public TableValue GetValue(string ColumnName, TableCondition Condition)
        {
            string query = $"SELECT {ColumnName} FROM {tableName} WHERE {Condition.SqlConditionString}";

            using (SqlCommand command = sqlClient.CreateCommand(query))
            {
                command.Parameters.AddRange(Condition.SqlParameters.ToArray());

                if (command.ExecuteScalar() != null)
                {
                    return new TableValue(ColumnName, command.ExecuteScalar());
                }

                return null;
            }
        }

        public TableValue[] GetValues(string ColumnName, TableCondition Condition)
        {
            string query = $"SELECT {ColumnName} FROM {tableName} WHERE {Condition.SqlConditionString}";

            using (SqlCommand command = sqlClient.CreateCommand(query))
            {
                command.Parameters.AddRange(Condition.SqlParameters.ToArray());

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<TableValue> Values = new List<TableValue>();

                    while (reader.Read())
                    {
                        TableValue Value = new TableValue(ColumnName, reader.GetValue(0));

                        Values.Add(Value);
                    }

                    return Values.ToArray();
                }
            }
        }

        public void SetValue(string ColumnName, TableCondition Condition, object Value)
        {
            string query = $"UPDATE {tableName} SET {ColumnName} = @value WHERE {Condition.SqlConditionString}";

            using (SqlCommand command = sqlClient.CreateCommand(query))
            {
                command.Parameters.AddWithValue($"@value", Value);

                command.Parameters.AddRange(Condition.SqlParameters.ToArray());

                command.ExecuteNonQuery();
            }
        }

        public void AddRow(TableRow Row)
        {
            TableValue[] RowValues = Row.Values;

            string columns = RowValues[0].ColumnName;
            string values = "@value_0";

            for (int i = 1; i < RowValues.Length; i++)
            {
                columns += $", {RowValues[i].ColumnName}";
                values += $", @value_{i}";
            }

            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            using (SqlCommand command = sqlClient.CreateCommand(query))
            {
                for (int i = 0; i < RowValues.Length; i++)
                {
                    command.Parameters.AddWithValue($"@value_{i}", RowValues[i].Value);
                }

                command.ExecuteNonQuery();
            }
        }

        public void DeleteRow(TableCondition Condition)
        {
            string query = $"DELETE FROM {tableName} WHERE {Condition.SqlConditionString}";

            using (SqlCommand command = sqlClient.CreateCommand(query))
            {
                command.Parameters.AddRange(Condition.SqlParameters.ToArray());

                command.ExecuteNonQuery();
            }
        }
    }
}