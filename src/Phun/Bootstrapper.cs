namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
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
    public static class Bootstrapper
    {
        /// <summary>
        /// The API list
        /// </summary>
        internal static IDictionary<string, Type> ApiList = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The API scripts
        /// </summary>
        internal static IDictionary<string, string> ApiScripts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
 
        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public static ICmsConfiguration Config { get; internal set; }

        /// <summary>
        /// Gets the content config.
        /// </summary>
        /// <value>
        /// The content config.
        /// </value>
        public static IMapRouteConfiguration ContentConfig { get; internal set; }

        /// <summary>
        /// Registers the template API.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="objectType">Type of the object.</param>
        public static void RegisterRequireModule(string name, Type objectType)
        {
            if (!ApiList.ContainsKey(name))
            {
                ApiList.Add(name, objectType);
            }
            else
            {
                ApiList[name] = objectType;
            }
        }

        /// <summary>
        /// Registers the template API.
        /// </summary>
        /// <typeparam name="T">API object type to register.</typeparam>
        /// <param name="name">The name.</param>
        public static void RegisterRequireModule<T>(string name)
        {
            RegisterRequireModule(name, typeof(T));
        }

        /// <summary>
        /// Registers additional JavaScript to load.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="script">The script.</param>
        public static void RegisterApiScript(string name, string script)
        {
            if (ApiScripts.ContainsKey(name))
            {
                ApiScripts.Add(name, script);
            }
            else
            {
                ApiScripts[name] = script;
            }
        }


        /// <summary>
        /// Initializes this instance using web.config.
        /// </summary>
        /// <param name="routeMissingUrlToPhunCms">if set to <c>true</c> [route missing URL to simple CMS].</param>
        /// <param name="registerPhunCmsVirtualPath">if set to <c>true</c> [register simple CMS virtual path].</param>
        /// <exception cref="System.ApplicationException">Unable to locate phun CMS configuration.</exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static void Initialize(bool routeMissingUrlToPhunCms = true, bool registerPhunCmsVirtualPath = true)
        {
            // attempt to resolve phuncms configuration;
            ICmsConfiguration config = null;

            var configType = ConfigurationManager.AppSettings["PhunCmsConfiguration"];
            if (!string.IsNullOrEmpty(configType))
            {
                var type = Type.GetType(configType);
                config = Activator.CreateInstance(type) as ICmsConfiguration;
            }

            if (config == null)
            {
                config = ConfigurationManager.GetSection("phuncms") as ICmsConfiguration;
            }

            if (config == null)
            {
                throw new ApplicationException("Unable to locate phuncms configuration.");
            }

            Config = config;
            ContentConfig =
                config.ContentMaps.FirstOrDefault(
                    m =>
                    string.Compare(m.RouteNormalized, config.ContentRouteNormalized, StringComparison.OrdinalIgnoreCase)
                    == 0);

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
                    r.RouteHandler = new PhunMvcRouteHandler();
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
                                        new RouteValueDictionary(new { controller = routeMap.Controller, action = "Retrieve", path = UrlParameter.Optional }),
                                                                   new MvcRouteHandler()));
            }

            // handle embedded resources route 
            if (registerPhunCmsVirtualPath)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new ResourceVirtualPathProvider());
            }
        }
    }
}
