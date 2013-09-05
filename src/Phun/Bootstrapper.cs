namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;

    /// <summary>
    /// Class for MVC web initialization.
    /// </summary>
    public class Bootstrapper : Singleton<Bootstrapper>
    {
        /// <summary>
        /// The has initialized
        /// </summary>
        internal bool hasInitialized = false;

        /// <summary>
        /// The API list
        /// </summary>
        internal IDictionary<string, Type> ApiList =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The API scripts
        /// </summary>
        internal IDictionary<string, string> ApiScripts =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The internal static content regular expression
        /// </summary>
        internal Regex internalContentRegEx = null;

        /// <summary>
        /// The host aliases
        /// </summary>
        internal IDictionary<string, string> HostAliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public ICmsConfiguration Config { get; internal set; }

        /// <summary>
        /// Gets the content config.
        /// </summary>
        /// <value>
        /// The content config.
        /// </value>
        public IMapRouteConfiguration ContentConfig { get; internal set; }

        /// <summary>
        /// Gets or sets the static content regular expression.
        /// </summary>
        /// <value>
        /// The static content regular expression.
        /// </value>
        public Regex ContentRegEx
        {
            get
            {
                if (this.internalContentRegEx == null)
                {
                    this.internalContentRegEx = new Regex("\\.(" + this.Config.StaticContentExtension + ")+$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                }

                return this.internalContentRegEx;
            }
        }

        /// <summary>
        /// Registers the template API.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="objectType">Type of the object.</param>
        public void RegisterRequireJsModule(string name, Type objectType)
        {
            if (!this.ApiList.ContainsKey(name))
            {
                this.ApiList.Add(name, objectType);
            }
            else
            {
                this.ApiList[name] = objectType;
            }
        }

        /// <summary>
        /// Registers the template API.
        /// </summary>
        /// <typeparam name="T">API object type to register.</typeparam>
        /// <param name="name">The name.</param>
        public void RegisterRequireJsModule<T>(string name)
        {
            this.RegisterRequireJsModule(name, typeof(T));
        }

        /// <summary>
        /// Registers additional JavaScript to load.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="script">The script.</param>
        public void RegisterApiScript(string name, string script)
        {
            if (this.ApiScripts.ContainsKey(name))
            {
                this.ApiScripts.Add(name, script);
            }
            else
            {
                this.ApiScripts[name] = script;
            }
        }


        /// <summary>
        /// Initializes this instance using web.config.
        /// </summary>
        /// <param name="routeMissingUrlToPhunCms">if set to <c>true</c> [route missing URL to simple CMS].</param>
        /// <param name="registerPhunCmsVirtualPath">if set to <c>true</c> [register simple CMS virtual path].</param>
        /// <exception cref="System.ApplicationException">Unable to locate phun CMS configuration.</exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public void Initialize(bool routeMissingUrlToPhunCms = true, bool registerPhunCmsVirtualPath = true)
        {
            if (this.hasInitialized)
            {
                return;
            }

            this.hasInitialized = true;

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

            this.Config = config;
            this.ContentConfig =
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

            foreach (var hostAlias in config.HostAliases)
            {
                HostAliases.Add(hostAlias.Key, hostAlias.Value);
            }

            // handle embedded resources route 
            if (registerPhunCmsVirtualPath)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new ResourceVirtualPathProvider());
            }

            if (ControllerBuilder.Current != null)
            {
                // only replace controller factory if this is not an ioc factory
                var controllerFactory = ControllerBuilder.Current.GetControllerFactory();
                if (controllerFactory is DefaultControllerFactory)
                {
                    ControllerBuilder.Current.SetControllerFactory(new ControllerFactoryWrapper());
                }
            }
        }
    }
}
