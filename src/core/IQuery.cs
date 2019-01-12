using System.Data;

namespace mssql_exporter.core
{
    public interface IQuery
    {
        /// <summary>
        /// Given a database connection, gets metrics.
        /// </summary>
        /// <param name="connection"></param>
        void Measure(DataSet dataSet);

        /// <summary>
        /// Called if a timeout or exception occurs.
        /// </summary>
        void Clear();

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
    }
}
