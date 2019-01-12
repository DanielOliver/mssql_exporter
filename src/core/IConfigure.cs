namespace mssql_exporter.core
{
    public interface IConfigure
    {
        /// <summary>
        /// Database Connection String
        /// </summary>
        string DataSource { get; set; }

        /// <summary>
        /// Path to the file containing metric configuration.
        /// </summary>
        string ConfigFile { get; set; }

        /// <summary>
        /// Default: "/metrics"
        /// </summary>
        /// <example>metrics</example>
        string ServerPath { get; set; }

        /// <summary>
        /// Default: "80"
        /// </summary>
        /// <example>80</example>
        int ServerPort { get; set; }

        /// <summary>
        /// If true, adds default Prometheus Exporter metrics.
        /// </summary>
        /// <example>false</example>
        bool AddExporterMetrics { get; set; }
    }
}
