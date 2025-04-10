namespace SqlServer.TableManagement
{
    public class TableRow
    {
        public TableValue[] Values { get; private set; }

        public TableRow(TableValue[] tableValues)
        {
            Values = tableValues;
        }

        public TableRow(TableValue tableValue)
        {
            Values = [tableValue];
        }
    }
}