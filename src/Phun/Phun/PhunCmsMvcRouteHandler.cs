namespace Phun
{
    using System.Configuration;
    using System.Linq;
    using System.Web.Routing;

    using Phun.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// For handling mvc phuncms 404 routes.
    /// </summary>
    public class PhunCmsMvcRouteHandler : MvcRouteHandler
    {
        /// <summary>
        /// The controller exists
        /// </summary>
        private static readonly Dictionary<string, bool> controllerExists = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The content controller name
        /// </summary>
        private static string contentControllerName = string.Empty;

        /// <summary>
        /// Gets or sets the controller builder.
        /// </summary>
        /// <value>
        /// The controller builder.
        /// </value>
        internal ControllerBuilder ControllerBuilder { get; set; }

        /// <summary>
        /// Returns the HTTP handler by using the specified HTTP context.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        protected internal virtual void PreHandleRequestInternal(System.Web.Routing.RequestContext requestContext)
        {
            // keep a list of registered controller.
            var controllerName = requestContext.RouteData.Values["controller"] + string.Empty;
            var controllerBuilder = this.ControllerBuilder ?? ControllerBuilder.Current;
            var controllerFactory = controllerBuilder.GetControllerFactory();
            if (!string.IsNullOrEmpty(controllerName) && !controllerExists.ContainsKey(controllerName))
            {
                try
                {
                    var controller = controllerFactory.CreateController(requestContext, controllerName);
                    if (!controllerExists.ContainsKey(controllerName))
                    {
                        controllerExists.Add(controllerName, controller != null);
                    }
                }
                catch
                {
                    // controller does not exists
                    if (!controllerExists.ContainsKey(controllerName))
                    {
                        controllerExists.Add(controllerName, false);
                    }
                }
            }

            // if request controller or route is not in the list, assuming mvc convention of {controller} parameter
            if (!controllerExists.ContainsKey(controllerName) || (controllerExists.ContainsKey(controllerName) && !controllerExists[controllerName]))
            {
                // attempt to get content controller info
                if (string.IsNullOrEmpty(contentControllerName))
                {
                    var config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
                    var routeController = config.RouteNormalized + "/";

                    var route =
                        RouteTable.Routes.Cast<Route>()
                                  .FirstOrDefault(
                                      r => r.Url.StartsWith(routeController, StringComparison.OrdinalIgnoreCase));

                    if (route != null)
                    {
                        contentControllerName = route.Defaults["controller"] + string.Empty;
                    }
                }

                // redirect to content route
                requestContext.RouteData.Values["controller"] = contentControllerName;
                requestContext.RouteData.Values["action"] = "Page";

                // fallback to base handler once we updated the controller and action
            }
        }

        /// <summary>
        /// Returns the HTTP handler by using the specified HTTP context.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <returns>
        /// The HTTP handler.
        /// </returns>
        protected override System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            this.PreHandleRequestInternal(requestContext);
            return base.GetHttpHandler(requestContext);
        }
    }
}
