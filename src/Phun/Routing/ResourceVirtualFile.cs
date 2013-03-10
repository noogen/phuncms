namespace Phun.Routing
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    using Phun.Configuration;

    /// <summary>
    /// Get virtual file from resource.  Also use as a utility class.
    /// </summary>
    public class ResourceVirtualFile : VirtualFile
    {
        /// <summary>
        /// The resource path
        /// </summary>
        private readonly string resourcePath;

        /// <summary>
        /// The virtual file path, this is use for debugging purposes.
        /// </summary>
        private string virtualFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceVirtualFile" /> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        public ResourceVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            this.Config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
            this.resourcePath = this.TranslateToResourcePath(virtualPath);
            this.virtualFilePath = virtualPath;
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        protected internal PhunCmsConfigurationSection Config { get; set; }

        /// <summary>
        /// When overridden in a derived class, returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>
        /// A read-only stream to the virtual file.
        /// </returns>
        public override System.IO.Stream Open()
        {
            var util = new ResourcePathUtility();
            var result = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.resourcePath);
            if (result == null && this.resourcePath.EndsWith("scripts.phuncms.config.js"))
            {
                var fileString =
                    string.Format(ResourcePathUtility.ScriptsphuncmsConfigJs, this.Config.ResourceRouteNormalized, this.Config.ContentRouteNormalized)
                        .Replace("[", "{")
                        .Replace("]", "}");

                result = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(fileString));
            }

            if (result == null)
            {
                throw new HttpException(404, "Resource virtual file not found.");
            }

            return result;
        }


        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="context">The context.</param>
        public void WriteFile(HttpContextBase context)
        {
            var response = context.Response;
            var stream = this.Open();

            if (stream == null)
            {
                throw new HttpException(404, "Path '" + this.virtualFilePath + "' cannot be found.");
            }

            response.ContentType = MimeTypes.GetContentType(System.IO.Path.GetExtension(this.virtualFilePath));
            stream.CopyTo(response.OutputStream);
            response.OutputStream.Flush();
            response.End();
        }


        /// <summary>
        /// Translates to resource path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The resource path.</returns>
        protected virtual string TranslateToResourcePath(string virtualPath)
        {
            // actual resource folders are translated to periods
            var resource = (virtualPath + string.Empty)
                .Replace("~", string.Empty)
                .ToLowerInvariant()
                .Substring(this.Config.ResourceRouteNormalized.Length + 2)
                .Replace("/", ".")
                .Trim('.');

            resource = string.Concat("Phun.Properties.", resource);
            return resource;
        }
    }
}
