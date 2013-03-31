namespace Phun.Tests.StorageStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Phun.Routing;

    /// <summary>
    /// Unit tests for Simple Cms Controller / Content Controller
    /// </summary>
    [TestClass]
    public class CmsContentControllerTests
    {
        /// <summary>
        /// Tests the edit return resource edit view result.
        /// </summary>
        [TestMethod]
        public void TestEditReturnResourceEditViewResult()
        {
            // Arrange
            var controller = new CmsContentController();

            // Act
            var result = controller.Edit("~/blah") as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests the file browser return resource view result.
        /// </summary>
        [TestMethod]
        public void TestFileManagerReturnResourceViewResult()
        {
            // Arrange
            var controller = new CmsContentController();

            // Act
            var result = controller.FileManager(string.Empty) as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}