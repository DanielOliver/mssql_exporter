using mssql_exporter.core;

namespace mssql_exporter.server
{
    class ConfigurationOptions : IConfigure
    {
        public string DatabaseConnectionString { get; set; }
        public string MetricsConfigurationFile { get; set; } = "metrics.json";
        public string ServerPath { get; set; } = "metrics";
        public int ServerPort { get; set; } = 80;
    }
}
