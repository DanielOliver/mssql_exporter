using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Prometheus.Advanced;

namespace mssql_exporter.core.queries
{
    class GaugeGroupQuery : IQuery
    {

        private readonly IEnumerable<Column> _labelColumns;
        private readonly Prometheus.Gauge _gauge;
        private readonly string _description;
        private readonly Column _valueColumn;

        public GaugeGroupQuery(string name, string description, string query, IEnumerable<Column> labelColumns, Column valueColumn, MetricFactory metricFactory, int? millisecondTimeout)
        {
            Name = name;
            this._description = description;
            Query = query;
            MillisecondTimeout = millisecondTimeout;
            this._valueColumn = valueColumn;
            this._labelColumns = labelColumns.OrderBy(x => x.Order).ToArray();
            _gauge = metricFactory.CreateGauge(name, description, new Prometheus.GaugeConfiguration
            {
                LabelNames = this._labelColumns.Select(x => x.Label).ToArray(),
                SuppressInitialValue = true
            });
        }

        public string Name { get; }
        public string Query { get; }
        public int? MillisecondTimeout { get; }

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
                    _gauge.WithLabels(labels).Set(result);
                }
            }
        }


        public void Clear()
        {
            // TODO: What should I do here?
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
}
