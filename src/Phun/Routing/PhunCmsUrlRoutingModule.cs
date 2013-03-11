namespace Phun.Routing
{
    using System;
    using System.Configuration;
    using System.Web;
    using System.Web.Routing;

    using Phun.Configuration;

    using StackExchange.Profiling;

    /// <summary>
    /// Url routing modules
    /// </summary>
    public class PhunCmsUrlRoutingModule : UrlRoutingModule
    {
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        protected internal PhunCmsConfigurationSection Config { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCmsUrlRoutingModule"/> class.
        /// </summary>
        public PhunCmsUrlRoutingModule()
        {
            this.Config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
        }

        /// <summary>
        /// Matches the HTTP request to a route, retrieves the handler for that route, and sets the handler as the HTTP handler for the current request.
        /// </summary>
        /// <param name="context">Encapsulates all HTTP-specific information about an individual HTTP request.</param>
        public override void PostResolveRequestCache(HttpContextBase context)
        {
            if (this.Config.IsResourceRoute(context.Request.Path) || this.Config.IsContentRoute(context.Request.Path))
            {
                return;
            }

            using (MiniProfiler.Current.Step("Resolve route"))
            {
                base.PostResolveRequestCache(context);
            }
        }
    }
}
