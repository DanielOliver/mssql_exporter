using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mssql_exporter.core;
using mssql_exporter.core.config;
using mssql_exporter.core.metrics;
using Prometheus;

namespace mssql_exporter.server
{
    public class OnDemandCollector
    {
        private readonly string _sqlConnectionString;
        private readonly int _millisecondTimeout;
        private readonly CollectorRegistry _registry;
        private readonly Gauge _exceptionsGauge;
        private readonly Gauge _timeoutGauge;
        private readonly MetricFactory _metricFactory;
        private readonly IQuery[] _metrics;

        public OnDemandCollector(string sqlConnectionString, int millisecondTimeout, CollectorRegistry registry, Func<MetricFactory, IEnumerable<IQuery>> configureAction)
        {
            _sqlConnectionString = sqlConnectionString;
            _millisecondTimeout = millisecondTimeout;
            _registry = registry;
            _metricFactory = Metrics.WithCustomRegistry(registry);
            _registry.AddBeforeCollectCallback(UpdateMetrics);
            _metrics =
                configureAction(_metricFactory)
                    .Append(new ConnectionUp(_metricFactory))
                    .ToArray();

            _exceptionsGauge = _metricFactory.CreateGauge("mssql_exceptions", "Number of queries throwing exceptions.");
            _timeoutGauge = _metricFactory.CreateGauge("mssql_timeouts", "Number of queries timing out.");
        }

        public void UpdateMetrics()
        {
            var results = Task.WhenAll(_metrics.Select(x => x.MeasureWithConnection(_sqlConnectionString, _millisecondTimeout)).ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();

            _exceptionsGauge?.Set(results.Count(x => x == MeasureResult.Exception));

            _timeoutGauge?.Set(results.Count(x => x == MeasureResult.Timeout));
        }
    }
}
