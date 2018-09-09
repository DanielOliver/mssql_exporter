using Prometheus;
using Prometheus.Advanced;
using System;
using System.Collections.Generic;
using System.Data;

namespace mssql_exporter.core.queries
{
    public class GenericQuery: IQuery
    {
        public GenericQuery(string name, string query, GaugeColumn[] gaugeColumns, CounterColumn[] counterColumns)
        {
            Name = name;
            Query = query;
            GaugeColumns = gaugeColumns;
            CounterColumns = counterColumns;
        }

        public string Name { get; }
        public string Query { get; }
        public GaugeColumn[] GaugeColumns { get; }
        public CounterColumn[] CounterColumns { get; }

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

        public class GaugeColumn
        {
            public GaugeColumn(string name, string label, string description, MetricFactory metricFactory)
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
                gauge = metricFactory.CreateGauge(label, description, new Prometheus.GaugeConfiguration());
            }

            public void Measure(DataSet dataSet)
            {
                var table = dataSet.Tables[0];
                if (table.Rows.Count >= 0)
                {
                    var row = table.Rows[0];
                    var valueIndex = IQueryExtensions.GetColumnIndex(table, Name);
                    if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                    {
                        gauge.Set(result);
                    }
                }
            }

            public string Name { get; }
            public string Label { get; }

            private readonly Gauge gauge;
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
                counter = metricFactory.CreateCounter(label, description, new Prometheus.CounterConfiguration());
            }

            public void Measure(DataSet dataSet)
            {
                var table = dataSet.Tables[0];
                if(table.Rows.Count >= 0)
                {
                    var row = table.Rows[0];
                    var valueIndex = IQueryExtensions.GetColumnIndex(table, Name);
                    if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                    {
                        counter.Set(result);
                    }
                }
            }

            public string Name { get; }
            public string Label { get; }

            private readonly Counter counter;
        }
    }
}
