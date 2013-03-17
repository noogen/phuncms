namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    /// IContentRepository implementation for file store.
    /// Strategy:
    /// lets say: basePath = c:\App_Data, host = localhost, name = 
    /// { name = , path = home} = c:\App_Data\localhost\home\_index
    /// { name = test, path = data } = c:\App_Data\localhost\data\test
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class FileContentRepository : AContentRepository, IContentRepository
    {
        /// <summary>
        /// The default host
        /// </summary>
        private string defaultHost = "localhost";

        /// <summary>
        /// The base path
        /// </summary>
        private string basePath = string.Empty;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentRepository" /> class.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <exception cref="System.ArgumentException">basePath is required.;basePath</exception>
        public FileContentRepository(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentException("basePath is required.", "basePath");
            }

            if (!System.IO.Directory.Exists(basePath))
            {
                throw new ArgumentException("Please check PhunCms content repository configuration.  Path does not exists: " + basePath, "basePath");
            }

            this.basePath = basePath;
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
            var file = this.ResolvePath(content);
            if (File.Exists(file))
            {
                if (includeData)
                {
                    content.Data = System.IO.File.ReadAllBytes(file);
                    content.DataLength = content.Data.Length;
                }

                var fi = new FileInfo(file);
                content.ModifyDate = fi.LastWriteTime;
                content.CreateDate = fi.CreationTime;
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
            var path = this.ResolvePath(content);
            return content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? System.IO.Directory.Exists(path) : File.Exists(path);
        }

        /// <summary>
        /// Saves the specified content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        public void Save(ContentModel content)
        {
            var pathAndName = this.ResolvePath(content);
            var path = Path.GetDirectoryName(pathAndName);
            var isValidChildOfBasePath =
                System.IO.Path.GetFullPath(path)
                      .StartsWith(System.IO.Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);

            if (!isValidChildOfBasePath)
            {
                return;
            }

            // make sure directories exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            // write all data
            File.WriteAllBytes(pathAndName, content.Data);
        }

        /// <summary>
        /// Removes the specified content path or route.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.  If name is set to "*" then removes all for host and path.</param>
        public void Remove(ContentModel content)
        {
            var path = this.ResolvePath(content);
            var isValidChildOfBasePath =
                            System.IO.Path.GetFullPath(path)
                                  .StartsWith(System.IO.Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase);

            if (!isValidChildOfBasePath)
            {
                return;
            }

            if (File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            else if (path.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                this.Empty(path.Replace("*", string.Empty));
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
            var path = this.ResolvePath(content);
            var result = new List<ContentModel>();
            var isValidChildOfBasePath = System.IO.Path.GetFullPath(path)
                        .StartsWith(System.IO.Path.GetFullPath(this.basePath), StringComparison.OrdinalIgnoreCase);

            // don't do anything for invalid path
            if (!isValidChildOfBasePath)
            {
                return result.AsQueryable();
            }

            // only proceed if it is a folder 
            if (content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                var directory = new DirectoryInfo(path);

                // build directory result
                foreach (var dir in directory.GetDirectories())
                {
                    result.Add(
                        new ContentModel()
                            {
                                Host = content.Host,
                                Path = string.Concat(content.Path, dir.Name, "/"),
                                CreateDate = directory.CreationTime,
                                ModifyDate = directory.LastWriteTime
                            });
                }

                // build file result
                foreach (var file in directory.GetFiles())
                {
                    result.Add(
                        new ContentModel()
                            {
                                Host = content.Host,
                                Path = string.Concat(content.Path, file.Name),
                                CreateDate = file.CreationTime,
                                ModifyDate = file.LastWriteTime
                            });
                }
            }

            return result.AsQueryable();
        }

        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// The full path to the content or file.
        /// </returns>
        /// <exception cref="System.ArgumentException">Content is required.;content
        /// or
        /// Content path is required or not valid:  + content.Path;content.Path</exception>
        private string ResolvePath(ContentModel content)
        {
            bool isFolder = content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase);

            if (content == null)
            {
                throw new ArgumentException("Content is required.", "content");
            }

            if (string.IsNullOrEmpty(content.Host))
            {
                content.Host = this.defaultHost;
            }

            // add: 'basePath\host\contentPath'
            var result = string.Concat(this.basePath, "\\", content.Host, "\\", content.Path.Trim('/').Replace("/", "\\"));

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

            return result;
        }

        /// <summary>
        /// Empties the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        private void Empty(string path)
        {
            var directory = new DirectoryInfo(path);
            directory.Delete(true);
        }
    }
}
