namespace Phun.Tests.StorageStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Unit tests for Simple Cms Controller / Content Controller
    /// </summary>
    [TestClass]
    public class CmsContentControllerTests
    {
        /// <summary>
        /// Tests the edit return resource edit view result.
        /// </summary>
        [TestMethod]
        public void TestEditReturnResourceEditViewResult()
        {
            // Arrange
            var controller = new CmsContentController();

            // Act
            var result = controller.Edit("~/blah") as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the file browser return resource view result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerReturnResourceViewResult()
        {
            // Arrange
            var controller = new CmsContentController();

            // Act
            var result = controller.FileManager() as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the file browser DYNATREE of root path return A single result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerDynatreeOfRootPathReturnASingleResult()
        {
            // Arrange
            var controller = new CmsContentController();
            var repo = new Mock<IContentConnector>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeResult = new List<ContentModel>();
            fakeResult.Add(new ContentModel() { Path = "/ab" });
            fakeResult.Add(new ContentModel() { Path = "/cd" });

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.List(It.IsAny<string>(), It.IsAny<Uri>())).Returns(fakeResult.AsQueryable());
            controller.ContentConnector = repo.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.FileManagerDynatree("/") as JsonResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);

            var data = result.Data as DynaTreeViewModel;
            Assert.IsNotNull(data);
        }

    }
}