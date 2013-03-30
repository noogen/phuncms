namespace Phun.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Web;

    using Phun.Data;
    using Phun.Routing;

    /// <summary>
    /// The phun api.
    /// </summary>
    public class PhunApi : IPhunApi
    {
        /// <summary>
        /// The tenant host
        /// </summary>
        private readonly string tenantHost;

        /// <summary>
        /// The phun path
        /// </summary>
        private readonly IPath phunPath;

        /// <summary>
        /// The phun file system
        /// </summary>
        private readonly IFileSystem phunFileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunApi" /> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public PhunApi(HttpContextBase httpContext)
        {
            this.tenantHost = new ResourcePathUtility().GetTenantHost(httpContext.Request.Url);
            this.request = new PhunRequest(httpContext);
            this.response = new PhunResponse(httpContext);
            this.phunFileSystem = new PhunFileSystem(this, httpContext);
            this.phunPath = new PhunPath();

            this.user = httpContext.User;
            this.cache = new PhunCache(httpContext);
            this.trace = new Trace();
        }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public IRequest request { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public IResponse response { get; set; }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        public ICache cache { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public IPrincipal user { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public ContentModel FileModel { get; set; }

        /// <summary>
        /// Gets or sets the trace.
        /// </summary>
        /// <value>
        /// The trace.
        /// </value>
        public ITrace trace { get; set; }

        /// <summary>
        /// Requires the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Component to require.
        /// </returns>
        public object require(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "fs":
                    return this.phunFileSystem;
                case "path":
                    return this.phunPath;
                default:
                    if (Bootstrapper.ApiList.ContainsKey(name))
                    {
                        return Activator.CreateInstance(Bootstrapper.ApiList[name]);
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Gets the tenant host.
        /// </summary>
        /// <value>
        /// The tenant host.
        /// </value>
        public string TenantHost
        {
            get
            {
                return this.tenantHost;
            }
        }

        /// <summary>
        /// Partials the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Render partial without going through the view engine.
        /// </returns>
        public string partial(string name)
        {
            return Phun.Extensions.HtmlHelpers.PhunPartial(name, this.request.url);
        }

        /// <summary>
        /// Partialeditables the specified name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>
        /// Render editable partial without going through the view engine.
        /// </returns>
        public string partialeditable(string tagName, string contentName, object attributes)
        {
            return Phun.Extensions.HtmlHelpers.PhunPartialEditable(this.request.url, tagName, contentName, attributes).ToString();
        }

        /// <summary>
        /// Bundleses this instance.
        /// </summary>
        /// <returns>Javascript bundles.</returns>
        public string bundles()
        {   
            var provider = new ResourcePathUtility();
            return provider.PhunCmsRenderBundles();
        }

        /// <summary>
        /// URLs the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// return resource url.
        /// </returns>
        public string resourceurl(string path)
        {
            var provider = new ResourcePathUtility();
            return provider.GetResourcePath(path);
        }

        /// <summary>
        /// Contenturls the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Get the content url.</returns>
        public string contenturl(string path)
        {
            var provider = new ResourcePathUtility();
            return string.Format("/{0}/download/page/{1}", provider.Config.ContentRouteNormalized, path).Replace("//", "/");
        }

        /// <summary>
        /// Exceptions the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ApplicationException">PhunCMS script exception:  + message</exception>
        public void exception(string message)
        {
            throw new ApplicationException("PhunCMS script exception: " + message);
        }
    }
}
