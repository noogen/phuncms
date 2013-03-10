namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;

    /// <summary>
    /// Class for MVC web initialization.
    /// </summary>
    public static class PhunCmsBootstrapper
    {
        /// <summary>
        /// Initializes this instance using web.config.
        /// </summary>
        /// <param name="routeMissingUrlToPhunCms">if set to <c>true</c> [route missing URL to simple CMS].</param>
        /// <param name="registerPhunCmsVirtualPath">if set to <c>true</c> [register simple CMS virtual path].</param>
        public static void Initialize(bool routeMissingUrlToPhunCms = true, bool registerPhunCmsVirtualPath = true)
        {
            var config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;

            if (routeMissingUrlToPhunCms)
            {
                // overwrite default mvc route handler with our own
                // basically, whenever a route does not exist search our content and render if it exists
                // user can override content by creating their own controller and route
                foreach (
                    var r in
                        RouteTable.Routes.Cast<Route>()
                                  .Where(route => route.RouteHandler == null || route.RouteHandler is MvcRouteHandler))
                {
                    r.RouteHandler = new PhunCmsMvcRouteHandler();
                }
            }

            foreach (var routeMap in config.ContentMaps)
            {                          
                // insert content routing
                // insert regular content routing
                RouteTable.Routes.Insert(
                                    0,
                                    new Route(
                                        routeMap.RouteNormalized + "/{action}",
                                        new RouteValueDictionary(new { controller = routeMap.Controller, action = "Retrieve" }),
                                                                   new MvcRouteHandler()));

                // make download url friendly 
                RouteTable.Routes.Insert(
                                    0,
                                    new Route(
                                        routeMap.RouteNormalized + "/Download/{*path}",
                                        new RouteValueDictionary(new { controller = routeMap.Controller, action = "Download", path = UrlParameter.Optional }),
                                                                   new MvcRouteHandler()));
            }

            // handle embedded resources route 
            if (registerPhunCmsVirtualPath)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new ResourcePathProvider());
            }
        }
    }
}
