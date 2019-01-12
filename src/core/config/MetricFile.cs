namespace mssql_exporter.core.config
{
    public class MetricFile
    {
        public MetricQuery[] Queries { get; set; }

        public int MillisecondTimeout { get; set; } = 10_000;
    }
}
