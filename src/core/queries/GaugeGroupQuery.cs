using Prometheus.Advanced;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace mssql_exporter.core.queries
{
    class GaugeGroupQuery : IQuery
    {

        private readonly IEnumerable<Column> labelColumns;
        private readonly Prometheus.Gauge gauge;
        private readonly string description;
        private readonly Column valueColumn;

        public GaugeGroupQuery(string name, string description, string query, IEnumerable<Column> labelColumns, Column valueColumn, MetricFactory metricFactory)
        {
            Name = name;
            this.description = description;
            Query = query;
            this.valueColumn = valueColumn;
            this.labelColumns = labelColumns.OrderBy(x => x.Order).ToArray();
            gauge = metricFactory.CreateGauge(name, description, new Prometheus.GaugeConfiguration
            {
                LabelNames = this.labelColumns.Select(x => x.Label).ToArray(),
                SuppressInitialValue = true
            });
        }

        public string Name { get; }
        public string Query { get; }

        public void Measure(DataSet dataSet)
        {
            var table = dataSet.Tables[0];

            var columnIndices = labelColumns.Select(x => IQueryExtensions.GetColumnIndex(table, x.Name));
            var valueIndex = IQueryExtensions.GetColumnIndex(table, valueColumn.Name);

            foreach (var row in table.Rows.Cast<DataRow>())
            {
                var labels = columnIndices.Select(x => row.ItemArray[x].ToString().Trim()).ToArray();
                if (double.TryParse(row.ItemArray[valueIndex].ToString(), out double result))
                {
                    gauge.WithLabels(labels).Set(result);
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
}
