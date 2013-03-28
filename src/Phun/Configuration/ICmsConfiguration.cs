namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// CMS Configuration
    /// </summary>
    public interface ICmsConfiguration
    {
        /// <summary>
        /// Gets or sets the admin roles.
        /// </summary>
        /// <value>
        /// The admin roles.
        /// </value>
        string AdminRoles { get; set; }

        /// <summary>
        /// Gets or sets the resource route.
        /// </summary>
        /// <value>
        /// The resource route.
        /// </value>
        string ResourceRoute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable resource cache].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable resource cache]; otherwise, <c>false</c>.
        /// </value>
        bool DisableResourceCache { get; set; }

        /// <summary>
        /// Gets or sets the content route.
        /// </summary>
        /// <value>
        /// The content route.
        /// </value>
        string ContentRoute { get; set; }

        /// <summary>
        /// Gets or sets the content map.
        /// </summary>
        /// <value>
        /// The content map.
        /// </value>
        ICollection<IHostAuthorizationConfiguration> HostAuthorizations { get; set; }

        /// <summary>
        /// Gets or sets the other contents.
        /// </summary>
        /// <value>
        /// The other contents.
        /// </value>
        ICollection<IMapRouteConfiguration> ContentMaps { get; set; }

        /// <summary>
        /// Gets the resource route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The resource route stripped of all invalid characters.
        /// </value>
        string ResourceRouteNormalized { get; }

        /// <summary>
        /// Gets the route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The route stripped of all invalid characters.
        /// </value>
        string ContentRouteNormalized { get; }

        /// <summary>
        /// Gets or sets the domain level.
        /// </summary>
        /// <value>
        /// The domain level.
        /// </value>
        [ConfigurationProperty("domainLevel", IsRequired = false, DefaultValue = 1)]
        int DomainLevel { get; set; }

        /// <summary>
        /// Determines whether [is resource route] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is resource route] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        bool IsResourceRoute(string path);

        /// <summary>
        /// Determines whether [is content route] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is content route] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        bool IsContentRoute(string path);
    }
}
