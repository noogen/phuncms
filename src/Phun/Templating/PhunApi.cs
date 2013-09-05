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
        protected internal string host;

        /// <summary>
        /// The phun path
        /// </summary>
        protected internal IPath phunPath;

        /// <summary>
        /// The phun file system
        /// </summary>
        protected internal IFileSystem phunFileSystem;

        /// <summary>
        /// The connector
        /// </summary>
        protected internal IContentConnector connector;

        /// <summary>
        /// The utility
        /// </summary>
        protected internal ResourcePathUtility utility;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunApi" /> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="connector">The connector.</param>
        public PhunApi(HttpContextBase httpContext, IContentConnector connector)
        {
            this.utility = new ResourcePathUtility();
            this.connector = connector;
            this.host = this.utility.GetTenantHost(httpContext.Request.Url);
            this.request = new PhunRequest(httpContext);
            this.response = new PhunResponse(httpContext);
            this.phunFileSystem = new PhunFileSystem(this, connector);
            this.phunPath = new PhunPath();

            this.user = httpContext.User;
            this.cache = new PhunCache(httpContext);
            this.trace = new Trace();
            this.templateCache = new TemplateCache(this, httpContext);
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
        /// Gets or sets the template cache.
        /// </summary>
        /// <value>
        /// The template cache.
        /// </value>
        public TemplateCache templateCache { get; set; }

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
                    if (Bootstrapper.Default.ApiList.ContainsKey(name))
                    {
                        return Activator.CreateInstance(Bootstrapper.Default.ApiList[name]);
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
        public string tenantHost
        {
            get
            {
                return this.host;
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
            return this.utility.PhunPartial(name, this.request.url);
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
        public string partialEditable(string tagName, string contentName, object attributes)
        {
            return this.utility.PhunPartialEditable(this.request.url, tagName, contentName, attributes);
        }

        /// <summary>
        /// Bundleses this instance.
        /// </summary>
        /// <returns>Javascript bundles.</returns>
        public string bundles()
        {   
            return this.utility.PhunCmsRenderBundles();
        }

        /// <summary>
        /// URLs the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// return resource url.
        /// </returns>
        public string resourceUrl(string path)
        {
            return this.utility.GetResourcePath(path);
        }

        /// <summary>
        /// Contenturls the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Get the content url.</returns>
        public string contentUrl(string path)
        {
            return string.Format("/{0}/download/content/{1}", this.utility.Config.ContentRouteNormalized, path).Replace("//", "/");
        }

        /// <summary>
        /// Pages the content URL.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The page content url.
        /// </returns>
        public string pageContentUrl(string path)
        {
            return string.Format("/{0}/download/page/{1}", this.utility.Config.ContentRouteNormalized, path).Replace("//", "/");
        }
    }
}
