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
    /// Container for file system tests.
    /// </summary>
    [TestClass]
    public class PhunFileSystemTests
    {
        /// <summary>
        /// Tests the file system read file sync auto add page path and vash extension.
        /// </summary>
        [TestMethod]
        public void TestFileSystemReadFileSyncAutoAddPagePathAndVashExtension()
        {
            // Arrange
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var fakeApi = new Mock<IPhunApi>();
            var fakeConnector = new Mock<IContentConnector>();
            var expected = "Helloooo nurse!!!";
            var expectedUrl = "/page/test.vash";

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            fakeConnector.Setup(r => r.Retrieve(It.IsAny<string>(), It.IsAny<Uri>())).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes(expected) });
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testfailed"));
            mockRequest.Setup(rq => rq.Path).Returns("/testfailed");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);
            fakeUtility.Setup(u => u.Normalize(expectedUrl)).Returns(expectedUrl);
            fakeConnector.Setup(c => c.ContentRepository).Returns(fakeRepository.Object);
            fakeApi.Setup(r => r.request).Returns(new PhunRequest(context.Object));

            // Act                               
            var fs = new PhunFileSystem(fakeApi.Object, fakeConnector.Object);
            fs.myUtility = fakeUtility.Object;
            var result = fs.readFileSync("/test", null);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests the file system read file sync search in shared for relative file.
        /// </summary>
        [TestMethod]
        public void TestFileSystemReadFileSyncSearchInSharedForRelativeFile()
        {
            // Arrange
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var fakeApi = new Mock<IPhunApi>();
            var fakeConnector = new Mock<IContentConnector>();
            var expected = "Helloooo nurse!!!";
            var expectedUrl = "/fake/test.vash";

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(false);
            fakeConnector.Setup(r => r.Retrieve("/page/shared/test.vash", It.IsAny<Uri>())).Returns(new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes(expected) });
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testfailed"));
            mockRequest.Setup(rq => rq.Path).Returns("/testfailed");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);
            fakeUtility.Setup(u => u.Normalize(expectedUrl)).Returns(expectedUrl);
            fakeConnector.Setup(c => c.ContentRepository).Returns(fakeRepository.Object);
            fakeApi.Setup(r => r.request).Returns(new PhunRequest(context.Object));
            fakeApi.Setup(r => r.FileModel.ParentPath).Returns("/fake/");

            // Act                               
            var fs = new PhunFileSystem(fakeApi.Object, fakeConnector.Object);
            fs.myUtility = fakeUtility.Object;
            var result = fs.readFileSync("test", null);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
