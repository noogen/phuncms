namespace Phun.Templating
{
    using System.Web;

    using Phun.Routing;

    /// <summary>
    /// The phun cache.
    /// </summary>
    public class PhunCache : ICache
    {
        /// <summary>
        /// The HTTP context
        /// </summary>
        private readonly HttpContextBase httpContext;

        /// <summary>
        /// The tenant host
        /// </summary>
        private readonly string tenantHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCache" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PhunCache(HttpContextBase context)
        {
            this.httpContext = context;
            this.tenantHost = new ResourcePathUtility().GetTenantHost(context.Request.Url);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The cached object.</returns>
        public object get(string key)
        {
            string myKey = string.Format("__PhunUserCache__${0}${1}", this.tenantHost, key);
            return this.httpContext.Cache.Get(myKey);
        }

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public object set(string key, object value)
        {
            string myKey = string.Format("__PhunUserCache__${0}${1}", this.tenantHost, key);
            this.httpContext.Cache[myKey] = value;
            return value;
        }
    }
}
