namespace Phun.Tests
{
    using System;
    using System.Web;

    using Phun.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Phun.Extensions;

    /// <summary>
    /// Unit tests for resource virtual file
    /// </summary>
    [TestClass]
    public class ResourceVirtualFileTests
    {
        /// <summary>
        /// Tests the render bundles with no parameters return valid path.
        /// </summary>
        [TestMethod]
        public void TestRenderBundlesWithNoParametersReturnValidPath()
        {
            // Arrange
            var rvf = new ResourceVirtualFile("~/blah", string.Empty);
            rvf.Config = new PhunCmsConfigurationSection() { ResourceRoute = "BogusRoute" };
            var expected = "<script type='text/javascript' src='/BogusRoute/Scripts/jquery.js'></script>";

            // Act
            var result = rvf.PhunCmsRenderBundles();

            // Assert
            Assert.IsTrue(result.Contains(expected));
            Assert.IsTrue(result.Contains("PhunCms.initEditor()"));
        }

        /// <summary>
        /// Tests the render bundles does not render editor initialize.
        /// </summary>
        [TestMethod]
        public void TestRenderBundlesDoesNotRenderEditorInitialize()
        {
            // Arrange
            var rvf = new ResourceVirtualFile("~/blah", string.Empty);
            rvf.Config = new PhunCmsConfigurationSection() { ResourceRoute = "BogusRoute" };
            var expected = "<script type='text/javascript' src='/BogusRoute/Scripts/jquery.js'></script>";

            // Act
            var result = rvf.PhunCmsRenderBundles(includeEditorInit: false);

            // Assert
            Assert.IsTrue(result.Contains(expected));
            Assert.IsFalse(result.Contains("PhunCms.initEditor()"));
        }

        /// <summary>
        /// Tests the open return static resource virtual file.
        /// </summary>
        [TestMethod]
        public void TestOpenReturnStaticResourceVirtualFile()
        {
            // Arrange
            var rvf = new ResourceVirtualFile("~/blah", "Phun.Properties.scripts.phuncms.config.js");

            // Act
            using (var result = rvf.Open())
            {
                var data = System.Text.Encoding.ASCII.GetString(result.ReadAll());
                
                // Assert
                Assert.IsTrue(data.Contains("window.PhunCms"));
            }
        }

        /// <summary>
        /// Tests the open throw exception for null result.
        /// </summary>
        [TestMethod, ExpectedException(typeof(HttpException))]
        public void TestOpenThrowExceptionForNullResult()
        {
            // Arrange
            var rvf = new ResourceVirtualFile("~/blah", "blah.js");

            // Act
            rvf.Open();
        }

        /// <summary>
        /// Tests the open return A valid stream.
        /// </summary>
        [TestMethod]
        public void TestOpenReturnAValidStream()
        {
            // Arrange
            var rvf = new ResourceVirtualFile("~/blah", "Phun.Properties.scripts.jquery.js");

            // Act
            var stream = rvf.Open();
            
            // Assert
            Assert.IsNotNull(stream);
        }
    }
}