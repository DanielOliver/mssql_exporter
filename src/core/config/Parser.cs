using Newtonsoft.Json;

namespace mssql_exporter.core.config
{
    public static class Parser
    {
        public static MetricFile FromJson(string text)
        {
            return JsonConvert.DeserializeObject<MetricFile>(text);
        }
    }
}
