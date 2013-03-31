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
    /// Container for cache tests.
    /// </summary>
    [TestClass]
    public class PhunCacheTests
    {
        /// <summary>
        /// Tests the phun cache can get and set value from cache.
        /// </summary>
        [TestMethod]
        public void TestPhunCacheCanGetAndSetValueFromCache()
        {
            // Arrange
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(false);
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testfailed"));
            mockRequest.Setup(rq => rq.Path).Returns("/testfailed");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            context.Setup(c => c.Cache).Returns(HttpRuntime.Cache);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);

            // Act                               
            var cache = new PhunCache(context.Object);
            var expected = new ContentConnector();
            var initialCount = HttpRuntime.Cache.Count;
            cache.set("connector", expected);
            var afterCount = HttpRuntime.Cache.Count;

            // Assert
            Assert.AreEqual(initialCount + 1, afterCount);
            var result = cache.get("connector");
            Assert.AreEqual(expected, result);
        }
    }
}
