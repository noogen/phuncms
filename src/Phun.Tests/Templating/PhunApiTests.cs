namespace Phun.Tests.Templating
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;
    using Phun.Templating;

    /// <summary>
    /// Container for Phun API tests.
    /// </summary>
    [TestClass]
    public class PhunApiTests
    {
        /// <summary>
        /// Tests the phun API bundles call utility render bundles.
        /// </summary>
        [TestMethod]
        public void TestPhunApiBundlesCallUtilityRenderBundles()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunCmsRenderBundles(true, true, true, true, true)).Verifiable();
                                    
            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            api.bundles();

            // Assert
            fakeUtility.VerifyAll();
        }

        /// <summary>
        /// Tests the phun API resource URL call utility get resource path.
        /// </summary>
        [TestMethod]
        public void TestPhunApiResourceUrlCallUtilityGetResourcePath()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeUtility.Setup(u => u.GetResourcePath("/testz")).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            api.resourceUrl("/testz");

            // Assert
            fakeUtility.VerifyAll();
        }

        /// <summary>
        /// Tests the phun API content URL returns page download path.
        /// </summary>
        [TestMethod]
        public void TestPhunApiPageContentUrlReturnsPageDownloadPath()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var expected = "/asdf/download/page/testz";

            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeConfig.Setup(cf => cf.ContentRouteNormalized).Returns("asdf");
            fakeUtility.Setup(u => u.Config).Returns(fakeConfig.Object);

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            var result = api.pageContentUrl("/testz");

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests the phun API content URL returns page download path.
        /// </summary>
        [TestMethod]
        public void TestPhunApiContentUrlReturnsContentDownloadPath()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var expected = "/asdf/download/content/testz";

            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeConfig.Setup(cf => cf.ContentRouteNormalized).Returns("asdf");
            fakeUtility.Setup(u => u.Config).Returns(fakeConfig.Object);

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            var result = api.contentUrl("/testz");

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests the phun API partial call utility phun partial.
        /// </summary>
        [TestMethod]
        public void TestPhunApiPartialCallUtilityPhunPartial()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunPartial(It.IsAny<string>(), It.IsAny<Uri>())).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            api.partial("test");

            // Assert
            fakeUtility.VerifyAll();
        }

        /// <summary>
        /// Tests the phun API partial editable call utility phun partial editable.
        /// </summary>
        [TestMethod]
        public void TestPhunApiPartialEditableCallUtilityPhunPartialEditable()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunPartialEditable(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;

            // Act
            api.partialEditable("test", "test", new { title = "test" });

            // Assert
            fakeUtility.VerifyAll();
        }

        /// <summary>
        /// Tests the phun API tenant host return internal property.
        /// </summary>
        [TestMethod]
        public void TestPhunApiTenantHostReturnInternalProperty()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var expected = "testing";

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunPartialEditable(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;
            api.host = expected;

            // Act
            var result = api.tenantHost;

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests the phun API require return interal properties.
        /// </summary>
        [TestMethod]
        public void TestPhunApiRequireReturnInteralProperties()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var expectedFileSystem = new Mock<IFileSystem>();
            var expectedPath = new Mock<IPath>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunPartialEditable(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;
            api.phunFileSystem = expectedFileSystem.Object;
            api.phunPath = expectedPath.Object;

            // Act
            // Assert
            Assert.AreEqual(expectedFileSystem.Object, api.require("fs"));
            Assert.AreEqual(expectedPath.Object, api.require("path"));
            Assert.AreEqual(null, api.require("unknown"));
        }

        [TestMethod]
        public void TestPhunApiRequireReturnAnInstanceOfRegisteredApiObject()
        {
            // Arrange 
            var connector = new ContentConnector();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();

            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testz"));
            mockRequest.Setup(rq => rq.Path).Returns("/testz");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            fakeUtility.Setup(u => u.PhunPartialEditable(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable();

            var api = new PhunApi(context.Object, connector);
            api.utility = fakeUtility.Object;
            Bootstrapper.Default.RegisterRequireJsModule<ContentConnector>("connector");

            // Act
            var result = api.require("connector");
            Bootstrapper.Default.ApiList.Clear();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ContentConnector));
        }
    }
}
