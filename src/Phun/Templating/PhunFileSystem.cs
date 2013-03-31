namespace Phun.Templating
{
    using System;
    using System.IO;
    using System.Web;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Extensions;
    using Phun.Routing;

    /// <summary>
    /// Default implementation for template file system.
    /// </summary>
    public class PhunFileSystem : IFileSystem
    {
        /// <summary>
        /// The connector
        /// </summary>                                
        protected internal IContentConnector connector;

        /// <summary>
        /// The API
        /// </summary>                  
        protected internal IPhunApi api;

        /// <summary>
        /// The context
        /// </summary>                            
        protected internal HttpContextBase context;

        /// <summary>
        /// The config
        /// </summary>
        protected internal ICmsConfiguration config;

        /// <summary>
        /// My utility
        /// </summary>
        protected internal ResourcePathUtility myUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunFileSystem" /> class.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="connector">The connector.</param>
        /// <param name="context">The context.</param>
        public PhunFileSystem(IPhunApi api, IContentConnector connector, HttpContextBase context)
        {
            this.connector = connector;
            this.api = api;
            this.context = context;
            this.config = Bootstrapper.Config;
        }

        /// <summary>
        /// Reads the file sync.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options is ignored.</param>
        /// <returns>
        /// Returns the contents of the filename.
        /// </returns>
        public string readFileSync(string filename, string options)
        {
            try
            {
                filename = (filename + string.Empty).Trim('.');

                if (!filename.EndsWith(".vash", StringComparison.OrdinalIgnoreCase))
                {
                    filename += ".vash";
                }

                // Use FileModel path with key to support partial content caching
                if (!filename.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    filename = this.api.FileModel.ParentPath + filename;
                }
                else if (!filename.StartsWith("/page", StringComparison.OrdinalIgnoreCase))
                {
                    // since it does start with "/" for absolute path, but does not start with page
                    filename = "/page" + filename;
                }

                var path = this.myUtility.Normalize(filename);

                return this.CacheRetrieve(path) + string.Empty;
            }
            catch (Exception ex)
            {
                return string.Concat("@{ throw new Error('\r\n", ex.Message.Replace("'", "\\'").Replace("\r", string.Empty).Replace("\n", string.Empty), "') }");
            }
        }

        /// <summary>
        /// Caches the retrieve.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>
        /// Cache retrieve.
        /// </returns>
        protected virtual string CacheRetrieve(string filename)
        {
            var resultKey = string.Format("__PhunRetrieveCache__{0}__{1}", this.api.tenantHost, filename);
            var result = this.context.Cache[resultKey] as string;

            if (result == null)
            {
                var path = filename;
                var content = new ContentModel() { Path = path, Host = this.api.tenantHost };

                // search the shared folder for file 
                if (!this.connector.ContentRepository.Exists(content))
                {
                    content.Path = "/page/shared/" + content.FileName;
                }

                // connector should be good for handling and retrieve error
                var contentResult = this.connector.Retrieve(content.Path, this.api.request.url);
                contentResult.SetDataFromStream();
                result = System.Text.Encoding.UTF8.GetString(contentResult.Data);

                // implement sliding expiration for content cache to provide some performance and/or DOS attack
                this.context.Cache.Insert(
                    resultKey, result, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(this.config.CacheDuration));
            }

            return result;
        }
    }
}
