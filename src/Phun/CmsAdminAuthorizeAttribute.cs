namespace Phun
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Routing;

    /// <summary>
    /// Allow for authorizing of content edit.
    /// </summary>
    public class CmsAdminAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The util.
        /// </summary>
        private static readonly ResourcePathUtility util = new ResourcePathUtility();

        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="filterContext">The filter context, which encapsulates information for using <see cref="T:System.Web.Mvc.AuthorizeAttribute" />.</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var config = Bootstrapper.Config;
            this.PopulateRolesFromConfiguration(config, filterContext);
            base.OnAuthorization(filterContext);
        }

        /// <summary>
        /// Populates the roles from configuration.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="filterContext">The filter context.</param>
        protected internal virtual void PopulateRolesFromConfiguration(ICmsConfiguration config, AuthorizationContext filterContext)
        {
            this.Roles = config.AdminRoles;
            if (config.HostAuthorizations.Any())
            {
                var host = util.GetTenantHost(filterContext.HttpContext.Request.Url);
                var found = config.HostAuthorizations.FirstOrDefault(cfg => string.Compare(host, cfg.Key, StringComparison.OrdinalIgnoreCase) == 0);
                if (found != null)
                {
                    this.Roles = found.Value;
                }
                else
                {
                    // attempt to resolve with wildcard
                    foreach (var auth in config.HostAuthorizations.Where(ha => ha.Key.Contains("*")))
                    {
                        var endHost = auth.Key.Replace("*", string.Empty);
                        if (host.EndsWith(endHost, StringComparison.OrdinalIgnoreCase))
                        {
                            this.Roles = auth.Value;
                            break;
                        }
                    }
                }
            }
        }
    }
}
