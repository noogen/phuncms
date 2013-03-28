namespace Phun.Templating
{
    using System;
    using System.Web;
    using System.Web.Mvc;

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
        /// Initializes a new instance of the <see cref="PhunFileSystem" /> class.
        /// </summary>
        /// <param name="api">The API.</param>
        public PhunFileSystem(IPhunApi api)
        {
            this.connector = new ContentConnector();
            this.api = api;
        }

        /// <summary>
        /// Reads the file sync.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the contents of the filename.
        /// </returns>
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

            var result = string.Empty;
            content = this.connector.Retrieve(content.Path, this.api.request.url);
            if (content.DataLength != null)
            {
                content.SetDataFromStream();
                result = System.Text.Encoding.UTF8.GetString(content.Data);
            }

            return result;
        }
    }
}
