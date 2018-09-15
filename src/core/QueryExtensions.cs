using mssql_exporter.core.config;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace mssql_exporter.core
{
    public static class QueryExtensions
    {
        public static async Task<MeasureResult> MeasureWithConnection(this IQuery query, string sqlConnectionString, int defaultMillisecondTimeout)
        {
            var timeout = Math.Min(defaultMillisecondTimeout, query.MillisecondTimeout ?? 100_000_000);
            var tokenSource = new CancellationTokenSource(timeout).Token;

            var measureTask = Task.Run(() =>
            {
                try
                {
                    using (var sqlConnection = new SqlConnection(sqlConnectionString))
                    {
                        sqlConnection.Open();
                        tokenSource.ThrowIfCancellationRequested();

                        using (var dataset = new DataSet())
                        {
                            using (var command = new SqlCommand(query.Query, sqlConnection))
                            {
                                tokenSource.ThrowIfCancellationRequested();
                                using (var adapter = new SqlDataAdapter
                                {
                                    SelectCommand = command
                                })
                                {
                                    adapter.Fill(dataset);
                                    tokenSource.ThrowIfCancellationRequested();

                                    query.Measure(dataset);
                                    return MeasureResult.Success;
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    query.Clear();
                    return MeasureResult.Timeout;
                }
                catch (Exception)
                {
                    try
                    {
                        query.Clear();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    return MeasureResult.Exception;
                }
            }, tokenSource);

            var delayTask = Task.Run(async () =>
            {
                await Task.Delay(timeout);
                return MeasureResult.Timeout;
            });

            return await await Task.WhenAny(delayTask, measureTask);
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
