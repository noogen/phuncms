namespace Phun.Tests.Data
{
    using System;
    using System.Linq;

    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for Sql Content Repository.
    /// </summary>
    [TestClass]
    public class SqlContentRepositoryTests
    {
        /// <summary>
        /// Mies the test initialize.
        /// </summary>
        /// <param name="testContext">The test context.</param>
        [AssemblyInitialize()]
        public static void MyTestInitialize(TestContext testContext)
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);

            var fileRepo =
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));

            // Act
            var result = fileRepo.Retrieve(new ContentModel() { Host = "localhost", Path = "/ArticleTitle" }, true);
            repo.Save(result);

            result = fileRepo.Retrieve(new ContentModel() { Host = "localhost", Path = "/test/test/article" }, true);
            repo.Save(result);

            result = fileRepo.Retrieve(new ContentModel() { Host = "localhost", Path = "/test/test/article-title" }, true);
            repo.Save(result);
        }

        /// <summary>
        /// Tests the exists find file on system.
        /// </summary>
        [TestMethod]
        public void TestExists()
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);

            // Act
            var result = repo.Exists(new ContentModel() { Host = "localhost", Path = "/ArticleTitle" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests the retrieve get content from file.
        /// </summary>
        [TestMethod]
        public void TestRetrieveGetContent()
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);

            // Act
            var result = repo.Retrieve(new ContentModel() { Host = "localhost", Path = "/ArticleTitle" }, true);

            // Assert
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.DataLength > 0);
        }

        /// <summary>
        /// Tests the save new content to file.
        /// </summary>
        [TestMethod]
        public void TestSaveNewContent()
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);

            // Act
            var result = repo.Retrieve(new ContentModel() { Host = "localhost", Path = "/test/test/article-title" });
            repo.Save(
                    new ContentModel()
                    {
                        Host = "localhost",
                        Path = "/test/test/article-title",
                        Data = System.Text.Encoding.UTF8.GetBytes("test")
                    });

            var result2 = repo.Retrieve(new ContentModel() { Host = "localhost", Path = "/test/test/article-title" });

            // Assert
            Assert.IsNotNull(result.Data);
            Assert.IsNotNull(result2.Data);

            Assert.AreNotEqual(System.Text.Encoding.UTF8.GetString(result.Data), System.Text.Encoding.UTF8.GetString(result2.Data));
        }

        /// <summary>
        /// Tests the delete file.
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);
            var model = new ContentModel() { Host = "localhost", Path = "/test/test/article" };

            // Act
            repo.Remove(model);
            var result = repo.Exists(model);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests the list root path returns two item.
        /// </summary>
        [TestMethod]
        public void TestListRootPathReturnsTwoItem()
        {
            // Arrange
            var repo =
                new SqlContentRepository(new SqlDataRepository(), "DefaultDatabase", "CmsContent", string.Empty);
            var model = new ContentModel() { Host = "localhost", Path = "/" };

            // Act
            repo.Save(
                    new ContentModel()
                    {
                        Host = "localhost",
                        Path = "/test/foo/article",
                        Data = System.Text.Encoding.UTF8.GetBytes("test"),
                        CreateBy = string.Empty
                    });
            var result = repo.List(model);

            // Assert
            Assert.AreEqual(2, result.Count());
        }
    }
}