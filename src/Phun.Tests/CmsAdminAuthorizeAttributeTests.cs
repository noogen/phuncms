namespace Phun.Tests
{
    using System;
    using System.Web.Mvc;
    using System.Collections;
    using System.Collections.Generic;

    using Phun.Configuration;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Unit tests for Cms Admin Authorize Attribute
    /// </summary>
    [TestClass]
    public class CmsAdminAuthorizeAttributeTests
    {
        /// <summary>
        /// Tests the populate roles from configuration.
        /// </summary>
        [TestMethod]
        public void TestPopulateDefaultRolesFromPhunCmsConfigurationSection()
        {
            // Arrange
            var attr = new CmsAdminAuthorizeAttribute();
            var expected = "test,test2";
            var authContext = new Mock<AuthorizationContext>();
            authContext.Setup(ac => ac.HttpContext.Request.Url).Returns(new Uri("http://localhost/BogusRoute"));

            // Act
            attr.PopulateRolesFromConfiguration(new PhunCmsConfigurationSection() { AdminRoles = expected }, authContext.Object);

            // Assert
            Assert.AreEqual(expected, attr.Roles);
        }

        /// <summary>
        /// Tests the override default roles from domain authorization config.
        /// </summary>
        [TestMethod]
        public void TestOverrideDefaultRolesFromHostAuthorizationConfig()
        {
            // Arrange
            var attr = new CmsAdminAuthorizeAttribute();
            var expected = "local,local2";
            var authContext = new Mock<AuthorizationContext>();
            authContext.Setup(ac => ac.HttpContext.Request.Url).Returns(new Uri("http://localhost/BogusRoute"));
            var config = new PhunCmsConfigurationSection() { AdminRoles = "test,test2" };
            var expectedElement = new HostAuthorizationConfiguration() { Key = "localhost", Value = expected };
            config.HostAuthorizations = (new HostAuthorizationCollection()) as ICollection<IHostAuthorizationConfiguration>;
            config.HostAuthorizations.Add(expectedElement);
 
            // Act
            attr.PopulateRolesFromConfiguration(config, authContext.Object);

            // Assert
            Assert.AreEqual(expected, attr.Roles);
        }
    }
}