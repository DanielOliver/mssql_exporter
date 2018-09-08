using mssql_exporter.core.queries;
using System.Linq;

namespace mssql_exporter.core.config
{
    public class MetricQuery
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Query { get; set; }
        public MetricQueryColumn[] Columns { get; set; }
        public string Usage { get; set; }
        public QueryUsage QueryUsage => Constants.GetQueryUsage(this.Usage) ?? QueryUsage.Empty;

        public IQuery GetSpecificQuery(Prometheus.Advanced.MetricFactory metricFactory)
        {
            switch (QueryUsage)
            {
                case QueryUsage.Counter:
                    var labelColumns = Columns.Where(x => x.ColumnUsage == ColumnUsage.CounterLabel).Select(x => new CounterGroupQuery.Column(x.Name, x.Order ?? 0, x.Label));
                    var valueColumn = Columns.Where(x => x.ColumnUsage == ColumnUsage.Counter).Select(x => new CounterGroupQuery.Column(x.Name, x.Order ?? 0, x.Label)).FirstOrDefault();
                    return new CounterGroupQuery(Name, Description ?? string.Empty, Query, labelColumns, valueColumn, metricFactory);

                case QueryUsage.Gauge:
                    var gaugeLabelColumns = Columns.Where(x => x.ColumnUsage == ColumnUsage.GaugeLabel).Select(x => new GaugeGroupQuery.Column(x.Name, x.Order ?? 0, x.Label));
                    var gaugeValueColumn = Columns.Where(x => x.ColumnUsage == ColumnUsage.Gauge).Select(x => new GaugeGroupQuery.Column(x.Name, x.Order ?? 0, x.Label)).FirstOrDefault();
                    return new GaugeGroupQuery(Name, Description ?? string.Empty, Query, gaugeLabelColumns, gaugeValueColumn, metricFactory);

                case QueryUsage.Empty:
                    break;

                default:
                    break;
            }
            throw new System.Exception("Undefined QueryUsage.");
        }
    }
}
