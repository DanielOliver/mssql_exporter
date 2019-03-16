using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using mssql_exporter.core;
using Prometheus;

namespace mssql_exporter.server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0].Equals("serve", System.StringComparison.InvariantCultureIgnoreCase))
            {
                RunWebServer(args.Skip(1).ToArray());
            }
            else
            {
                Help();
            }
        }

        public static void Help()
        {
            System.Console.WriteLine("Commands");
            System.Console.WriteLine("   help");
            System.Console.WriteLine("   serve");
            System.Console.WriteLine("      -DataSource (Connection String)");
            System.Console.WriteLine("      -ConfigFile (metrics.json)");
            System.Console.WriteLine("      -ServerPath (/metrics)");
            System.Console.WriteLine("      -ServerPort (80)");
            System.Console.WriteLine("      -AddExporterMetrics (false)");
            System.Console.WriteLine(string.Empty);
            System.Console.WriteLine("Or environment variables:");
            System.Console.WriteLine("      PROMETHEUS_MSSQL_DataSource");
            System.Console.WriteLine("      PROMETHEUS_MSSQL_ConfigFile");
            System.Console.WriteLine("      PROMETHEUS_MSSQL_ServerPath");
            System.Console.WriteLine("      PROMETHEUS_MSSQL_ServerPort");
            System.Console.WriteLine("      PROMETHEUS_MSSQL_AddExporterMetrics");
        }

        public static void RunWebServer(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
            {
                { "-DataSource", "DataSource" },
                { "-ConfigFile", "ConfigFile" },
                { "-ServerPath", "ServerPath" },
                { "-ServerPort", "ServerPort" },
                { "-AddExporterMetrics", "AddExporterMetrics" }
            };

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, false)
                .AddEnvironmentVariables("PROMETHEUS_MSSQL_")
                .AddCommandLine(args, switchMappings)
                .Build();
            IConfigure configurationBinding = new ConfigurationOptions();
            config.Bind(configurationBinding);

            var filePath = configurationBinding.ConfigFile;
            var fileText = System.IO.File.ReadAllText(filePath);
            var metricFile = core.config.Parser.FromJson(fileText);

            var registry = new CollectorRegistry();
            var collector = ConfigurePrometheus(configurationBinding, metricFile, registry);

            CreateWebHostBuilder(args, configurationBinding, registry).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfigure configurationBinding, CollectorRegistry registry)
        {
            var defaultPath = "/" + configurationBinding.ServerPath.Replace("/", string.Empty, StringComparison.CurrentCultureIgnoreCase);
            if (defaultPath.Equals("/", StringComparison.CurrentCultureIgnoreCase))
            {
                defaultPath = string.Empty;
            }

            return WebHost.CreateDefaultBuilder(args)
                .Configure(app => app.UseMetricServer(defaultPath, registry))
                .UseUrls($"http://*:{configurationBinding.ServerPort}");
        }

        public static IEnumerable<IQuery> ConfigureMetrics(core.config.MetricFile metricFile, MetricFactory metricFactory)
        {
            return metricFile.Queries.Select(x => MetricQueryFactory.GetSpecificQuery(metricFactory, x));
        }

        public static OnDemandCollector ConfigurePrometheus(IConfigure configure, core.config.MetricFile metricFile, CollectorRegistry registry)
        {
            return new OnDemandCollector(
                configure.DataSource,
                metricFile.MillisecondTimeout,
                registry,
                metricFactory => ConfigureMetrics(metricFile, metricFactory)
                );
        }
    }
}
