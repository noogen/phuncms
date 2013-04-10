using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Phun.Templating
{
    using Phun.Configuration;

    /// <summary>
    /// Template cache
    /// </summary>
    public class TemplateCache
    {
        /// <summary>
        /// The API
        /// </summary>                  
        protected internal IPhunApi api;

        /// <summary>
        /// The context
        /// </summary>                            
        protected internal HttpContextBase context;

        /// <summary>
        /// The config
        /// </summary>
        protected internal ICmsConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateCache" /> class.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="context">The context.</param>
        public TemplateCache(IPhunApi api, HttpContextBase context)
        {
            this.api = api;
            this.context = context;
            this.config = Bootstrapper.Config;
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string Get(string key)
        {
            var resultKey = string.Format("__PhunTemplateCache__{0}__{1}__{2}", this.api.tenantHost, this.api.FileModel.Path, key);
            var result = this.context.Cache[resultKey] as string;
            return result;
        }

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string set(string key, string value)
        {
            var resultKey = string.Format("__PhunTemplateCache__{0}__{1}__{2}", this.api.tenantHost, this.api.FileModel.Path, key);

            // implement sliding expiration for content cache to provide some performance and/or DOS attack
            this.context.Cache.Insert(
                resultKey, value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(this.config.CacheDuration));

            return value;
        }
    }
}
