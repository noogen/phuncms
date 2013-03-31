namespace Phun.Tests.Templating
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;
    using Phun.Templating;

    /// <summary>
    /// Container for template handler unit tests.
    /// </summary>
    [TestClass]
    public class TemplateHandlerTests
    {
        /// <summary>
        /// Tests the can render vash file.
        /// </summary>
        [TestMethod]
        public void TestCanRenderVashFile()
        {
            // Arrange
            var th = new TemplateHandler();
            var expected = true;

            // Act
            var result = th.CanRender(new ContentModel() { Path = "/test.Vash" });

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests the render execute javascript engine.
        /// </summary>
        [TestMethod]
        public void TestRenderExecuteJavascriptEngine()
        {
            // Arrange
            var connector = new Mock<IContentConnector>();
            var fakeUtility = new Mock<ResourcePathUtility>();
            var fakeRepository = new Mock<IContentRepository>();
            var mockRequest = new Mock<HttpRequestBase>();
            var context = new Mock<HttpContextBase>();
            var fakeConfig = new Mock<ICmsConfiguration>();
            var fakeResponse = new Mock<HttpResponseBase>();
            var fakeFile = new Mock<ResourceVirtualFile>();
            var fakeContentModel = new ContentModel() { Data = System.Text.Encoding.UTF8.GetBytes("Hello world."), DataLength = 12 };
            var handler = new TemplateHandler();

            fakeUtility.Setup(u => u.GetTenantHost(It.IsAny<Uri>())).Returns("localhost");
            fakeUtility.Setup(u => u.GetResourcePath(It.IsAny<string>())).Returns(string.Empty);
            fakeFile.Setup(f => f.Open())
                    .Returns(
                        new MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(
                                "module.exports['helpers'] = { constructor: { reportError: function() {} }}; module.exports['renderFile'] = function() {};")));
            fakeConfig.Setup(cf => cf.IsResourceRoute(It.IsAny<string>())).Returns(false);
            fakeConfig.Setup(c => c.GetResourceFile(It.IsAny<string>())).Returns(fakeFile.Object);
            fakeUtility.Setup(c => c.Config).Returns(fakeConfig.Object);
            fakeRepository.Setup(r => r.Exists(It.IsAny<ContentModel>())).Returns(true);
            fakeRepository.Setup(r => r.Retrieve(It.IsAny<ContentModel>(), It.IsAny<bool>())).Returns(fakeContentModel);
            connector.Setup(r => r.ContentRepository).Returns(fakeRepository.Object);
            mockRequest.Setup(rq => rq.Url).Returns(new Uri("http://localhost/testrender.vash"));
            mockRequest.Setup(rq => rq.Path).Returns("/testrender.vash");
            context.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            mockRequest.Setup(rq => rq.QueryString).Returns(new NameValueCollection());
            fakeResponse.Setup(r => r.End()).Verifiable();
            context.Setup(c => c.Response).Returns(fakeResponse.Object);
            handler.Utility = fakeUtility.Object;
            Bootstrapper.RegisterApiScript("test", "var a = 1;");

            // Act
            handler.Render(new ContentModel() { Path = "Fake.vash" }, connector.Object, context.Object );

            // Assert
            fakeResponse.VerifyAll();
        }
    }
}
