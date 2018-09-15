using mssql_exporter.core;
using Prometheus.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mssql_exporter.core.config;
using mssql_exporter.core.metrics;
using Prometheus;

namespace mssql_exporter.server
{
    public class OnDemandCollector : IOnDemandCollector
    {
        public OnDemandCollector(string sqlConnectionString, int millisecondTimeout, Func<MetricFactory, IEnumerable<IQuery>> configureAction)
        {
            this._sqlConnectionString = sqlConnectionString;
            this._millisecondTimeout = millisecondTimeout;
            this._configureAction = configureAction;
        }

        private MetricFactory _metricFactory;
        private IQuery[] _metrics;
        private readonly string _sqlConnectionString;
        private readonly int _millisecondTimeout;
        private readonly Func<MetricFactory, IEnumerable<IQuery>> _configureAction;
        private Gauge _exceptionsGauge;
        private Gauge _timeoutGauge;

        public void RegisterMetrics(ICollectorRegistry registry)
        {
            _metricFactory = Prometheus.Metrics.WithCustomRegistry(registry);
            _metrics = 
                _configureAction(_metricFactory)
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
