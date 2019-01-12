using System;

namespace mssql_exporter.core.config
{
    public static class Constants
    {
        public const string USAGE_COLUMN_COUNTER = "Counter";
        public const string USAGE_COLUMN_COUNTER_LABEL = "CounterLabel";
        public const string USAGE_COLUMN_GUAGE = "Gauge";
        public const string USAGE_COLUMN_GUAGE_LABEL = "GaugeLabel";

        public const string USAGE_QUERY_COUNTER = "CountersWithLabels";
        public const string USAGE_QUERY_GUAGE = "GaugesWithLabels";

        public static ColumnUsage? GetColumnUsage(string usage)
        {
            if (string.IsNullOrWhiteSpace(usage))
            {
                return ColumnUsage.Empty;
            }

            if (USAGE_COLUMN_COUNTER.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return ColumnUsage.Counter;
            }

            if (USAGE_COLUMN_COUNTER_LABEL.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return ColumnUsage.CounterLabel;
            }

            if (USAGE_COLUMN_GUAGE.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return ColumnUsage.Gauge;
            }

            if (USAGE_COLUMN_GUAGE_LABEL.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return ColumnUsage.GaugeLabel;
            }

            return null;
        }

        public static QueryUsage? GetQueryUsage(string usage)
        {
            if (USAGE_QUERY_COUNTER.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return QueryUsage.Counter;
            }

            if (USAGE_QUERY_GUAGE.Equals(usage, StringComparison.InvariantCultureIgnoreCase))
            {
                return QueryUsage.Gauge;
            }

            return null;
        }
    }
}
