using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using mssql_exporter.core;
using Prometheus.Advanced;

namespace mssql_exporter.server
{
    public class OnDemandCollector : IOnDemandCollector
    {
        public OnDemandCollector(string sqlConnectionString, Func<MetricFactory, IEnumerable<IQuery>> configureAction)
        {
            this.sqlConnectionString = sqlConnectionString;
            this.configureAction = configureAction;
        }

        private MetricFactory metricFactory;
        private IEnumerable<IQuery> metrics;
        private readonly string sqlConnectionString;
        private readonly Func<MetricFactory, IEnumerable<IQuery>> configureAction;

        public void RegisterMetrics(ICollectorRegistry registry)
        {
            metricFactory = Prometheus.Metrics.WithCustomRegistry(registry);
            metrics = configureAction(metricFactory);
        }

        public void UpdateMetrics()
        {
            using (var sqlConnection = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    sqlConnection.Open();
                }
                catch (Exception e)
                {
                    throw new ScrapeFailedException("Failed to open SQL Connection", e);
                }
                Task.WaitAll(metrics.Select(x => x.MeasureWithConnection(sqlConnection)).ToArray());
            }
        }
    }
}
