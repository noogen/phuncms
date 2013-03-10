namespace Phun.Tests
{
    using System;

    using Phun.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for resource path provider.
    /// </summary>
    [TestClass]
    public class ResourcePathProviderTests
    {
        /// <summary>
        /// Tests the get file return resource virtual file.
        /// </summary>
        [TestMethod]
        public void TestGetFileReturnResourceVirtualFile()
        {
            // Arrange
            var rvp = new ResourcePathProvider();
            rvp.Config = new PhunCmsConfigurationSection() { ResourceRoute = "BogusRoute" };

            // Act
            var result = rvp.GetFile("~/BogusRoute/blah");

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the file exists return true for resource route.
        /// </summary>
        [TestMethod]
        public void TestFileExistsReturnTrueForResourceRoute()
        {
            // Arrange
            var rvp = new ResourcePathProvider();
            rvp.Config = new PhunCmsConfigurationSection() { ResourceRoute = "BogusRoute" };

            // Act
            var result = rvp.FileExists("~/BogusRoute/blah");

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests the cache dependency return null for resource route.
        /// </summary>
        [TestMethod]
        public void TestCacheDependencyReturnNullForResourceRoute()
        {
            // Arrange
            var rvp = new ResourcePathProvider();
            rvp.Config = new PhunCmsConfigurationSection() { ResourceRoute = "BogusRoute" };

            // Act
            var result = rvp.GetCacheDependency("~/BogusRoute/blah", null, DateTime.Now);

            // Assert
            Assert.IsNull(result);
        }
    }
}