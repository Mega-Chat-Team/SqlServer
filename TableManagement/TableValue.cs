using Microsoft.Data.SqlClient;

namespace SqlServer.TableManagement
{
    public class TableValue
    {
        public string ColumnName { get; private set; }
        public object Value { get; private set; }

        public TableValue(string columnName, object value)
        {
            ColumnName = columnName;
            Value = value;
        }

        public static TableCondition operator |(TableValue left, TableValue right)
        {
            string ConditionString = $"{left.ColumnName} = @value_1 OR {right.ColumnName} = @value_2";

            SqlParameter[] sqlParameters = [new SqlParameter("@value_1", left.Value), new SqlParameter("@value_2", right.Value)];

            return new TableCondition(ConditionString, [.. sqlParameters]);
        }

        public static TableCondition operator &(TableValue left, TableValue right)
        {
            string ConditionString = $"{left.ColumnName} = @value_1 AND {right.ColumnName} = @value_2";

            SqlParameter[] sqlParameters = [new("@value_1", left.Value), new SqlParameter("@value_2", right.Value)];

            return new TableCondition(ConditionString, [.. sqlParameters]);
        }

        public static implicit operator TableCondition(TableValue tableValue)
        {
            string SqlConditionString = $"{tableValue.ColumnName} = @value_1";

            List<SqlParameter> SqlParameters = [new("@value_1", tableValue.Value)];

            return new TableCondition(SqlConditionString, SqlParameters);
        }

        public override string ToString()
        {
            return $"Column: {ColumnName}, Value: {Value}";
        }
    }
}