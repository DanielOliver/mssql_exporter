using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mssql_exporter.core;
using Prometheus;
using System.Collections.Generic;

namespace mssql_exporter.server
{
    public class Program
    {
        public static void Main(string[] args)
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
            var parserResults = core.config.Parser.FromJson(fileText);
            foreach (var item in parserResults.Queries)
            {
                System.Console.WriteLine($"{item.Name} - {item.QueryUsage} - {item.Query}");
            }


            // Clear prometheus default metrics.
            Prometheus.Advanced.DefaultCollectorRegistry.Instance.Clear();

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
    }
}
