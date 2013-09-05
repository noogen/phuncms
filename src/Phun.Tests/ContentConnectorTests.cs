namespace Phun.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;
    using Phun.Templating;

    /// <summary>
    /// Container for all content connector tests.
    /// </summary>
    [TestClass]
    public class ContentConnectorTests
    {
        /// <summary>
        /// Tests the content of the create return error for existing.
        /// </summary>
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestCreateReturnErrorForExistingContent()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            connector.Create("/test", string.Empty, new Uri("http://localhost/test"));
        }

        /// <summary>
        /// Tests the retrieve folder return A zip file.
        /// </summary>
        [TestMethod]
        public void TestRetrieveFolderReturnAZipFile()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeRepository.Setup(r => r.GetFolder(It.IsAny<ContentModel>())).Returns(System.IO.Path.GetTempFileName());
            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            var result = connector.Retrieve("/test/", new Uri("http://localhost/test"));

            // Assert
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Path.EndsWith(".zip"));
        }

        /// <summary>
        /// Tests the retrieve throws not found exception.
        /// </summary>
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestRetrieveThrowsNotFoundException()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeRepository.Setup(r => r.Retrieve(It.IsAny<ContentModel>(), It.IsAny<bool>())).Returns(new ContentModel());
            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.Normalize(It.IsAny<string>())).Returns("/test");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            connector.Retrieve("/test", new Uri("http://localhost/test"));
        }

        /// <summary>
        /// Tests the content of the retrieve returns valid.
        /// </summary>
        [TestMethod]
        public void TestRetrieveReturnsValidContent()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var expected = 100;

            fakeRepository.Setup(r => r.Retrieve(It.IsAny<ContentModel>(), It.IsAny<bool>())).Returns(new ContentModel() { DataLength = expected });
            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.Normalize(It.IsAny<string>())).Returns("/test");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            var result = connector.Retrieve("/test", new Uri("http://localhost/test"));
            
            // Assert
            Assert.AreEqual(expected, result.DataLength);
        }

        /// <summary>
        /// Tests the create or update can create empty file.
        /// </summary>
        [TestMethod]
        public void TestCreateOrUpdateCanCreateEmptyFile()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeRepository.Setup(r => r.Save(It.Is<ContentModel>(cm => cm.Data == null))).Verifiable();
            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.Normalize(It.IsAny<string>())).Returns("/test");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            var result = connector.CreateOrUpdate("/test", null, new Uri("http://localhost/test"));

            // Assert
            fakeRepository.VerifyAll();
            Assert.IsNull(result.Data);
        }

        /// <summary>
        /// Tests the create or update throws root path error.
        /// </summary>      
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestCreateOrUpdateThrowsRootPathError()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            connector.CreateOrUpdate(null, null, new Uri("http://localhost/test"));
        }

        /// <summary>
        /// Tests the delete throws root path exception.
        /// </summary>
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestDeleteThrowsRootPathException()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            connector.Delete(null, new Uri("http://localhost/test"));
        }

        /// <summary>
        /// Tests the delete successfully call repository remove.
        /// </summary>
        [TestMethod]
        public void TestDeleteSuccessfullyCallRepositoryRemove()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.Normalize(It.IsAny<string>())).Returns("/test");
            fakeRepository.Setup(r => r.Remove(It.IsAny<ContentModel>())).Verifiable();
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            connector.Delete("/test", new Uri("http://localhost/test"));

            // Assert
            fakeRepository.VerifyAll();
        }

        /// <summary>
        /// Tests the list returns queryable.
        /// </summary>
        [TestMethod]
        public void TestListReturnsQueryable()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.Normalize(It.IsAny<string>())).Returns("/test");
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            fakeRepository.Setup(r => r.List(It.IsAny<ContentModel>()))
                          .Returns(new List<ContentModel>().AsQueryable());
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;

            // Act
            var result = connector.List("/test", new Uri("http://localhost/test"));

            // Assert
            Assert.IsNotNull(result);
        }


        /// <summary>
        /// Tests the render page render resource.
        /// </summary>
        [TestMethod]
        public void TestRenderPageRenderResource()
        {
            // Arrange
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var fakeResourceFile = new Mock<ResourceVirtualFile>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            connector.ContentRepository = fakeRepository.Object;
            connector.PathUtility = fakeUtility.Object;
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/test"));
            mockRequest.Setup(rq => rq.Path).Returns("/test");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(new Cache());
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeResourceFile.Setup(rf => rf.WriteFile(It.IsAny<HttpContextBase>())).Verifiable();
            fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(true);
            fakeConfig.Setup(cf => cf.GetResourceFile(It.IsAny<string>())).Returns(fakeResourceFile.Object);
            connector.Config = fakeConfig.Object;

            // Act
            connector.RenderPage(context.Object);

            // Assert
            fakeResourceFile.VerifyAll();
        }

        ///// <summary>
        ///// Tests the render page result in HTTP exception.
        ///// </summary>
        //[TestMethod, ExpectedException(typeof(HttpException))]
        //public void TestRenderPageResultInHttpException()
        //{
        //    // Arrange
        //    var connector = new ContentConnector();
        //    var fakeUtility = new Mock<ResourcePathUtility>();
        //    var fakeRepository = new Mock<IContentRepository>();
        //    var mockRequest = new Mock<HttpRequestBase>();
        //    var context = new Mock<HttpContextBase>();
        //    var fakeConfig = new Mock<ICmsConfiguration>();

        //    fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
        //    fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(false);
        //    connector.ContentRepository = fakeRepository.Object;
        //    connector.PathUtility = fakeUtility.Object;
        //    mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testfailed"));
        //    mockRequest.Setup(rq => rq.Path).Returns("/testfailed");
        //    context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
        //    context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
        //    mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
        //    fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);
        //    connector.Config = fakeConfig.Object;

        //    // Act
        //    connector.RenderPage(context.Object);
        //}

        ///// <summary>
        ///// Tests the render page with valid path calls template handler.
        ///// </summary>
        //[TestMethod]
        //public void TestRenderPageWithValidPathCallsTemplateHandler()
        //{
        //    // Arrange
        //    var connector = new ContentConnector();
        //    var fakeUtility = new Mock<ResourcePathUtility>();
        //    var fakeRepository = new Mock<IContentRepository>();
        //    var mockRequest = new Mock<HttpRequestBase>();
        //    var context = new Mock<HttpContextBase>();
        //    var fakeConfig = new Mock<ICmsConfiguration>();
        //    var fakeHandler = new Mock<ITemplateHandler>();

        //    fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
        //    fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
        //    connector.ContentRepository = fakeRepository.Object;
        //    connector.PathUtility = fakeUtility.Object;
        //    mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testgood"));
        //    mockRequest.Setup(rq => rq.Path).Returns("/testgood");
        //    context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
        //    context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
        //    mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
        //    fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);
        //    fakeHandler.Setup( h => h.Render(It.IsAny<ContentModel>(), It.IsAny<IContentConnector>(), It.IsAny<HttpContextBase>())).Verifiable();
        //    connector.TemplateHandler = fakeHandler.Object;
        //    connector.Config = fakeConfig.Object;

        //    // Act
        //    connector.RenderPage(context.Object);

        //    // Assert
        //    fakeHandler.VerifyAll();
        //}
    }
}
