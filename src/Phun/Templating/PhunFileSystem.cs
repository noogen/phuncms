namespace Phun.Templating
{
    using System;
    using System.IO;
    using System.Web;

    using Phun.Data;
    using Phun.Extensions;

    /// <summary>
    /// Default implementation for template file system.
    /// </summary>
    public class PhunFileSystem : IFileSystem
    {
        /// <summary>
        /// The connector
        /// </summary>
        private readonly ContentConnector connector;

        /// <summary>
        /// The API
        /// </summary>
        private readonly IPhunApi api;

        /// <summary>
        /// The context
        /// </summary>
        private readonly HttpContextBase context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunFileSystem" /> class.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="context">The context.</param>
        public PhunFileSystem(IPhunApi api, HttpContextBase context)
        {
            this.connector = new ContentConnector();
            this.api = api;
            this.context = context;
        }

        /// <summary>
        /// Reads the file sync.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the contents of the filename.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Unable to locate .vash file in both local path and shared folder:  + path</exception>
        public string readFileSync(string filename, string options)
        {
            if (!filename.EndsWith(".vash", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".vash";
            }

            if (!filename.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                filename = this.api.FileModel.ParentPath + filename;
            }
            else if (!filename.StartsWith("/page"))
            {
                filename = "/page" + filename;
            }

            var path = filename;

            var content = new ContentModel()
            {
                Path = path,
                Host = this.api.TenantHost
            };

            // search the chared folder for file 
            if (!this.connector.ContentRepository.Exists(content))
            {
                content.Path = "/page/shared/" + content.FileName;
            }

            var result = this.CacheRetrieve(content);
            if (content.DataLength == null)
            {
                throw new FileNotFoundException("Unable to locate .vash file in both local path and shared folder: " + path);
            }

            return result;
        }

        /// <summary>
        /// Caches the retrieve.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        /// Cache retrieve.
        /// </returns>
        protected virtual string CacheRetrieve(ContentModel content)
        {
            var resultKey = string.Format("__PhunRetrieveCache__{0}__{1}", this.api.TenantHost, content.Path);
            var result = this.context.Cache[resultKey] as string;

            if (result == null)
            {
                var contentResult = this.connector.Retrieve(content.Path, this.api.request.url);

                if (contentResult.DataLength != null)
                {
                    content.DataLength = contentResult.DataLength;
                    contentResult.SetDataFromStream();
                    result = System.Text.Encoding.UTF8.GetString(contentResult.Data);
                }

                // implement 2 seconds sliding expiration for file cache to provide some dos attack support
                if (result != null)
                {
                    this.context.Cache.Insert(
                        resultKey, result, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(2));
                }
            }
            else
            {
                content.DataLength = result.Length;
            }

            return result;
        }
    }
}
