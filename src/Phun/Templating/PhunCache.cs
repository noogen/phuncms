namespace Phun.Templating
{
    /// <summary>
    /// The phun cache.
    /// </summary>
    public class PhunCache : ICache
    {
        /// <summary>
        /// The controller
        /// </summary>
        private readonly PhunCmsController controller;

        /// <summary>
        /// The tenant host
        /// </summary>
        private readonly string tenantHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCache"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public PhunCache(PhunCmsController controller)
        {
            this.controller = controller;
            this.tenantHost = this.controller.GetCurrentHost(this.controller.ContentConfig, this.controller.Request.Url);
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object get(string key)
        {
            string myKey = string.Format("PhunCache${0}${1}", this.tenantHost, key);
            return this.controller.HttpContext.Cache.Get(myKey);
        }

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void set(string key, object value)
        {
            string myKey = string.Format("PhunCache${0}${1}", this.tenantHost, key);
            this.controller.HttpContext.Cache[myKey] = value;
        }
    }
}
