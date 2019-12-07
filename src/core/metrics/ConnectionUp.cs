using mssql_exporter.core.queries;
using Prometheus;
using Serilog;

namespace mssql_exporter.core.metrics
{
    public class ConnectionUp : GenericQuery
    {
        private readonly ILogger _logger;

        public ConnectionUp(MetricFactory metricFactory, ILogger logger)
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            : base("mssql_up", "SELECT 1 mssql_up", new[] { new GaugeColumn("mssql_up", "mssql_up", "mssql_up", metricFactory, 0) }, new CounterColumn[] { }, logger, null)
#pragma warning restore CA1825 // Avoid zero-length array allocations.
        {
            _logger = logger;
        }
    }
}
