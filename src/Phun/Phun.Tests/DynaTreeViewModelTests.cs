namespace Phun.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for Dyna Tree View Model
    /// </summary>
    [TestClass]
    public class DynaTreeViewModelTests
    {
        /// <summary>
        /// Tests the dynatree constructor can parse folder.
        /// </summary>
        [TestMethod]
        public void TestDynatreeConstructorCanParseFolder()
        {
            // Arrange
            var expectedTitle = "folder";
            var expectedKey = "/dang/folder/";

            // Act                            
            var tree = new DynaTreeViewModel(expectedKey);

            // Assert
            Assert.AreEqual(expectedTitle, tree.title);
            Assert.AreEqual(expectedKey, tree.key);
            Assert.IsNull(tree.addClass);
            Assert.IsTrue(tree.isFolder);
            Assert.IsTrue(tree.isLazy);
            Assert.IsNotNull(tree.children);
        }

        /// <summary>
        /// Tests the dynatree constructor can parse file with extension.
        /// </summary>
        [TestMethod]
        public void TestDynatreeConstructorCanParseFileWithExtension()
        {
            // Arrange
            var expectedTitle = "myfile.txt";
            var expectedKey = "/dang/folder/myfile.txt";

            // Act                            
            var tree = new DynaTreeViewModel(expectedKey);

            // Assert
            Assert.AreEqual(expectedTitle, tree.title);
            Assert.AreEqual(expectedKey, tree.key);
            Assert.AreEqual("ext_txt", tree.addClass);
            Assert.IsFalse(tree.isFolder);
            Assert.IsFalse(tree.isLazy);
            Assert.IsNotNull(tree.children);
        }
    }
}