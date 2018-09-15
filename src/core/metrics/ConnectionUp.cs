using mssql_exporter.core.queries;
using Prometheus.Advanced;

namespace mssql_exporter.core.metrics
{
    public class ConnectionUp: GenericQuery
    {
        public ConnectionUp(MetricFactory metricFactory)
            : base("mssql_up", "SELECT 1 mssql_up", new[] { new GaugeColumn("mssql_up", "mssql_up", "mssql_up", metricFactory, 0) }, new CounterColumn[] {}, null)
        {
        }
    }
}
