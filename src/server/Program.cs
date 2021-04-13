using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using mssql_exporter.core;
using mssql_exporter.core.config;
using Prometheus;
using Serilog;

namespace mssql_exporter.server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if ((args.Length >= 1 && args[0].Equals("serve", StringComparison.CurrentCulture)) || WindowsServiceHelpers.IsWindowsService())
            {
                RunWebServer(args.Where(a => !string.Equals("serve", a, StringComparison.InvariantCultureIgnoreCase)).ToArray());
            }
            else
            {
                Help();
            }
        }

        /// <summary>
        /// dotnet run -- serve -ConfigFile "../../test.json" -DataSource "Server=tcp:{ YOUR DATABASE HERE },1433;Initial Catalog={ YOUR INITIAL CATALOG HERE };Persist Security Info=False;User ID={ USER ID HERE };Password={ PASSWORD HERE };MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
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
            Console.WriteLine("      PROMETHEUS_MSSQL_Serilog__MinimumLevel");
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
                {"-ConfigText", "ConfigText"}
            };

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, false)
                .AddJsonFile("appsettings.json", true, false)
                .AddEnvironmentVariables("PROMETHEUS_MSSQL_")
                .AddCommandLine(args, switchMappings)
                .Build();
            IConfigure configurationBinding = new ConfigurationOptions();
            config.Bind(configurationBinding);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .Enrich.FromLogContext()
                //Logging setup above is a default in case load from configuration doesn't override.
                .ReadFrom.Configuration(config)
                .CreateLogger();

            Log.Logger.Information("ServerPath {ServerPath}; ServerPort {ServerPort}; AddExporterMetrics {AddExporterMetrics}", configurationBinding.ServerPath, configurationBinding.ServerPort, configurationBinding.AddExporterMetrics);
            if (string.IsNullOrWhiteSpace(configurationBinding.DataSource))
            {
                Log.Logger.Error("Expected DataSource: SQL Server connectionString");
                return;
            }

            MetricFile metricFile;
            if (string.IsNullOrWhiteSpace(configurationBinding.ConfigText))
            {
                var filePath = TryGetAbsolutePath(configurationBinding.ConfigFile);
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

            CreateHostBuilder(args, configurationBinding, registry).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfigure configurationBinding,
            CollectorRegistry registry)
        {
            var defaultPath =
                "/" + configurationBinding.ServerPath.Replace("/", string.Empty,
                    StringComparison.CurrentCultureIgnoreCase);
            if (defaultPath.Equals("/", StringComparison.CurrentCultureIgnoreCase))
            {
                defaultPath = string.Empty;
            }

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder
                    .Configure(applicationBuilder => applicationBuilder.UseMetricServer(defaultPath, registry))
                    .UseUrls($"http://*:{configurationBinding.ServerPort}"))
                .UseWindowsService()
                .UseSerilog();
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

        private static string TryGetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (Path.IsPathFullyQualified(path))
            {
                return path;
            }

            return Path.Combine(AppContext.BaseDirectory, path);
        }
    }
}