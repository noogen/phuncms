namespace Phun.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Mvc;

    using Phun.Data;
    using Phun.Routing;

    /// <summary>
    /// The phun api.
    /// </summary>
    public class PhunApi : IPhunApi
    {
        /// <summary>
        /// The require types
        /// </summary>
        private static readonly Dictionary<string, Type> requireTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The tenant host
        /// </summary>
        private readonly string tenantHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunApi" /> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public PhunApi(HttpContextBase httpContext)
        {
            this.tenantHost = new ResourcePathUtility().GetTenantHost(httpContext.Request.Url);
            this.request = new PhunRequest(httpContext);
            this.response = new PhunResponse(httpContext);

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
                    return new PhunFileSystem(this);
                case "path":
                    return new PhunPath();
                default:
                    break;
            }

            if (requireTypes.ContainsKey(name))
            {
                return Activator.CreateInstance(requireTypes[name]);
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
    }
}
