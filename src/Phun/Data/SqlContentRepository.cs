namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    /// IContentRepository implementation for SQL store.
    /// </summary>
    public class SqlContentRepository : IContentRepository
    {
        /// <summary>
        /// The connection string
        /// </summary>
        protected readonly string ConnectionString;

        /// <summary>
        /// The factory
        /// </summary>
        protected readonly DbProviderFactory DbFactory;

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
                throw new ArgumentException("Connection does not exist.");
            }

            this.DbFactory = DbProviderFactories.GetFactory(cstring.ProviderName);
            this.ConnectionString = cstring.ConnectionString;
            this.TableName = tableName ?? "CmsContent";

            if (!schemaExists)
            {
                schemaExists = true;
                this.EnsureSchema();
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
        public ContentModel Retrieve(ContentModel content, bool includeData = true)
        {
            /* this is horrible manual labor/classic asp.net direct access sql stuff
             * I'm only doing this for the benefit of integrating with mini-profiler
             * I don't even know if it work with anything other than SQL Server
             */

            using (var conn = this.OpenConnection())
            {
                content.Path = this.NormalizedPath(content.Path);
                var sqlCommand = string.Format("SELECT TOP 1 CreateDate, CreateBy, ModifyDate, ModifyBy, Data FROM [{0}] WHERE HostName = @Host AND Path = @Path", this.TableName);
                if (!includeData)
                {
                    sqlCommand = sqlCommand.Replace("ModifyBy, Data FROM", "ModifyBy FROM");
                }

                var command = this.DbFactory.CreateCommand();
                command.Connection = conn;
                command.CommandText = string.Format("SELECT TOP 1 CreateDate, CreateBy, ModifyDate, ModifyBy, Data FROM [{0}] WHERE HostName = @Host AND Path = @Path", this.TableName);
                this.AddParam(command, DbType.String, content.Path, "Path");
                this.AddParam(command, DbType.String, content.Host, "Host");

                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                if (!reader.Read())
                {
                    return content;
                }

                if (includeData)
                {
                    var dataIdx = reader.GetOrdinal("Data");
                    long len = reader.GetBytes(dataIdx, 0, null, 0, 0);

                    // Create a buffer to hold the bytes, and then 
                    // read the bytes from the DataTableReader.
                    var buffer = new byte[len];
                    reader.GetBytes(dataIdx, 0, buffer, 0, (int)len);

                    content.Data = buffer;
                }

                content.CreateBy = reader.GetValue(reader.GetOrdinal("CreateBy")) + string.Empty;
                content.ModifyBy = reader.GetValue(reader.GetOrdinal("ModifyBy")) + string.Empty;

                if (reader.GetValue(reader.GetOrdinal("CreateDate")) != DBNull.Value)
                {
                    content.CreateDate = reader.GetDateTime(reader.GetOrdinal("CreateDate"));
                }

                if (reader.GetValue(reader.GetOrdinal("ModifyDate")) != DBNull.Value)
                {
                    content.CreateDate = reader.GetDateTime(reader.GetOrdinal("ModifyDate"));
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

            using (var conn = this.OpenConnection())
            {
                var command = this.DbFactory.CreateCommand();
                command.Connection = conn;
                command.CommandText = string.Format("SELECT TOP 1 1 FROM [{0}] WHERE HostName = @Host AND Path = @Path", this.TableName);
                this.AddParam(command, DbType.String, content.Path, "Path");
                this.AddParam(command, DbType.String, content.Host, "Host");

                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader.Read();
            }
        }

        /// <summary>
        /// Saves the specified content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        public void Save(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);
            var exists = this.Exists(content);

            // this is an upsert
            using (var conn = this.OpenConnection())
            {
                var command = this.DbFactory.CreateCommand();
                bool isFile = !content.Path.EndsWith("/");
                if (exists)
                {
                    if (isFile)
                    {
                        command.CommandText =
                            string.Format(@"UPDATE [{0}] SET [Data] = @Data, [ModifyDate] = getdate(), [ModifyBy] = @ModifyBy WHERE HostName = @Host AND [Path] = @Path"
                                , this.TableName);
                    }
                    else
                    {
                         command.CommandText =
                            string.Format(@"UPDATE [{0}] SET [ModifyDate] = getdate(), [ModifyBy] = @ModifyBy WHERE HostName = @Host AND [Path] = @Path"
                                , this.TableName);
                    }
                }
                else
                {
                    // if this is a file insert
                    // insert folders first
                    if (isFile)
                    {
                        // while not root dir, attempt to parent folders
                        var parentPath = content.ParentDirectory;
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
                            this.Save(model);
                            parentPath = model.ParentDirectory;
                        }

                        // then insert file
                        command.CommandText =
                            string.Format(@"INSERT INTO [{0}] ([HostName], [Path], [CreateDate], [CreateBy], [Data]) VALUES (@Host, @Path, getdate(), @CreateBy, @Data)"
                                , this.TableName);
                    }
                    else
                    {
                        // if it is a folder, just insert with null data
                        command.CommandText =
                            string.Format(@"INSERT INTO [{0}] ([HostName], [Path], [CreateDate], [CreateBy]) VALUES (@Host, @Path, getdate(), @CreateBy)"
                                , this.TableName);
                    }
                }

                command.Connection = conn;
                this.AddParam(command, DbType.String, content.Path, "Path");
                this.AddParam(command, DbType.String, content.Host, "Host");
                this.AddParam(command, DbType.String, content.CreateBy ?? string.Empty, "CreateBy");
                this.AddParam(command, DbType.String, content.ModifyBy ?? string.Empty, "ModifyBy");
                this.AddParam(command, DbType.Binary, content.Data, "Data");

                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        /// <summary>
        /// Removes the specified content path or route.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.  If name is set to "*" then removes all for host and path.</param>
        public void Remove(ContentModel content)
        {
            content.Path = this.NormalizedPath(content.Path);

            using (var conn = this.OpenConnection())
            {
                var command = this.DbFactory.CreateCommand();
                var path = content.Path;
                var operString = "=";
                if (content.Path.EndsWith("*"))
                {
                    operString = "LIKE";
                    path = path.TrimEnd('*').TrimEnd('/') + "%";
                }

                command.Connection = conn;
                command.CommandText = string.Format("DELETE FROM [{0}] WHERE HostName = @Host AND ([Path] {1} @Path)", this.TableName, operString);
                this.AddParam(command, DbType.String, path, "Path");
                this.AddParam(command, DbType.String, content.Host, "Host");

                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        /// <summary>
        /// Lists the specified content.Path
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// Enumerable to content model.
        /// </returns>
        public IQueryable<ContentModel> List(ContentModel content)
        {
            /* this is horrible manual labor/classic asp.net direct access sql stuff
            * I'm only doing this for the benefit of integrating with mini-profiler
            * I don't even know if it work with anything other than SQL Server
            */
            content.Path = this.NormalizedPath(content.Path);
            var result = new List<ContentModel>();

            // completely prevent wildcard in this search since we're doing our own wildcard
            content.Path = content.Path.Replace("*", string.Empty).TrimEnd('/') + "/";

            // this say that we should only get folders
            var myStartPath = content.Path + "%";

            using (var conn = this.OpenConnection())
            {
                var command = this.DbFactory.CreateCommand();
                command.Connection = conn;

// where match host
// match all files under parent folder
// match all folders under parent folder
                command.CommandText = string.Format(@"SELECT Path, CreateDate, CreateBy, ModifyDate, ModifyBy 
FROM [{0}] WHERE HostName = @Host 
AND (Path LIKE @StartPath AND Path NOT LIKE (@StartPath + '/%')) 
OR (Path LIKE (@StartPath + '/') AND PATH NOT LIKE (@StartPath + '/%/'))", this.TableName);

                this.AddParam(command, DbType.String, myStartPath, "StartPath");
                this.AddParam(command, DbType.String, content.Host, "Host");

                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    var newContent = new ContentModel() { Host = content.Host };

                    newContent.Path = reader.GetString(reader.GetOrdinal("Path"));
                    newContent.CreateBy = reader.GetValue(reader.GetOrdinal("CreateBy")) + string.Empty;
                    newContent.ModifyBy = reader.GetValue(reader.GetOrdinal("ModifyBy")) + string.Empty;

                    if (reader.GetValue(reader.GetOrdinal("CreateDate")) != DBNull.Value)
                    {
                        newContent.CreateDate = reader.GetDateTime(reader.GetOrdinal("CreateDate"));
                    }

                    if (reader.GetValue(reader.GetOrdinal("ModifyDate")) != DBNull.Value)
                    {
                        newContent.CreateDate = reader.GetDateTime(reader.GetOrdinal("ModifyDate"));
                    }

                    result.Add(newContent);
                }
            }

            // parse folder and files from result
            var folders = new SortedDictionary<string, ContentModel>(StringComparer.OrdinalIgnoreCase);
            var files = new SortedList<string, ContentModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in result)
            {
                var itemPath = item.Path.Substring(content.Path.Length);
                var pathIndex = itemPath.IndexOf('/');
                if (pathIndex > 0)
                {
                    string folderName = itemPath.Substring(0, pathIndex);
                    if (!folders.ContainsKey(folderName))
                    {
                        folders.Add(
                            folderName,
                            new ContentModel()
                                {
                                    Path = string.Concat(content.Path, folderName, "/"),
                                    Host = content.Host
                                });
                    }
                }
                else
                {
                    files.Add(item.Path, item);

                    // set back item full path
                    item.Path = string.Concat(content.Path, item.Path.Trim('/'));
                }
            }

            result.Clear();
            result.AddRange(folders.Values);
            result.AddRange(files.Values);

            return result.AsQueryable();
        }

        /// <summary>
        /// Gets the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>
        /// Path to temp file that is a zip of the folder content.
        /// </returns>
        public string GetFolder(ContentModel folder)
        {
            var fileName = Guid.NewGuid().ToString();
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            System.IO.Directory.CreateDirectory(tempPath);

            this.GetFolderTo(tempPath, folder);

            // zip up folder
            var tempFile = tempPath + ".zip";
            ZipFile.CreateFromDirectory(tempPath, tempFile, CompressionLevel.Fastest, false);

            try
            {
                System.IO.Directory.Delete(tempPath, true);
            }
            catch
            {
                // just try to delete the temp folder we just created, do nothing if error
            }

            return tempFile;
        }

        /// <summary>
        /// Ensures the schema.
        /// </summary>
        internal void EnsureSchema()
        {
            using (var conn = this.OpenConnection())
            {
                var command = this.DbFactory.CreateCommand();

                command.Connection = conn;

                command.CommandText = string.Format("SELECT CASE WHEN EXISTS ((SELECT top 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')) THEN 1 ELSE 0 END", this.TableName);
                var tablesExist = (int)command.ExecuteScalar() == 1;
                if (tablesExist)
                {
                    return;
                }

                // because we support SQLCE, we are limited by index and sizes 
                var sqlString = string.Format(@"CREATE TABLE [{0}](
	[HostName] [nvarchar](200) NOT NULL,
	[Path] [nvarchar](250) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateBy] [nvarchar](100) NOT NULL,
	[Data] [image] NULL,
	[ModifyDate] [datetime] NULL,
	[ModifyBy] [nvarchar](100) NULL, 
	CONSTRAINT UC_{0} UNIQUE ([HostName], [Path])
)
GO
CREATE INDEX IX_{0}_HostName ON [{0}] ([HostName])
GO
CREATE INDEX IX_{0}_Path ON [{0}] ([Path])
GO
CREATE INDEX IX_{0}_ModifyDate ON [{0}] ([ModifyDate])
", this.TableName);

                // sql ce must split into multiple execute
                if (command.GetType().FullName.IndexOf("ce", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    foreach (var sql in sqlString.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    command.CommandText = sqlString;
                    command.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns>An opened connection.</returns>
        private DbConnection OpenConnection()
        {
            var conn = this.DbFactory.CreateConnection();
            conn.ConnectionString = this.ConnectionString;
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Adds the param.
        /// http://dotnetfacts.blogspot.com/2009/01/adonet-command-parameters.html
        /// </summary>
        /// <param name="forCommand">For command.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        protected virtual void AddParam(DbCommand forCommand, DbType type, object value, string name)
        {
            var parameterPrefix = "@";
            var typeName = forCommand.GetType().FullName;

            if (typeName.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) > 0)
            {
                parameterPrefix = "?";
                forCommand.CommandText = forCommand.CommandText.Replace("@", parameterPrefix);
            }
            else if (typeName.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) > 0)
            {
                parameterPrefix = ":";
                forCommand.CommandText = forCommand.CommandText.Replace("@", parameterPrefix);
            }

            // any other dbcommand would require override and/or custom implementation register with ServiceLocator
            var param = forCommand.CreateParameter();
            param.DbType = type;
            param.Value = value;
            param.ParameterName = parameterPrefix + name;
            forCommand.Parameters.Add(param);
        }

        /// <summary>
        /// Normalized the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The normalized path.</returns>
        /// <exception cref="System.ArgumentException">Illegal path detected:  + path;path</exception>
        private string NormalizedPath(string path)
        {
            // make sure no illegal characters
            var result = path.Replace("%", string.Empty);

            // prevent * as wildcard character unless it is the last character 
            if (result.IndexOf('*') != (result.Length - 1))
            {
                result = result.Replace("*", string.Empty);
            }

            if (path.Length < 1)
            {
                throw new ArgumentException("Illegal path detected: " + path, "path");
            }

            return result;
        }

        /// <summary>
        /// Gets the folder to.
        /// </summary>
        /// <param name="destPhysicalFolder">The destination physical folder.</param>
        /// <param name="sourceFolder">The source folder.</param>
        private void GetFolderTo(string destPhysicalFolder, ContentModel sourceFolder)
        {
            var result = this.List(sourceFolder);
            foreach (var content in result)
            {
                // a folder, do recursive call
                if (content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    this.GetFolderTo(destPhysicalFolder, content);
                }
                else
                {
                    string contentPhysicalPath = System.IO.Path.Combine(destPhysicalFolder, content.Path.TrimStart('/').Replace("/", "\\"));
                    string directoryName = System.IO.Path.GetDirectoryName(contentPhysicalPath);
                    this.Retrieve(content, true);

                    if (!System.IO.Directory.Exists(directoryName))
                    {
                        System.IO.Directory.CreateDirectory(directoryName);
                    }

                    System.IO.File.WriteAllBytes(contentPhysicalPath, content.Data);
                }
            }
        }
    }
}
