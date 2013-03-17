namespace Phun.Data
{
    using System;
    using System.IO;
    using System.Linq;

    using Dapper;

    using Phun.Extensions;

    /// <summary>
    /// Use by SQL Server Repository to store and retrieve data.
    /// </summary>
    public class PhunDataRepository : ISqlDataRepository
    {
        /// <summary>
        /// Populates the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>
        /// Content model with populated data stream.
        /// </returns>
        public virtual ContentModel PopulateData(DapperContext context, ContentModel content, string cachePath)
        {
            this.CacheData(context, content, cachePath);

            if (!string.IsNullOrEmpty(cachePath))
            {
                var localPath = this.ResolvePath(content, cachePath);

                // return a stream
                content.DataStream = System.IO.File.OpenRead(localPath);
                content.Data = null;
            }

            return content;
        }

        /// <summary>
        /// Caches the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        public virtual void CacheData(DapperContext context, ContentModel content, string cachePath)
        {
            if (content.Data == null)
            {
                var data =
                    context.Connection.Query<ContentModel>(
                        "SELECT Data, DataLength FROM PhunData WHERE Id = @DataId", content).FirstOrDefault();

                if (data != null)
                {
                    content.Data = data.Data;
                    content.DataLength = data.DataLength;
                }
            }

            if (string.IsNullOrEmpty(cachePath))
            {
                return;
            }

            // determine if local content exists or is out of date
            var localPath = this.ResolvePath(content, cachePath);
            var canCache = !File.Exists(localPath);
            if (!canCache)
            {
                var lastWriteTime = File.GetLastWriteTime(localPath);
                canCache = lastWriteTime < (content.ModifyDate ?? content.CreateDate);
            }

            if (canCache)
            {
                var localDir = System.IO.Path.GetDirectoryName(localPath);
                if (!System.IO.Directory.Exists(localDir))
                {
                    System.IO.Directory.CreateDirectory(localDir);
                }

                System.IO.File.WriteAllBytes(localPath, content.Data ?? new byte[0]);
            }
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        public virtual void SaveData(DapperContext context, ContentModel content, string cachePath)
        {
            var newDataId = Guid.NewGuid();
            var newContent = new ContentModel()
                                 {
                                     DataId = newDataId,
                                     Host = content.Host,
                                     Path = content.Path,
                                     CreateBy =
                                         string.IsNullOrEmpty(content.ModifyBy)
                                             ? content.CreateBy
                                             : content.ModifyBy,
                                     Data =
                                         content.Data ?? content.DataStream.ReadAll(),
                                 };
            newContent.DataLength = content.Data != null ? content.Data.Length : 0;
            context.Connection.Execute(
                    "INSERT INTO PhunData (Id, HostAndPath, Data, DataLength, CreateDate, CreateBy) VALUES (@DataId, @Host + @Path, @Data, @DataLength, getdate(), CreateBy)", content);
            content.Data = newContent.Data;
            content.DataLength = newContent.DataLength;
            content.DataId = newContent.DataId;
            this.CacheData(context, content, cachePath);
        }

        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns>
        /// The full path to the content or file.
        /// </returns>
        /// <exception cref="System.ArgumentException">Content is required.;content
        /// or
        /// Content path is required or not valid:  + content.Path;content.Path</exception>
        private string ResolvePath(ContentModel content, string basePath)
        {
            bool isFolder = content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase);

            if (content == null)
            {
                throw new ArgumentException("Content is required.", "content");
            }


            // add: 'basePath\host\contentPath'
            var result = string.Concat(basePath, "\\", content.Host, "\\", content.Path.Trim('/').Replace("/", "\\"));

            // make sure that there is no illegal path
            result = result.Replace("..", string.Empty).Replace("\\\\", "\\").TrimEnd('\\');

            // result full path must not be more than 3 characters
            if (result.Length <= 3)
            {
                throw new ArgumentException("Illegal path detected: " + content.Path, "path");
            }

            if (isFolder)
            {
                result = result + "\\";
            }


            var isValidChildOfBasePath = System.IO.Path.GetFullPath(result)
                        .StartsWith(System.IO.Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);

            if (!isValidChildOfBasePath)
            {
                throw new ArgumentException("Illegal path access: " + content.Path, "path");
            }

            return result;
        }
    }
}
