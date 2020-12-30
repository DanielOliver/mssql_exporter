using mssql_exporter.core;

namespace mssql_exporter.server
{
    public class ConfigurationOptions : IConfigure
    {
        public string DataSource { get; set; }

        public string ConfigFile { get; set; } = "metrics.json";
        
        public string ConfigText { get; set; }

        public string ServerPath { get; set; } = "metrics";

        public int ServerPort { get; set; } = 80;

        public bool AddExporterMetrics { get; set; } = false;

        public string LogLevel { get; set; }

        public string LogFilePath { get; set; } = "mssqlexporter-log.txt";
    }
}
