using mssql_exporter.core.queries;
using Prometheus;

namespace mssql_exporter.core.metrics
{
    public class ConnectionUp : GenericQuery
    {
        public ConnectionUp(MetricFactory metricFactory)
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            : base("mssql_up", "SELECT 1 mssql_up", new[] { new GaugeColumn("mssql_up", "mssql_up", "mssql_up", metricFactory, 0) }, new CounterColumn[] { }, null)
#pragma warning restore CA1825 // Avoid zero-length array allocations.
        {
        }
    }
}
