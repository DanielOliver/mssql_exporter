using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace mssql_exporter.core
{
    public interface IQuery
    {
        /// <summary>
        /// Given a database connection, gets metrics.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        void Measure(DataSet dataSet);

        /// <summary>
        /// The name of this query.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The query that returns results.
        /// </summary>
        string Query { get; }
    }
}
