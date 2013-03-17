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
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <exception cref="System.ArgumentException">Connection does not exist.</exception>
        public SqlContentRepository(string connectionStringName, string tableName)
        {
            var cstring = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cstring == null)
            {
                throw new ArgumentException("Connection does not exist: " + connectionStringName);
            }

            this.ConnectionStringName = connectionStringName;
            this.TableName = tableName ?? "CmsContent";

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
	[Host] [nvarchar](200) NOT NULL,
	[Path] [nvarchar](250) NOT NULL,
    [ParentPath] nvarchar(250) NOT NULL,
	[CreateDate] [datetime] NULL,
	[CreateBy] [nvarchar](100) NULL,
	[ModifyDate] [datetime] NULL,
	[ModifyBy] [nvarchar](100) NULL,
    [DataLength] [bigint] NULL, 
	[Data] [image] NULL,
	CONSTRAINT UC_{0} UNIQUE ([Host], [Path])
)
GO
CREATE INDEX IX_{0}_Host ON [{0}] ([Host])
GO
CREATE INDEX IX_{0}_Path ON [{0}] ([Path])
GO
CREATE INDEX IX_{0}_ParentPath ON [{0}] ([ParentPath])
GO
CREATE INDEX IX_{0}_ModifyDate ON [{0}] ([ModifyDate])", 
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
                    "SELECT TOP 1 CreateDate, CreateBy, ModifyDate, ModifyBy, DataLength, [Data] FROM [{0}] WHERE Host = @Host AND [Path] = @Path",
                    this.TableName);

            if (!includeData)
            {
                sqlCommand = sqlCommand.Replace(", [Data] FROM", " FROM");
            }

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
                    content.Data = result.Data;
                    content.DataLength = result.DataLength;
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

            // this is an upsert
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                // is folder, set content to null to help with upsert for folder
                if (content.Path.EndsWith("/"))
                {
                    content.DataId = null;
                    content.DataLength = null;
                }

                this.Save(content, db);

                // attempt to create parent folders
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
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                var tableExists = db.Connection.Query(string.Format(@"SELECT top 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", this.TableName)).Any();
                if (tableExists)
                {
                    return;
                }

                foreach (var sql in this.SchemaSql.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    db.Connection.Execute(sql);
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
            var sqlCommand = string.Format(
@"UPDATE [{0}] SET ModifyDate = getdate(), ModifyBy = @ModifyBy, Data = @Data, DataLength = @DataLength WHERE Host = @Host AND [Path] = @Path",
       this.TableName);
            db.Connection.Execute(sqlCommand, content);

            sqlCommand =
                string.Format(
                    "INSERT INTO [{0}] (Host, [Path], ParentPath, CreateDate, CreateBy, ModifyDate, ModifyBy, Data, DataLength) SELECT @Host, @Path, @ParentPath, getdate(), @CreateBy, getdate(), @ModifyBy, @Data, @DataLength WHERE NOT EXISTS (SELECT TOP 1 1 FROM [{0}] WHERE Host = @Host AND [Path] = @Path)",
                    this.TableName);
            db.Connection.Execute(sqlCommand, content);
        }
    }
}
