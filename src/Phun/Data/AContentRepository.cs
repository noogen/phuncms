namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    /// Provide common use methods for content repository
    /// </summary>
    public abstract class AContentRepository 
    {
        /// <summary>
        /// Gets the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>
        /// Path to temp file that is a zip of the folder content.
        /// </returns>
        public virtual string GetFolder(ContentModel folder)
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
        /// Histories of the specified content.
        /// File repository does not have the ability store history.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// Specific content change history.
        /// </returns>
        public virtual IQueryable<ContentModel> RetrieveHistory(ContentModel content)
        {
            return new List<ContentModel>().AsQueryable();
        }

        /// <summary>
        /// Populate or gets the content provided specific host, path, and name property.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        /// <param name="includeData">if set to <c>true</c> [include data].</param>
        /// <returns>
        /// The <see cref="ContentModel" /> that was passed in.
        /// </returns>
        public abstract ContentModel Retrieve(ContentModel content, bool includeData = true);

        /// <summary>
        /// Lists the specified content.Path
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// Enumerable to content model.
        /// </returns>
        public abstract System.Linq.IQueryable<ContentModel> List(ContentModel content);

        /// <summary>
        /// Populates the history data.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="historyDataId">The history data id.</param>
        public virtual void PopulateHistoryData(ContentModel content, System.Guid historyDataId)
        {
            // default does nothing
        }

        /// <summary>
        /// Normalized the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The normalized path.</returns>
        /// <exception cref="System.ArgumentException">Illegal path detected:  + path;path</exception>
        protected virtual string NormalizedPath(string path)
        {
            // make sure no illegal characters
            var result = path.Replace("%", string.Empty);

            // prevent * as wildcard character unless it is the last character 
            if (result.IndexOf('*') != (result.Length - 1))
            {
                result = result.Replace("*", string.Empty);
            }

            // make sure that there is no illegal path
            result = result.Replace("..", string.Empty).Replace("//", "/");

            if (path.Length < 1)
            {
                throw new ArgumentException("Illegal path length detected: " + path, "path");
            }

            return result;
        }

        /// <summary>
        /// Gets the folder to.
        /// </summary>
        /// <param name="destPhysicalFolder">The destination physical folder.</param>
        /// <param name="sourceFolder">The source folder.</param>
        protected virtual void GetFolderTo(string destPhysicalFolder, ContentModel sourceFolder)
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
                    string contentPhysicalPath = System.IO.Path.Combine(
                        destPhysicalFolder, content.Path.TrimStart('/').Replace("/", "\\"));
                    string directoryName = System.IO.Path.GetDirectoryName(contentPhysicalPath);
                    this.Retrieve(content, true);

                    if (!System.IO.Directory.Exists(directoryName))
                    {
                        System.IO.Directory.CreateDirectory(directoryName);
                    }

                    System.IO.File.WriteAllBytes(contentPhysicalPath, content.Data);
                }
            }
        } // end getfolderto
    }
}
