namespace Phun.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    using Phun.Routing;

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
        /// Gets or sets the host authorizations.
        /// </summary>
        /// <value>
        /// The host authorizations.
        /// </value>
        ICollection<IKeyValueConfiguration> HostAuthorizations { get; set; }

        /// <summary>
        /// Gets or sets the other contents.
        /// </summary>
        /// <value>
        /// The other contents.
        /// </value>
        ICollection<IMapRouteConfiguration> ContentMaps { get; set; }

        /// <summary>
        /// Gets or sets the host aliases.
        /// </summary>
        /// <value>
        /// The host aliases.
        /// </value>
        ICollection<IKeyValueConfiguration> HostAliases { get; set; }

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
        /// Gets or sets the cache in seconds.
        /// </summary>
        /// <value>
        /// The cache in seconds.
        /// </value>                
        [ConfigurationProperty("cacheDuration", IsRequired = false, DefaultValue = 60)]
        int CacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the file manager.
        /// </summary>
        /// <value>
        /// The file manager.
        /// </value>
        [ConfigurationProperty("fileManager", IsRequired = false, DefaultValue = "/[resourceroute]/filemanager.htm")]
        string FileManager { get; set; }

        /// <summary>
        /// Gets or sets the file editor.
        /// </summary>
        /// <value>
        /// The file editor.
        /// </value>
        [ConfigurationProperty("fileEditor", IsRequired = false, DefaultValue = "/[resourceroute]/editor.htm")]
        string FileEditor { get; set; }

        /// <summary>
        /// Gets or sets the static content extension.
        /// </summary>
        /// <value>
        /// The static content extension.
        /// </value>
        [ConfigurationProperty("staticContentExtension", IsRequired = false, DefaultValue = "ico|pdf|flv|jpg|jpeg|png|gif|js|css|htm|html|mp4|avi|txt")]
        string StaticContentExtension { get; set; }

        /// <summary>
        /// Gets the resource file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The resource virtual file.</returns>
        ResourceVirtualFile GetResourceFile(string path);

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
