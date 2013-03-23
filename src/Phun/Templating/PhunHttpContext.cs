namespace Phun.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Web.Mvc;

    /// <summary>
    /// The phun http context.
    /// </summary>
    public class PhunHttpContext : IHttpContext
    {
        /// <summary>
        /// The controller
        /// </summary>
        private readonly PhunCmsController controller;

        /// <summary>
        /// The require types
        /// </summary>
        private static readonly Dictionary<string, Type> requireTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The tenant host
        /// </summary>
        private readonly string tenantHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunHttpContext" /> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public PhunHttpContext(PhunCmsController controller)
        {
            this.controller = controller;
            this.tenantHost = this.controller.GetCurrentHost(this.controller.ContentConfig, this.controller.Request.Url);

            this.request = new PhunRequest(this.controller.Request);
            this.user = this.controller.User;
            this.cache = new PhunCache(this.controller);
        }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        public IRequest request { get; set; }

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
        /// Requires the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Component to require.</returns>
        public object require(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "fs":
                    return new PhunFileSystem(this.controller);
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
