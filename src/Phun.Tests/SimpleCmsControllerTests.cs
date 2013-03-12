namespace Phun.Tests.StorageStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
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
    public class PhunCmsControllerTests
    {
        /// <summary>
        /// Tests the controller create call repository save.
        /// </summary>
        [TestMethod]
        public void TestControllerCreateCallRepositorySave()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object); 
            repo.Setup(rp => rp.Save(It.IsAny<ContentModel>())).Verifiable();
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.Create("/blah", "blah", "blah") as RedirectResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the controller update return JSON result.
        /// </summary>
        [TestMethod]
        public void TestControllerUpdateReturnJsonResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Save(It.IsAny<ContentModel>())).Verifiable();
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.Update("/blah", "blah", null) as JsonResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the controller delete call repository remove.
        /// </summary>
        [TestMethod]
        public void TestControllerDeleteCallRepositoryRemove()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Remove(It.IsAny<ContentModel>())).Verifiable();
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.Delete("~/blah") as JsonResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the download throw exception when no data returns.
        /// </summary>
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestDownloadThrowExceptionWhenNoDataReturns()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Retrieve(It.IsAny<ContentModel>())).Returns(new ContentModel());
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            controller.Download("~/blah");
        }

        /// <summary>
        /// Tests the download returns file result.
        /// </summary>
        [TestMethod]
        public void TestDownloadReturnsFileResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Retrieve(It.IsAny<ContentModel>()))
                .Returns(new ContentModel() { Data = new byte[] { 1, 1 } });
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var file = controller.Download("~/blah") as FileResult;

            // Assert
            Assert.IsNotNull(file);
        }

        /// <summary>
        /// Tests the edit return resource edit view result.
        /// </summary>
        [TestMethod]
        public void TestEditReturnResourceEditViewResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();

            // Act
            var result = controller.Edit("~/blah") as ViewResult;

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
            var controller = new PhunCmsContentController();

            // Act
            var result = controller.FileManager() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the file browser post save file and return resource view result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerPostSaveFileAndReturnResourceViewResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var httpPostFile = new Mock<HttpPostedFileBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Save(It.IsAny<ContentModel>())).Verifiable();
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            httpPostFile.Setup(pf => pf.FileName).Returns("test.txt");
            httpPostFile.Setup(pf => pf.InputStream).Returns(new MemoryStream(new byte[] { 1, 1 }));

            // Act
            var result = controller.FileManager(httpPostFile.Object, "~/blah") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            repo.VerifyAll();
        }

        /// <summary>
        /// Tests the file browser DYNATREE returns multiple result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerDynatreeReturnsMultipleResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeResult = new List<ContentModel>();
            fakeResult.Add(new ContentModel() { Path = "/ab" }); 
            fakeResult.Add(new ContentModel() { Path = "/cd" });

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.List(It.IsAny<ContentModel>())).Returns(fakeResult);
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.FileManagerDynatree("/blah") as JsonResult;
            
            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);

            var data = result.Data as List<DynaTreeViewModel>;
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count);
        }

        /// <summary>
        /// Tests the file browser DYNATREE of root path return A single result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerDynatreeOfRootPathReturnASingleResult()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeResult = new List<ContentModel>();
            fakeResult.Add(new ContentModel() { Path = "/ab" });
            fakeResult.Add(new ContentModel() { Path = "/cd" });

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.List(It.IsAny<ContentModel>())).Returns(fakeResult);
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.FileManagerDynatree("/") as JsonResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);

            var data = result.Data as DynaTreeViewModel;
            Assert.IsNotNull(data);
        }

        /// <summary>
        /// Tests the page display method return bundle resources.
        /// </summary>
        [TestMethod]
        public void TestPageDisplayMethodReturnBundleResources()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Retrieve(It.IsAny<ContentModel>())).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes("<html><head></head></html>") });
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.Page() as ContentResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Content.Contains("<script type='text/javascript' src='/PhunCms/Scripts/jquery.js'></script>"));
        }

        /// <summary>
        /// Tests the retrieve with template content result in content replacement.
        /// </summary>
        [TestMethod]
        public void TestRetrieveWithTemplateContentResultInContentReplacement()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var repo = new Mock<IContentRepository>();
            var contentConfig = new Mock<MapRouteConfiguration>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/blah"));
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            repo.Setup(rp => rp.Retrieve(It.Is<ContentModel>(m => m.Path == "/test"))).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes("<html><head>%ReplaceMe%</head>%AndMe%</html>") });
            contentConfig.Setup(cf => cf.ContentRepository).Returns(repo.Object);
            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            controller.MyContentConfig = contentConfig.Object;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            repo.Setup(rp => rp.Retrieve(It.Is<ContentModel>(m => m.Path == "/ReplaceMe"))).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes("woohooo") });
            repo.Setup(rp => rp.Retrieve(It.Is<ContentModel>(m => m.Path == "/AndMe"))).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes("metoo") });
            
            // Act
            var result = controller.Retrieve("/test") as ContentResult;

            // Assert
            repo.VerifyAll();
            Assert.IsNotNull(result);
            Assert.AreEqual("<html><head>woohooo</head>metoo</html>", result.Content);
        }

        /// <summary>
        /// Tests the get current host support multi tenant two levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentHostSupportMultiTenantForTwoLevelDomain()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var contentConfig = new Mock<MapRouteConfiguration>();

            contentConfig.Setup(cf => cf.DomainLevel).Returns(2);
            controller.MyContentConfig = contentConfig.Object;

            // Act
            var result = controller.GetCurrentHost(
                controller.MyContentConfig, new Uri("http://www.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("cha.com", result);
        }

        /// <summary>
        /// Tests the get current host support multi tenant for three levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentHostSupportMultiTenantForThreeLevelsDomain()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var contentConfig = new Mock<MapRouteConfiguration>();

            contentConfig.Setup(cf => cf.DomainLevel).Returns(3);
            controller.MyContentConfig = contentConfig.Object;

            // Act
            var result = controller.GetCurrentHost(
                controller.MyContentConfig, new Uri("http://www.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("youbet.cha.com", result);
        }

        /// <summary>
        /// Tests the get current host support multi tenant for N levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentHostSupportMultiTenantForNLevelsDomain()
        {
            // Arrange
            var controller = new PhunCmsContentController();
            var contentConfig = new Mock<MapRouteConfiguration>();

            contentConfig.Setup(cf => cf.DomainLevel).Returns(1);
            controller.MyContentConfig = contentConfig.Object;

            // Act
            var result = controller.GetCurrentHost(
                controller.MyContentConfig, new Uri("http://www.haha.blahblah.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("haha.blahblah.youbet.cha.com", result);
        }
    }
}