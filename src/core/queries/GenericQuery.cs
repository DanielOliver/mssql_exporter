using System;
using System.Data;
using Prometheus;
using Prometheus.Advanced;

namespace mssql_exporter.core.queries
{
    public class GenericQuery: IQuery
    {
        public GenericQuery(string name, string query, GaugeColumn[] gaugeColumns, CounterColumn[] counterColumns, int? millisecondTimeout)
        {
            Name = name;
            Query = query;
            GaugeColumns = gaugeColumns;
            CounterColumns = counterColumns;
            MillisecondTimeout = millisecondTimeout;
        }

        public string Name { get; }
        public string Query { get; }
        public GaugeColumn[] GaugeColumns { get; }
        public CounterColumn[] CounterColumns { get; }
        public int? MillisecondTimeout { get; }

        public void Measure(DataSet dataSet)
        {
            foreach (var column in GaugeColumns)
            {
                column.Measure(dataSet);
            }
            foreach (var column in CounterColumns)
            {
                column.Measure(dataSet);
            }
        }

        public void Clear()
        {
            foreach (var column in GaugeColumns)
            {
                column.Clear();
            }
        }

        public class GaugeColumn
        {
            public GaugeColumn(string name, string label, string description, MetricFactory metricFactory, decimal? defaultValue = 0)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Expected name argument", nameof(name));
                }
                if (string.IsNullOrWhiteSpace(label))
                {
                    throw new ArgumentException("expected label argument", nameof(label));
                }

                _defaultValue = defaultValue;
                Name = name;
                Label = label;
                _gauge = metricFactory.CreateGauge(label, description, new Prometheus.GaugeConfiguration());
            }

            public void Measure(DataSet dataSet)
            {
                var table = dataSet.Tables[0];
                if (table.Rows.Count >= 0)
                {
                    var row = table.Rows[0];
                    var valueIndex = QueryExtensions.GetColumnIndex(table, Name);
                    if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                    {
                        _gauge.Set(result);
                    }
                }
            }

            public void Clear()
            {
                if(_defaultValue.HasValue)
                    _gauge.Set(Convert.ToDouble(_defaultValue.Value));
            }

            public string Name { get; }
            public string Label { get; }

            private readonly Gauge _gauge;
            private readonly decimal? _defaultValue;
        }
        public class CounterColumn
        {
            public CounterColumn(string name, string label, string description, MetricFactory metricFactory)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Expected name argument", nameof(name));
                }
                if (string.IsNullOrWhiteSpace(label))
                {
                    throw new ArgumentException("expected label argument", nameof(label));
                }
                Name = name;
                Label = label;
                _counter = metricFactory.CreateCounter(label, description, new Prometheus.CounterConfiguration());
            }

            public void Measure(DataSet dataSet)
            {
                var table = dataSet.Tables[0];
                if(table.Rows.Count >= 0)
                {
                    var row = table.Rows[0];
                    var valueIndex = QueryExtensions.GetColumnIndex(table, Name);
                    if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                    {
                        _counter.Set(result);
                    }
                }
            }

            public string Name { get; }
            public string Label { get; }

            private readonly Counter _counter;
        }
    }
}
