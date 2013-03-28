namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    using Dapper;

    /// <summary>
    /// Dapper data context.
    /// </summary>
    public class DapperContext : IDisposable
    {
        /// <summary>
        /// The parameter prefix
        /// </summary>
        private static string parameterPrefix = "@";

        /// <summary>
        /// The has initialized
        /// </summary>
        private static bool hasInitialized = false;

        /// <summary>
        /// The image type
        /// </summary>
        private static string imageType = "blob";

        /// <summary>
        /// The date time type
        /// </summary>
        private static string dateTimeType = "timestamp";

        /// <summary>
        /// The char type
        /// </summary>
        private static string charType = "varchar";

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperContext"/> class.
        /// </summary>
        public DapperContext()
            : this("DefaultConnection")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DapperContext"/> class.
        /// </summary>
        /// <param name="connectionStringName">
        /// Name of the connection string.
        /// </param>
        public DapperContext(string connectionStringName)
        {
            var cstring = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cstring == null)
            {
                throw new ArgumentException("Connection does not exist: " + connectionStringName);
            }

            var dbFactory = DbProviderFactories.GetFactory(cstring.ProviderName);
            
            this.Connection = dbFactory.CreateConnection();
            this.Connection.ConnectionString = cstring.ConnectionString;

            if (!hasInitialized)
            {
                hasInitialized = true;
                var connectionType = this.Connection.GetType().FullName.ToLowerInvariant();
                if (connectionType.Contains("pgsql") || connectionType.Contains("postgres"))
                {
                    imageType = "bytea";
                    dateTimeType = "timestamp";
                    parameterPrefix = ":";
                    charType = "varchar";
                }
                else if (connectionType.Contains("oracle"))
                {
                    imageType = "blob";
                    dateTimeType = "datetime";
                    parameterPrefix = ":";
                    charType = "nvarchar2";
                }
                else if (connectionType.Contains("mysql"))
                {
                    imageType = "blob";
                    dateTimeType = "datetime";
                    parameterPrefix = "?";
                    charType = "nvarchar";
                }
                else if (connectionType.Contains("sqlclient") || connectionType.Contains("sqlce"))
                {
                    imageType = "image";
                    dateTimeType = "datetime";
                    parameterPrefix = "@";
                    charType = "nvarchar";
                }
            }

            this.Connection.Open();
        }

        /// <summary>
        /// Queries the specified SQL.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Query result.</returns>
        public IEnumerable<T> Query<T>(string sql, object parameter)
        {
            var sql2 = sql.Replace("@", parameterPrefix);
            return this.Connection.Query<T>(sql2, parameter);
        }

        /// <summary>
        /// Executes the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameter">The parameter.</param>
        public void Execute(string sql, object parameter = null)
        {
            var sql2 = sql.Replace("@", parameterPrefix).Replace("bytea", imageType).Replace("timestamp", dateTimeType).Replace("varchar", charType);
            this.Connection.Execute(sql2, parameter);
        }


        /// <summary>
        /// Gets the wrapped connection.
        /// </summary>
        /// <value>
        /// The connection.
        /// </value>
        public IDbConnection Connection
        {
            get;
            private set;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.Connection.State == ConnectionState.Open)
            {
                this.Connection.Close();
            }
        }
    }
}
