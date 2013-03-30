namespace Phun.Tests.Data
{
    using System;
    using System.Linq;

    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for File Content Repository.
    /// </summary>
    [TestClass]
    public class FileContentRepositoryTests
    {
        /// <summary>
        /// Tests the exists find file on system.
        /// </summary>
        [TestMethod]
        public void TestExistsFindFileOnSystem()
        {
            // Arrange
            var repo =
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));

            // Act
            var result = repo.Exists(new ContentModel() { Host = "localhost", Path = "/ArticleTitle" });

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests the retrieve get content from file.
        /// </summary>
        [TestMethod]
        public void TestRetrieveGetContentFromFile()
        {
            // Arrange
            var repo =
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));

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
        public void TestSaveNewContentToFile()
        {
            // Arrange
            var repo =
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));

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
            Assert.IsNotNull(result.SetDataFromStream().Data);
            Assert.IsNotNull(result2.SetDataFromStream().Data);
            Assert.AreNotEqual(result.Data, result2.Data);
        }

        /// <summary>
        /// Tests the delete file.
        /// </summary>
        [TestMethod]
        public void TestDeleteFile()
        {
            // Arrange
            var repo =
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));
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
                new FileContentRepository(System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CmsContent"));
            var model = new ContentModel() { Host = "localhost", Path = "/" };

            // Act
            var result = repo.List(model);

            // Assert
            Assert.IsTrue(result.Count() == 2);
        }
    }
}