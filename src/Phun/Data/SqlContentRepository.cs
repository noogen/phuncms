namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Compression;
    using System.Linq;

    using Dapper;

    /// <summary>
    /// IContentRepository implementation for SQL store.
    /// </summary>
    public class SqlContentRepository : AContentRepository, IContentRepository
    {
        /// <summary>
        /// The cache path
        /// </summary>
        protected readonly string CachePath;

        /// <summary>
        /// The data repository
        /// </summary>
        protected readonly ISqlDataRepository DataRepository;

        /// <summary>
        /// The connection string name
        /// </summary>
        protected readonly string ConnectionStringName;

        /// <summary>
        /// The table name
        /// </summary>
        protected readonly string TableName;

        /// <summary>
        /// The ensured schema
        /// </summary>
        private static volatile bool schemaExists = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlContentRepository" /> class.
        /// </summary>
        /// <param name="dataRepo">The data repo.</param>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <exception cref="System.ArgumentException">Connection does not exist.</exception>
        public SqlContentRepository(ISqlDataRepository dataRepo, string connectionStringName, string tableName, string cachePath)
        {
            var cstring = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cstring == null)
            {
                throw new ArgumentException("Connection does not exist: " + connectionStringName);
            }

            this.ConnectionStringName = connectionStringName;
            this.TableName = tableName ?? "CmsContent";
            this.CachePath = cachePath;
            this.DataRepository = dataRepo;

            if (!schemaExists)
            {
                schemaExists = true;
                this.EnsureSchema();
            }
        }

        /// <summary>
        /// Gets the schema SQL.
        /// </summary>
        /// <value>
        /// The schema SQL.
        /// </value>
        protected virtual string SchemaSql
        {
            get
            {
                return string.Format(
@"CREATE TABLE [{0}](
	[Host]         NVARCHAR(200) NOT NULL,
	[Path]         NVARCHAR(250) NOT NULL,
    [ParentPath]   NVARCHAR(250) NOT NULL,
	[CreateDate]   DATETIME NULL,
	[CreateBy]     NVARCHAR(100) NULL,
	[ModifyDate]   DATETIME NULL,
	[ModifyBy]     NVARCHAR(100) NULL,
    [DataLength]   BIGINT NULL, 
	[DataIdString] NVARCHAR(38) NULL,
	CONSTRAINT UC_{0} UNIQUE ([Host], [Path])
)
GO
CREATE INDEX IX_{0}_Host ON [{0}] ([Host])
GO
CREATE INDEX IX_{0}_Path ON [{0}] ([Path])
GO
CREATE INDEX IX_{0}_ParentPath ON [{0}] ([ParentPath])
GO
CREATE INDEX IX_{0}_ModifyDate ON [{0}] ([ModifyDate])
", 
this.TableName);
            }
        }

        /// <summary>
        /// Populate or gets the content provided specific host, path, and name property.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        /// <param name="includeData">if set to <c>true</c> [include data].</param>
        /// <returns>
        /// The <see cref="ContentModel" /> that was passed in.
        /// </returns>
        public override ContentModel Retrieve(ContentModel content, bool includeData = true)
        {
            content.Path = this.NormalizedPath(content.Path);
            var sqlCommand =
                string.Format(
                    "SELECT TOP 1 CreateDate, CreateBy, ModifyDate, ModifyBy, DataIdString, DataLength FROM [{0}] WHERE Host = @Host AND [Path] = @Path",
                    this.TableName);

            using (var db = new DapperContext(this.ConnectionStringName))
            {
                var result =
                    db.Connection.Query<ContentModel>(sqlCommand, new { Path = content.Path, Host = content.Host })
                      .FirstOrDefault();

                if (result != null)
                {
                    content.CreateDate = result.CreateDate;
                    content.CreateBy = result.CreateBy;
                    content.ModifyDate = result.ModifyDate;
                    content.ModifyBy = result.ModifyBy;
                    content.DataIdString = result.DataIdString;
                    content.DataLength = result.DataLength;

                    if (includeData && content.DataId.HasValue)
                    {
                        this.DataRepository.PopulateData(db, content, this.TableName + "Data", this.CachePath);
                    }
                }
            }

            return content;
        }

        /// <summary>
        /// Check for exist of content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        /// <returns>
        /// true if content exists.
        /// </returns>
        public bool Exists(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);

            using (var db = new DapperContext(this.ConnectionStringName))
            {
                return db.Connection.Query(
                                  string.Format(
                                      "SELECT TOP 1 Host FROM [{0}] WHERE Host = @Host AND [Path] = @Path",
                                      this.TableName),
                                  new { Host = content.Host, Path = content.Path }).Any();
            }
        }

        /// <summary>
        /// Saves the specified content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        public virtual void Save(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);
            bool isFile = !content.Path.EndsWith("/");

            // this is an upsert
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                // save data first
                if (isFile)
                {
                    content.DataLength = content.DataLength ?? content.Data.Length;

                    this.DataRepository.SaveData(db, content, this.TableName + "Data", this.CachePath);
                }
                else
                {
                    // set null to help with upsert for folder
                    content.DataId = null;
                    content.DataLength = null;
                }

                this.Save(content, db);

                // create parent folders
                var parentPath = content.ParentPath;
                while (parentPath != "/")
                {
                    var model = new ContentModel()
                    {
                        Path = parentPath,
                        Host = content.Host,
                        CreateBy = content.CreateBy,
                        CreateDate = content.CreateDate,
                        ModifyBy = content.ModifyBy,
                        ModifyDate = content.ModifyDate
                    };

                    this.Save(model, db);
                    parentPath = model.ParentPath;
                }
            }
        }

        /// <summary>
        /// Removes the specified content path or route.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.  If name is set to "*" then removes all for host and path.</param>
        public void Remove(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);
            var path = content.Path;
            var operString = "=";
            if (content.Path.EndsWith("*"))
            {
                operString = "LIKE";
                path = path.TrimEnd('*').TrimEnd('/') + "%";
            }

            using (var db = new DapperContext(this.ConnectionStringName))
            {
                db.Connection.Execute(
                    string.Format(
                        "DELETE FROM [{0}] WHERE Host = @Host AND ([Path] {1} @Path)", this.TableName, operString),
                    new { Path = path, Host = content.Host });
            }
        }

        /// <summary>
        /// Lists the specified content.Path
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// Enumerable to content model.
        /// </returns>
        public override IQueryable<ContentModel> List(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);

            // completely prevent any wildcard in this search since we're doing our own wildcard
            content.Path = content.Path.Replace("*", string.Empty).TrimEnd('/') + "/";

            using (var db = new DapperContext(this.ConnectionStringName))
            {
                return
                    db.Connection.Query<ContentModel>(
                        string.Format(
                            @"SELECT Path, CreateDate, CreateBy, ModifyDate, ModifyBy FROM [{0}] WHERE Host = @Host AND ParentPath = @ParentPath ORDER BY Path",
                            this.TableName),
                        new { Host = content.Host, ParentPath = content.Path }).AsQueryable();
            }
        }

        /// <summary>
        /// Ensures the schema.
        /// </summary>
        protected virtual void EnsureSchema()
        {
            var phunDataSchema = string.Format(@"
CREATE TABLE [{0}Data](
	[IdString]    NVARCHAR(38) NOT NULL PRIMARY KEY,
	[Host]        NVARCHAR(200) NOT NULL,
	[Path]        NVARCHAR(250) NOT NULL,
	[Data]        IMAGE,
	[DataLength]  BIGINT NULL,
	[CreateDate]  DATETIME NULL,
	[CreateBy]    NVARCHAR(50) NULL
)
GO
CREATE INDEX [IX_{0}Data_Host] ON [{0}Data] ([Host])
GO
CREATE INDEX [IX_{0}Data_Path] ON [{0}Data] ([Path])
", 
  this.TableName);
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                if (this.DataRepository is SqlDataRepository)
                {
                    var dataTableExists =
                        db.Connection.Query(
                            string.Format(
                                @"SELECT top 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}Data'",
                                this.TableName)).Any();
                    if (!dataTableExists)
                    {
                        foreach (var sql in
                            phunDataSchema.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            try
                            {
                                db.Connection.Execute(sql);
                            }
                            catch (Exception ex)
                            {
                                if (sql.IndexOf("IMAGE,", StringComparison.OrdinalIgnoreCase) > 0)
                                {
                                    var sql2 = sql.Replace("IMAGE,", "BLOB,");
                                    db.Connection.Execute(sql2);
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }

                var tableExists = db.Connection.Query(string.Format(@"SELECT top 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", this.TableName)).Any();
                if (!tableExists)
                {
                    foreach (var sql in this.SchemaSql.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        db.Connection.Execute(sql);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the specified content.
        /// Since it is SQL Server, we can write better SQL than the generic one.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="db">The db context.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void Save(ContentModel content, DapperContext db)
        {
            // upsert means we have to use two separate statement since this would support sqlce
            content.ModifyDate = DateTime.UtcNow;
            if (!content.CreateDate.HasValue || content.CreateDate.Value == DateTime.MinValue)
            {
                content.CreateDate = DateTime.UtcNow;
            }

            var sqlCommand = string.Format(
@"UPDATE [{0}] SET ModifyDate = @ModifyDate, ModifyBy = @ModifyBy, DataIdString = @DataIdString, DataLength = @DataLength WHERE Host = @Host AND [Path] = @Path",
       this.TableName);
            db.Connection.Execute(sqlCommand, content);

            sqlCommand =
                string.Format(
                    "INSERT INTO [{0}] (Host, [Path], ParentPath, CreateDate, CreateBy, ModifyDate, ModifyBy, DataIdString, DataLength) SELECT @Host, @Path, @ParentPath, @CreateDate, @CreateBy, @ModifyDate, @ModifyBy, @DataIdString, @DataLength WHERE NOT EXISTS (SELECT TOP 1 1 FROM [{0}] WHERE Host = @Host AND [Path] = @Path)",
                    this.TableName);
            db.Connection.Execute(sqlCommand, content);
        }
    }
}
