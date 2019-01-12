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

        public int? MillisecondTimeout { get; set; }
    }
}
