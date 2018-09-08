using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mssql_exporter.server
{
    public static class CounterExtensions
    {
        public static void Set(this Prometheus.Counter counter, double value)
        {
            if(counter.Value < value)
            {
                counter.Inc(value - counter.Value);
            }
        }

        public static void Set(this Prometheus.Counter.Child counter, double value)
        {
            if (counter.Value < value)
            {
                counter.Inc(value - counter.Value);
            }
        }
    }
}
