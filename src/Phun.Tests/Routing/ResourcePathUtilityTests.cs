namespace Phun.Tests.Routing
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Phun.Configuration;
    using Phun.Routing;

    /// <summary>
    /// Container for all resource path utility tests.
    /// </summary>
    [TestClass]
    public class ResourcePathUtilityTests
    {
        /// <summary>
        /// Tests the get current host support multi tenant two levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentTenantSupportMultiTenantForTwoLevelDomain()
        {
            // Arrange
            var cc = new ResourcePathUtility();
            var config = new Mock<ICmsConfiguration>();
            config.Setup(cf => cf.DomainLevel).Returns(2);
            cc.Config = config.Object;

            // Act
            var result = cc.GetTenantHost(new Uri("http://www.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("cha.com", result);
        }

        /// <summary>
        /// Tests the get current host support multi tenant for three levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentTenantSupportMultiTenantForThreeLevelsDomain()
        {
            // Arrange
            var cc = new ResourcePathUtility();
            var config = new Mock<ICmsConfiguration>();
            config.Setup(cf => cf.DomainLevel).Returns(3);
            cc.Config = config.Object;

            // Act
            var result = cc.GetTenantHost(new Uri("http://www.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("youbet.cha.com", result);
        }

        /// <summary>
        /// Tests the get current host support multi tenant for N levels domain.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentTenantSupportMultiTenantForNLevelsDomain()
        {
            // Arrange
            var cc = new ResourcePathUtility();
            var config = new Mock<ICmsConfiguration>();
            config.Setup(cf => cf.DomainLevel).Returns(1);
            cc.Config = config.Object;

            // Act
            var result = cc.GetTenantHost(new Uri("http://www.haha.blahblah.youbet.cha.com/blah"));

            // Assert
            Assert.AreEqual("haha.blahblah.youbet.cha.com", result);
        }
    }
}
