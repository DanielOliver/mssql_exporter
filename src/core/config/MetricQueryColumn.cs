namespace mssql_exporter.core.config
{
    public class MetricQueryColumn
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Usage { get; set; }

        public string Label { get; set; }

        public int? Order { get; set; }

        public ColumnUsage ColumnUsage => Constants.GetColumnUsage(Usage) ?? ColumnUsage.Empty;

        public decimal? DefaultValue { get; set; }
    }
}
