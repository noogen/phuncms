namespace Phun.Routing
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;

    using Phun.Configuration;

    /// <summary>
    /// Allow for mapping of contents to our resources.
    /// </summary>
    public class ResourcePathProvider : VirtualPathProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePathProvider"/> class.
        /// </summary>
        public ResourcePathProvider()
        {
            this.Config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        protected internal PhunCmsConfigurationSection Config { get; set; }

        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>
        /// True if file exists.
        /// </returns>
        public override bool FileExists(string virtualPath)
        {
            return this.IsEmbeddedResourcePath(virtualPath) || base.FileExists(virtualPath);
        }

        /// <summary>
        /// Gets a virtual file from the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>
        /// A descendent of the <see cref="T:System.Web.Hosting.VirtualFile" /> class that represents a file in the virtual file system.
        /// </returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (this.IsEmbeddedResourcePath(virtualPath))
            {
                return new ResourceVirtualFile(virtualPath);
            }

            return base.GetFile(virtualPath);
        }

        /// <summary>
        /// Creates a cache dependency based on the specified virtual paths.
        /// </summary>
        /// <param name="virtualPath">The path to the primary virtual resource.</param>
        /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource.</param>
        /// <param name="utcStart">The UTC time at which the virtual resources were read.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Caching.CacheDependency" /> object for the specified virtual resources.
        /// </returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (this.IsEmbeddedResourcePath(virtualPath))
            {
                return null;
            }

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        /// <summary>
        /// Determines whether [is embedded resource path] [the specified virtual path].
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>
        ///   <c>true</c> if [is embedded resource path] [the specified virtual path]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsEmbeddedResourcePath(string virtualPath)
        {
            var checkPath = VirtualPathUtility.ToAppRelative(virtualPath).Trim();
            return this.Config.IsResourceRoute(checkPath);
        }
    }
}
