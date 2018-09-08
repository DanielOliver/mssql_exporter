using System;
using System.Collections.Generic;
using System.Text;

namespace mssql_exporter.core
{
    public interface IConfigure
    {
        /// <summary>
        /// Database Connection String
        /// </summary>
        string DatabaseConnectionString { get; set; }
        /// <summary>
        /// Path to the file containing metric configuration.
        /// </summary>
        string MetricsConfigurationFile { get; set; }
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
    }
}
