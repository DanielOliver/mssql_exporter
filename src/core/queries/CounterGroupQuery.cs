using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Prometheus;

namespace mssql_exporter.core.queries
{
#pragma warning disable CA1034 // Nested types should not be visible
    public class CounterGroupQuery : IQuery
    {
        private readonly IEnumerable<Column> _labelColumns;
        private readonly Prometheus.Counter _counter;
        private readonly string _description;
        private readonly Column _valueColumn;

        public CounterGroupQuery(string name, string description, string query, IEnumerable<Column> labelColumns, Column valueColumn, MetricFactory metricFactory, int? millisecondTimeout)
        {
            Name = name;
            this._description = description;
            Query = query;
            this._valueColumn = valueColumn;
            MillisecondTimeout = millisecondTimeout;
            this._labelColumns = labelColumns.OrderBy(x => x.Order).ToArray();

            var counterConfiguration = new Prometheus.CounterConfiguration
            {
                LabelNames = this._labelColumns.Select(x => x.Label).ToArray()
            };

            _counter = metricFactory.CreateCounter(name, description, counterConfiguration);
        }

        public string Name { get; }

        public string Query { get; }

        public int? MillisecondTimeout { get; }

        public void Clear()
        {
            // TODO: What should I do here?
        }

        public void Measure(DataSet dataSet)
        {
            var table = dataSet.Tables[0];

            var columnIndices = _labelColumns.Select(x => QueryExtensions.GetColumnIndex(table, x.Name));
            var valueIndex = QueryExtensions.GetColumnIndex(table, _valueColumn.Name);

            foreach (var row in table.Rows.Cast<DataRow>())
            {
                var labels = columnIndices.Select(x => row.ItemArray[x].ToString().Trim()).ToArray();
                if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                {
                    _counter.WithLabels(labels).Set(result);
                }
            }
        }

        public class Column
        {
            public Column(string name, int order, string label)
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
                Order = order;
                Label = label;
            }

            public string Name { get; }

            public int Order { get; }

            public string Label { get; }
        }
    }
#pragma warning restore CA1034
}
