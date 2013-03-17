namespace Phun.Data
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    /// <summary>
    /// Dapper data context.
    /// </summary>
    public class DapperContext : IDisposable
    {
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
            
            this.Connection.Open();
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
