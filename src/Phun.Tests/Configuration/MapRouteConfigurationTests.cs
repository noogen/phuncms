namespace Phun.Tests.Configuration
{
    using Phun.Configuration;
    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test content repository factory from configuration.
    /// </summary>
    [TestClass]
    public class MapRouteConfigurationTests
    {
        /// <summary>
        /// Tests the content repository return SQL repository.
        /// </summary>
        [TestMethod]
        public void TestContentRepositoryReturnSqlRepository()
        {
            // Arrange
            var config = new MapRouteConfiguration()
                             {
                                 RepositoryType = "sql",
                                 RepositorySource = "DefaultDatabase",
                                 RepositoryTable = "CmsContent"
                             };

            // Act
            var repo = config.ContentRepository;

            // Assert
            Assert.IsNotNull(repo);
            Assert.IsInstanceOfType(repo, typeof(SqlContentRepository));
        }

        /// <summary>
        /// Tests the content repository return file repository.
        /// </summary>
        [TestMethod]
        public void TestContentRepositoryReturnFileRepository()
        {
            // Arrange
            var config = new MapRouteConfiguration()
            {
                RepositoryType = "file",
                RepositorySource = string.Empty,
                RepositoryTable = string.Empty
            };

            // Act
            var repo = config.ContentRepository;

            // Assert
            Assert.IsNotNull(repo);
            Assert.IsInstanceOfType(repo, typeof(FileContentRepository));
        }
    }
}
