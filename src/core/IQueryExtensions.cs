using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace mssql_exporter.core
{
    public static class IQueryExtensions
    {
        public static Task MeasureWithConnection(this IQuery query, SqlConnection connection)
        {
            return Task.Run(() =>
            {
                using (var dataset = new DataSet())
                {
                    using (var command = new SqlCommand(query.Query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter
                        {
                            SelectCommand = command
                        })
                        {
                            adapter.Fill(dataset);

                            query.Measure(dataset);
                        }
                    }
                }
            });
        }

        public static int GetColumnIndex(DataTable dataTable, string columnName)
        {
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(columnName), $"Expected to find column {columnName}");
        }
    }
}
