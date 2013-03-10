﻿namespace Phun.Tests.Data
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
        /// Tests the exists find file on system.
        /// </summary>
        [TestMethod]
        public void TestExists()
        {
            // Arrange
            var repo =
                new SqlContentRepository("DefaultDatabase", "CmsContent");

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
                new SqlContentRepository("DefaultDatabase", "CmsContent");

            // Act
            var result = repo.Retrieve(new ContentModel() { Host = "localhost", Path = "/ArticleTitle" });

            // Assert
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.Length > 0);
        }

        /// <summary>
        /// Tests the save new content to file.
        /// </summary>
        [TestMethod]
        public void TestSaveNewContent()
        {
            // Arrange
            var repo =
                new SqlContentRepository("DefaultDatabase", "CmsContent");

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
            Assert.AreNotEqual(result.Data, result2.Data);
        }

        /// <summary>
        /// Tests the delete file.
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
            // Arrange
            var repo =
                new SqlContentRepository("DefaultDatabase", "CmsContent");
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
                new SqlContentRepository("DefaultDatabase", "CmsContent");
            var model = new ContentModel() { Host = "localhost", Path = "/" };

            // Act
            repo.Save(
                    new ContentModel()
                    {
                        Host = "localhost",
                        Path = "/test/foo/article",
                        Data = System.Text.Encoding.UTF8.GetBytes("test")
                    });
            var result = repo.List(model);

            // Assert
            Assert.AreEqual(2, result.Count());
        }
    }
}