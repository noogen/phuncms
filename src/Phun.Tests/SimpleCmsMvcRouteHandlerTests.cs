//namespace Phun.Tests
//{
//    using System;
//    using System.Web;
//    using System.Web.Mvc;
//    using System.Web.Routing;
//    using System.Web.SessionState;

//    using Microsoft.VisualStudio.TestTools.UnitTesting;
//    using Moq;
//    using Moq.Protected;

//    using Phun.Routing;

//    /// <summary>
//    /// PhunCmsMvcRouteHandler Tests
//    /// </summary>
//    [TestClass]
//    public class PhunCmsMvcRouteHandlerTests
//    {
//        /// <summary>
//        /// Gets the HTTP handler with invalid controller redirect to simple CMS test.
//        /// </summary>
//        [TestMethod]
//        public void GetHttpHandlerWithInvalidControllerRedirectToPhunCmsTest()
//        {
//            // Arrange
//            var mockController = new Mock<ControllerBase>(MockBehavior.Strict);
//            var expected = "unknownController";

//            var builder = new ControllerBuilder();
//            builder.SetControllerFactory(new SimpleControllerFactory(mockController.Object));

//            var contextMock = new Mock<HttpContextBase>();
//            var requestContext = new RequestContext(contextMock.Object, new RouteData());
//            requestContext.RouteData.Values["controller"] = expected;

//            // act
//            var handler = new PhunCmsMvcRouteHandler() { ControllerBuilder = builder };
//            handler.PreHandleRequestInternal(requestContext);

//            // Assert
//            Assert.AreNotEqual(expected, requestContext.RouteData.Values["controller"]);
//        }

//        /// <summary>
//        /// Gets the HTTP handler with valid controller does not change route test.
//        /// </summary>
//        [TestMethod]
//        public void GetHttpHandlerWithValidControllerDoesNotChangeRouteTest()
//        {
//            // Arrange
//            var mockController = new Mock<ControllerBase>(MockBehavior.Strict);
//            var expected = "fooController";

//            var builder = new ControllerBuilder();
//            builder.SetControllerFactory(new SimpleControllerFactory(mockController.Object));

//            var contextMock = new Mock<HttpContextBase>();
//            var requestContext = new RequestContext(contextMock.Object, new RouteData());
//            requestContext.RouteData.Values["controller"] = expected;

//            // act
//            var handler = new PhunCmsMvcRouteHandler() { ControllerBuilder = builder };
//            handler.PreHandleRequestInternal(requestContext);

//            // Assert
//            Assert.AreEqual(expected, requestContext.RouteData.Values["controller"]);
//        }
//        private class SimpleControllerFactory : IControllerFactory
//        {
//            private IController instance;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="SimpleControllerFactory"/> class.
//            /// </summary>
//            /// <param name="instance">The instance.</param>
//            public SimpleControllerFactory(IController instance)
//            {
//                this.instance = instance;
//            }

//            /// <summary>
//            /// Creates the controller.
//            /// </summary>
//            /// <param name="context">The context.</param>
//            /// <param name="controllerName">Name of the controller.</param>
//            /// <returns></returns>
//            public IController CreateController(RequestContext context, string controllerName)
//            {
//                return (controllerName == "fooController") ? this.instance : null;
//            }

//            /// <summary>
//            /// Releases the specified controller.
//            /// </summary>
//            /// <param name="controller">The controller.</param>
//            public void ReleaseController(IController controller)
//            {
//                IDisposable disposable = controller as IDisposable;
//                if (disposable != null)
//                {
//                    disposable.Dispose();
//                }
//            }

//            /// <summary>
//            /// Gets the controller's session behavior.
//            /// </summary>
//            /// <param name="requestContext">The request context.</param>
//            /// <param name="controllerName">The name of the controller whose session behavior you want to get.</param>
//            /// <returns>
//            /// The controller's session behavior.
//            /// </returns>
//            public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
//            {
//                return new SessionStateBehavior();
//            }
//        }
//    }
//}
