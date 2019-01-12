using System.Data;

namespace mssql_exporter.core
{
    public interface IQuery
    {
        /// <summary>
        /// The name of this query.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The query that returns results.
        /// </summary>
        string Query { get; }

        /// <summary>
        /// Query timeout in milliseconds.
        /// </summary>
        int? MillisecondTimeout { get; }

        /// <summary>
        /// Given a database dataset, gets metrics.
        /// </summary>
        /// <param name="dataSet">May contain multiple tables</param>
        void Measure(DataSet dataSet);

        /// <summary>
        /// Called if a timeout or exception occurs.
        /// </summary>
        void Clear();
    }
}
