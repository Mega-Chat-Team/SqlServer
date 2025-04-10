using Microsoft.Data.SqlClient;

namespace SqlServer.TableManagement
{
    public class TableCondition
    {
        public string SqlConditionString { get; set; }

        public List<SqlParameter> SqlParameters { get; set; }

        public TableCondition(string conditionString, List<SqlParameter> sqlParameters)
        {
            SqlConditionString = conditionString;

            SqlParameters = sqlParameters;
        }

        public TableCondition(string ColumnName, object Value)
        {
            SqlConditionString = $"{ColumnName} = @value_1";

            SqlParameters = [new("@value_1", Value)];
        }

        public static TableCondition operator |(TableCondition left, TableCondition right)
        {
            //Берём "левое" условие за основу, а второе преобразуем. Наша задача - убрать все совпадения названий параметров.
            string leftConditionString = left.SqlConditionString;
            string rightConditionString = right.SqlConditionString;

            //За основу берём параметры "левого" условия.
            List<SqlParameter> sqlParameters = left.SqlParameters;

            //Крутим параметры "правого" условия.
            foreach (SqlParameter rightSqlParameter in right.SqlParameters)
            {
                string[] parametersNames = [.. sqlParameters.Select(x => x.ParameterName)];

                //Проверяем есть ли совпадение.
                if (parametersNames.Contains(rightSqlParameter.ParameterName))
                {
                    //Берём последную цифру в названиях параметров и увеличиваем её на 1.
                    int newNumber = sqlParameters.Max(x => int.Parse(x.ParameterName.Split('_')[1])) + 1;

                    SqlParameter sqlParameter = new($"@value_{newNumber}", rightSqlParameter.Value);

                    sqlParameters.Add(sqlParameter);

                    rightConditionString = rightConditionString.Replace(rightSqlParameter.ParameterName, $"@value_{newNumber}");
                }
            }

            //Делаем итоговое условие, учитывая оператор OR.
            return new TableCondition($"({leftConditionString}) OR ({rightConditionString})", sqlParameters);
        }

        public static TableCondition operator &(TableCondition left, TableCondition right)
        {
            //Берём "левое" условие за основу, а второе преобразуем. Наша задача - убрать все совпадения названий параметров.
            string leftConditionString = left.SqlConditionString;
            string rightConditionString = right.SqlConditionString;

            //За основу берём параметры "левого" условия.
            List<SqlParameter> sqlParameters = left.SqlParameters;

            //Крутим параметры "правого" условия.
            foreach (SqlParameter rightSqlParameter in right.SqlParameters)
            {
                string[] parametersNames = [.. sqlParameters.Select(x => x.ParameterName)];

                //Проверяем есть ли совпадение.
                if (parametersNames.Contains(rightSqlParameter.ParameterName))
                {
                    //Берём последную цифру в названиях параметров и увеличиваем её на 1.
                    int newNumber = sqlParameters.Max(x => int.Parse(x.ParameterName.Split('_')[1])) + 1;

                    SqlParameter sqlParameter = new($"@value_{newNumber}", rightSqlParameter.Value);

                    sqlParameters.Add(sqlParameter);

                    rightConditionString = rightConditionString.Replace(rightSqlParameter.ParameterName, $"@value_{newNumber}");
                }
            }

            //Делаем итоговое условие, учитывая оператор AND.
            return new TableCondition($"({leftConditionString}) AND ({rightConditionString})", sqlParameters);
        }

        public override string ToString()
        {
            string conditionString = SqlConditionString;

            foreach (SqlParameter sqlParameter in SqlParameters)
            {
                conditionString = conditionString.Replace(sqlParameter.ParameterName, sqlParameter.Value.ToString());
            }

            return conditionString;
        }
    }
}