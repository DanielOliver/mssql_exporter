using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using mssql_exporter.core;
using mssql_exporter.core.config;
using Prometheus;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace mssql_exporter.server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0].Equals("serve", StringComparison.CurrentCulture))
            {
                RunWebServer(args.Skip(1).ToArray());
            }
            else
            {
                Help();
            }
        }

        /// <summary>
        /// dotnet run -- serve -ConfigFile "../../test.json" -DataSource "Server=tcp:{ YOUR DATABASE HERE },1433;Initial Catalog={ YOUR INITIAL CATALOG HERE };Persist Security Info=False;User ID={ USER ID HERE };Password={ PASSWORD HERE };MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" -LogLevel Debug
        /// </summary>
        public static void Help()
        {
            Console.WriteLine("Commands");
            Console.WriteLine("   help");
            Console.WriteLine("   serve");
            Console.WriteLine("      -DataSource (Connection String)");
            Console.WriteLine("      -ConfigFile (metrics.json)");
            Console.WriteLine("      -ServerPath (/metrics)");
            Console.WriteLine("      -ServerPort (80)");
            Console.WriteLine("      -AddExporterMetrics (false)");
            Console.WriteLine("      -ConfigText ()");
            Console.WriteLine(string.Empty);
            Console.WriteLine("Or environment variables:");
            Console.WriteLine("      PROMETHEUS_MSSQL_DataSource");
            Console.WriteLine("      PROMETHEUS_MSSQL_ConfigFile");
            Console.WriteLine("      PROMETHEUS_MSSQL_ServerPath");
            Console.WriteLine("      PROMETHEUS_MSSQL_ServerPort");
            Console.WriteLine("      PROMETHEUS_MSSQL_AddExporterMetrics");
            Console.WriteLine("      PROMETHEUS_MSSQL_ConfigText");
        }

        public static void RunWebServer(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
            {
                {"-DataSource", "DataSource"},
                {"-ConfigFile", "ConfigFile"},
                {"-ServerPath", "ServerPath"},
                {"-ServerPort", "ServerPort"},
                {"-AddExporterMetrics", "AddExporterMetrics"},
                {"-LogLevel", "LogLevel"},
                {"-ConfigText", "ConfigText"}
            };

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, false)
                .AddEnvironmentVariables("PROMETHEUS_MSSQL_")
                .AddCommandLine(args, switchMappings)
                .Build();
            IConfigure configurationBinding = new ConfigurationOptions();
            config.Bind(configurationBinding);

            var loggingLevel = LogEventLevel.Error;
            if (!string.IsNullOrWhiteSpace(configurationBinding.LogLevel) &&
                Enum.TryParse(configurationBinding.LogLevel, true, out LogEventLevel result))
            {
                loggingLevel = result;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Is(loggingLevel)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("ServerPath {ServerPath}; ServerPort {ServerPort}; AddExporterMetrics {AddExporterMetrics}; LogLevel {LogLevel}", configurationBinding.ServerPath, configurationBinding.ServerPort, configurationBinding.AddExporterMetrics, configurationBinding.LogLevel);
            if (string.IsNullOrWhiteSpace(configurationBinding.DataSource))
            {
                Log.Logger.Error("Expected DataSource: SQL Server connectionString");
                return;
            }

            MetricFile metricFile;
            if (string.IsNullOrWhiteSpace(configurationBinding.ConfigText))
            {
                var filePath = configurationBinding.ConfigFile;
                try
                {
                    var fileText = System.IO.File.ReadAllText(filePath);
                    Log.Logger.Information("Reading ConfigText {ConfigText} from {FileName}", fileText, filePath);
                    metricFile = Parser.FromJson(fileText);
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Failed to read and parse text from {FileName}", filePath);
                    throw;
                }
            }
            else
            {
                try
                {
                    Log.Logger.Information("Parsing ConfigText {ConfigText}", configurationBinding.ConfigText);
                    metricFile = Parser.FromJson(configurationBinding.ConfigText);
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Failed to parse text from ConfigText");
                    throw;
                }
            }

            var registry = (configurationBinding.AddExporterMetrics)
                ? Metrics.DefaultRegistry
                : new CollectorRegistry();

            var collector = ConfigurePrometheus(configurationBinding, Log.Logger, metricFile, registry);

            CreateWebHostBuilder(args, configurationBinding, registry).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfigure configurationBinding,
            CollectorRegistry registry)
        {
            var defaultPath =
                "/" + configurationBinding.ServerPath.Replace("/", string.Empty,
                    StringComparison.CurrentCultureIgnoreCase);
            if (defaultPath.Equals("/", StringComparison.CurrentCultureIgnoreCase))
            {
                defaultPath = string.Empty;
            }

            return WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .Configure(app => app.UseMetricServer(defaultPath, registry))
                .UseUrls($"http://*:{configurationBinding.ServerPort}");
        }

        public static IEnumerable<IQuery> ConfigureMetrics(core.config.MetricFile metricFile,
            MetricFactory metricFactory, ILogger logger)
        {
            return metricFile.Queries.Select(x => MetricQueryFactory.GetSpecificQuery(metricFactory, x, logger));
        }

        public static OnDemandCollector ConfigurePrometheus(IConfigure configure, ILogger logger,
            core.config.MetricFile metricFile, CollectorRegistry registry)
        {
            return new OnDemandCollector(
                configure.DataSource,
                metricFile.MillisecondTimeout,
                logger,
                registry,
                metricFactory => ConfigureMetrics(metricFile, metricFactory, logger));
        }
    }
}