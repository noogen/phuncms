namespace Phun.Tests.Data
{
    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for content model.
    /// </summary>
    [TestClass]
    public class ContentModelTests
    {
        /// <summary>
        /// Tests the path set method correctly normalize path.
        /// </summary>
        [TestMethod]
        public void TestPathSetMethodCorrectlyNormalizePath()
        {
            // Arrange
            var model = new ContentModel();
            var expected = "/test/one/two/three";

            // Act
            model.Path = "test\\one//two/three";

            // Assert
            Assert.AreEqual(expected, model.Path);
        }

        /// <summary>
        /// Tests the content model correctly parse parent directory.
        /// </summary>
        [TestMethod]
        public void TestContentModelCorrectlyParseParentDirectory()
        {
            // Arrange
            var model = new ContentModel();
            var expected = "/test/one/two/";

            // Act
            model.Path = "/test/one/two/three";

            // Assert
            Assert.AreEqual(expected, model.ParentDirectory);

            // Arrange
            expected = "/";

            // Act
            model.Path = string.Empty;

            // Assert
            Assert.AreEqual("/", model.ParentDirectory);
        }

        /// <summary>
        /// Tests the name of the content model correctly parse file.
        /// </summary>
        [TestMethod]
        public void TestContentModelCorrectlyParseFileName()
        {
            // Arrange
            var model = new ContentModel();
            var expected = "three";

            // Act
            model.Path = "/test/one/two/three";

            // Assert
            Assert.AreEqual(expected, model.FileName);
        }

        /// <summary>
        /// Tests the content model correctly parse file extension.
        /// </summary>
        [TestMethod]
        public void TestContentModelCorrectlyParseFileExtension()
        {
            // Arrange
            var model = new ContentModel();
            var expected = "txt";

            // Act
            model.Path = "/test/one/two/three.txt";

            // Assert
            Assert.AreEqual(expected, model.FileExtension);
        }
    }
}
