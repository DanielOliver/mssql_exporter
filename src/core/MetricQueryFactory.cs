﻿using System;
using System.Linq;
using mssql_exporter.core.config;
using mssql_exporter.core.queries;

namespace mssql_exporter.core
{
    public class MetricQueryFactory
    {
        public static IQuery GetSpecificQuery(Prometheus.Advanced.MetricFactory metricFactory, MetricQuery metricQuery)
        {
            switch (metricQuery.QueryUsage)
            {
                case QueryUsage.Counter:
                    var labelColumns =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.CounterLabel)
                        .Select(x => new CounterGroupQuery.Column(x.Name, x.Order ?? 0, x.Label));

                    var valueColumn =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.Counter)
                        .Select(x => new CounterGroupQuery.Column(x.Name, x.Order ?? 0, x.Label))
                        .FirstOrDefault();

                    return new CounterGroupQuery(metricQuery.Name, metricQuery.Description ?? string.Empty, metricQuery.Query, labelColumns, valueColumn, metricFactory, metricQuery.MillisecondTimeout);

                case QueryUsage.Gauge:
                    var gaugeLabelColumns =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.GaugeLabel)
                        .Select(x => new GaugeGroupQuery.Column(x.Name, x.Order ?? 0, x.Label));

                    var gaugeValueColumn =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.Gauge)
                        .Select(x => new GaugeGroupQuery.Column(x.Name, x.Order ?? 0, x.Label))
                        .FirstOrDefault();

                    return new GaugeGroupQuery(metricQuery.Name, metricQuery.Description ?? string.Empty, metricQuery.Query, gaugeLabelColumns, gaugeValueColumn, metricFactory, metricQuery.MillisecondTimeout);

                case QueryUsage.Empty:
                    var gaugeColumns =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.Gauge)
                        .Select(x => new GenericQuery.GaugeColumn(x.Name, x.Label, x.Description ?? x.Label, metricFactory, x.DefaultValue))
                        .ToArray();


                    var counterColumns =
                        metricQuery.Columns
                        .Where(x => x.ColumnUsage == ColumnUsage.Counter)
                        .Select(x => new GenericQuery.CounterColumn(x.Name, x.Label, x.Description ?? x.Label, metricFactory))
                        .ToArray();

                    return new GenericQuery(metricQuery.Name, metricQuery.Query, gaugeColumns, counterColumns, metricQuery.MillisecondTimeout);

                default:
                    break;
            }
            throw new Exception("Undefined QueryUsage.");
        }
    }
}
