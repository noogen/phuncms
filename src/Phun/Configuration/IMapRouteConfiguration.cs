namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    using Phun.Data;

    /// <summary>
    /// Map route configuration.
    /// </summary>
    public interface IMapRouteConfiguration
    {
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        [ConfigurationProperty("route", IsRequired = true)]
        string Route { get; set; }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        [ConfigurationProperty("controller", IsRequired = false, DefaultValue = "CmsContent")]
        string Controller { get; set; }

        /// <summary>
        /// Gets or sets the type of the repository.
        /// </summary>
        /// <value>
        /// The type of the repository.
        /// </value>
        [ConfigurationProperty("repositoryType", IsRequired = true)]
        string RepositoryType { get; set; }

        /// <summary>
        /// Gets or sets the source or connection string
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        [ConfigurationProperty("repositorySource", IsRequired = false, DefaultValue = "App_Data")]
        string RepositorySource { get; set; }

        /// <summary>
        /// Gets or sets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        [ConfigurationProperty("repositoryTable", IsRequired = false, DefaultValue = "CmsContent")]
        string RepositoryTable { get; set; }

        /// <summary>
        /// Gets or sets the cache location.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        [ConfigurationProperty("repositoryCache", IsRequired = false)]
        string RepositoryCache { get; set; }

        /// <summary>
        /// Gets the content repository.
        /// </summary>
        /// <value>
        /// The content repository.
        /// </value>
        IContentRepository ContentRepository { get; }

        /// <summary>
        /// Gets the route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The route stripped of all invalid characters.
        /// </value>
        string RouteNormalized { get; }
    }
}
