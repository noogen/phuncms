namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;

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
        /// The long data type
        /// </summary>
        private static string longDataType = "bigint";

        /// <summary>
        /// The integer data type
        /// </summary>  
        private static string intDataType = "integer";

        /// <summary>
        /// The identity replace
        /// </summary>
        private static string identityReplace = "serial";

        /// <summary>
        /// The connection type
        /// </summary>
        private string connectionType;


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
                this.connectionType = this.Connection.GetType().FullName.ToLowerInvariant();
                if (this.connectionType.Contains("pgsql") || this.connectionType.Contains("postgres"))
                {
                    imageType = "bytea";
                    dateTimeType = "timestamp";
                    parameterPrefix = ":";
                    charType = "varchar";
                    longDataType = "bigint";
                    intDataType = "integer";
                    identityReplace = "serial";
                }
                else if (this.connectionType.Contains("oracle"))
                {
                    imageType = "blob";
                    dateTimeType = "timestamp";
                    parameterPrefix = ":";
                    charType = "nvarchar2";
                    longDataType = "number(19)";
                    intDataType = "number(9)";
                    identityReplace = "number(9)";
                }
                else if (this.connectionType.Contains("mysql"))
                {
                    imageType = "blob";
                    dateTimeType = "datetime";
                    parameterPrefix = "?";
                    charType = "nvarchar";
                    longDataType = "bigint";
                    intDataType = "integer";
                    identityReplace = "integer auto_increment";
                }
                else if (this.connectionType.Contains("sqlclient") || this.connectionType.Contains("sqlce"))
                {
                    imageType = "image";
                    dateTimeType = "datetime";
                    parameterPrefix = "@";
                    charType = "nvarchar";
                    longDataType = "bigint";
                    intDataType = "integer";
                    identityReplace = "integer identity(1,1)";
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
        public void ExecuteSchema(string sql, object parameter = null)
        {
            var sql2 = sql.Trim().Replace("@", parameterPrefix)
                    .Replace("bytea", imageType)
                    .Replace("timestamp", dateTimeType)
                    .Replace("varchar", charType)
                    .Replace("bigint", longDataType)
                    .Replace("integer", intDataType)
                    .Replace("serial", identityReplace);
        
            // do not execute oracle specific statements
            if ((sql2.Contains("create or replace") || sql2.Contains("create sequence"))
                && !this.connectionType.Contains("oracle"))
            {
                return;
            }

            this.Connection.Execute(sql2, parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameter">The parameter.</param>
        public void Execute(string sql, object parameter = null)
        {
            var sql2 = sql.Trim().Replace("@", parameterPrefix);

            this.Connection.Execute(sql2, parameter);
        }

        /// <summary>
        /// Tables the exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>True if table exists.</returns>
        public bool TableExists(string tableName)
        {
            return this.Query<string>(string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName), null).Any();
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
