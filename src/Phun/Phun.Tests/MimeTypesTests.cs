namespace Phun.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for Mime Types
    /// </summary>
    [TestClass]
    public class MimeTypesTests
    {
        /// <summary>
        /// Tests the get content type for PNG successful.
        /// </summary>
        [TestMethod]
        public void TestGetContentTypeForPngSuccessful()
        {
            // Arrange
            var expected = "image/png";

            // Act
            var actual = MimeTypes.GetContentType("png");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests the get content type for unknown extension successful.
        /// </summary>
        [TestMethod]
        public void TestGetContentTypeForUnknownExtensionSuccessful()
        {
            // Arrange
            var expected = "application/octet-stream";

            // Act
            var actual = MimeTypes.GetContentType("unknown");

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}