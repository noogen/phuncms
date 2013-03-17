namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dapper;

    /// <summary>
    /// SQL server content repository.
    /// </summary>
    public class SqlServerContentRepository : SqlContentRepository
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
        /// Initializes a new instance of the <see cref="SqlServerContentRepository" /> class.
        /// </summary>
        /// <param name="dataRepo">The data repository.</param>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <exception cref="System.ArgumentException">Connection does not exist.</exception>
        public SqlServerContentRepository(ISqlDataRepository dataRepo, string connectionStringName, string cachePath)
            : base(connectionStringName, "PhunContent")
        {
            this.CachePath = cachePath;
            this.DataRepository = dataRepo;
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
                    "SELECT TOP 1 CreateDate, CreateBy, ModifyDate, ModifyBy, DataId, DataLength FROM [{0}] WHERE Host = @Host AND [Path] = @Path",
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
                    content.DataId = result.DataId;
                    content.DataLength = result.DataLength;

                    if (includeData && content.DataId.HasValue)
                    {
                        this.DataRepository.PopulateData(db, content, this.CachePath);
                    }
                }
            }

            return content;
        }

        /// <summary>
        /// Saves the specified content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        public override void Save(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);
            bool isFile = !content.Path.EndsWith("/");

            // this is an upsert
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                // save data first
                if (isFile)
                {
                    this.DataRepository.SaveData(db, content, this.CachePath);
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
        /// Gets the schema SQL.
        /// </summary>
        /// <value>
        /// The schema SQL.
        /// </value>
        protected override string SchemaSql
        {
            get
            {
                return base.SchemaSql.Replace("[Data] [image] NULL,", "[DataId] [uniqueidentifier] NULL,");
            }
        }

        /// <summary>
        /// Ensures the schema.
        /// </summary>
        protected override void EnsureSchema()
        {
            var phunDataSchema = @"
CREATE TABLE [PhunData](
	[Id] [uniqueidentifier] NOT NULL,
	[HostAndPath] [nvarchar](450) NOT NULL,
	[Data] [image] NOT NULL,
	[DataLength] [bigint] NULL,
	[CreateDate] [datetime] NULL,
	[CreateBy] [nvarchar](50) NULL,
	CONSTRAINT [PK_PhunData] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO
CREATE NONCLUSTERED INDEX [IX_PhunData_HostAndPath] ON [PhunData]
(
	[HostAndPath] ASC
)";
            using (var db = new DapperContext(this.ConnectionStringName))
            {
                var tableExists = 1
                                  == db.Connection.Query<int>(
                                      string.Format(
                                          @"SELECT CASE WHEN EXISTS ((SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PhunData')) THEN 1 ELSE 0 END",
                                          this.TableName)).FirstOrDefault();
                if (!tableExists)
                {
                    foreach (var sql in phunDataSchema.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        db.Connection.Execute(sql);
                    }
                }
            }

            base.EnsureSchema();
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
            var sqlCommand = string.Format(
@" UPDATE [{0}] SET ModifyDate = getdate(), ModifyBy = @ModifyBy, DataId = @DataId, DataLength = @DataLength WHERE Host = @Host AND [Path] = @Path

   if @@rowcount = 0
   begin
      INSERT INTO [{0}] (Host, [Path], ParentPath, CreateDate, CreateBy, ModifyDate, ModifyBy, DataId, DataLength) VALUES (@Host, @Path, getdate(), @CreateBy, getdate(), @CreateBy, @DataId, @DataLength)
   end",
       this.TableName);
            db.Connection.Execute(sqlCommand, content);
        }
    }
}
