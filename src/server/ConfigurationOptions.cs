using mssql_exporter.core;

namespace mssql_exporter.server
{
    public class ConfigurationOptions : IConfigure
    {
        public string DataSource { get; set; }

        public string ConfigFile { get; set; } = "metrics.json";

        public string ServerPath { get; set; } = "metrics";

        public int ServerPort { get; set; } = 80;

        public bool AddExporterMetrics { get; set; } = false;
    }
}
