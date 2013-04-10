using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phun.Routing
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Wrapper class for controller factory.
    /// </summary>
    public class ControllerFactoryWrapper : DefaultControllerFactory
    {
        /// <summary>
        /// Finds the controller.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>The controller type.</returns>
        public Type FindController(RequestContext requestContext, string controllerName)
        {
            return this.GetControllerType(requestContext, controllerName);
        }
    }
}
