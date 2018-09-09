using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mssql_exporter.core;
using Prometheus;
using Prometheus.Advanced;
using System.Collections.Generic;
using System.Linq;

namespace mssql_exporter.server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length >= 1 && args[0].Equals("serve", System.StringComparison.InvariantCultureIgnoreCase))
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
            System.Console.WriteLine("      -ConfigFile (/srv/metrics/test.json)");
            System.Console.WriteLine("      -ServerPath (/metrics)");
            System.Console.WriteLine("      -ServerPort (80)");
        }

        public static void RunWebServer(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
            {
                { "-DataSource", "DatabaseConnectionString" },
                { "-ConfigFile", "MetricsConfigurationFile" },
                { "-ServerPath", "ServerPath" },
                { "-ServerPort", "ServerPort" }
            };

            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("PROMETHEUS_MSSQL_")
                .AddCommandLine(args, switchMappings)
                .Build();
            IConfigure configurationBinding = new ConfigurationOptions();
            config.Bind(configurationBinding);


            var filePath = configurationBinding.MetricsConfigurationFile;
            var fileText = System.IO.File.ReadAllText(filePath);
            //var fileText = System.IO.File.ReadAllText(@"C:\Users\Daniel\Development\mssql_exporter\test.json");
            var metricFile = core.config.Parser.FromJson(fileText);
            ConfigurePrometheus(configurationBinding, metricFile);
            
            CreateWebHostBuilder(args, configurationBinding).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfigure configurationBinding)
        {
            var defaultPath = "/" + configurationBinding.ServerPath.Replace("/", "");
            if (defaultPath.Equals("/")) defaultPath = string.Empty;

            return WebHost.CreateDefaultBuilder(args)
                .Configure(app => app.UseMetricServer(defaultPath))
                .UseUrls($"http://*:{configurationBinding.ServerPort}");
        }

        public static IEnumerable<IQuery> ConfigureMetrics(core.config.MetricFile metricFile, MetricFactory metricFactory)
        {
            return metricFile.Queries.Select(x => MetricQueryFactory.GetSpecificQuery(metricFactory, x));
        }

        public static void ConfigurePrometheus(IConfigure configure, core.config.MetricFile metricFile)
        {
            // Clear prometheus default metrics.
            Prometheus.Advanced.DefaultCollectorRegistry.Instance.Clear();


            var collector = new OnDemandCollector("Server=tcp:testworld.database.windows.net,1433;Initial Catalog=db1;Persist Security Info=False;User ID=admin234234;Password=JKHKJ*&*(&hjko87;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                x => ConfigureMetrics(metricFile, x));
            Prometheus.Advanced.DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(collector);
        }
    }
}
