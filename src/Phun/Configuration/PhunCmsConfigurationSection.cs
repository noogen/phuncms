namespace Phun.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// For reading configuration from web.config.
    /// </summary>
    public class PhunCmsConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the admin roles.
        /// </summary>
        /// <value>
        /// The admin roles.
        /// </value>
        [ConfigurationProperty("adminRoles", IsRequired = false)]
        public string AdminRoles
        {
            get { return (string)this["adminRoles"]; }
            set { this["adminRoles"] = value; }
        }

        /// <summary>
        /// Gets or sets the resource route.
        /// </summary>
        /// <value>
        /// The resource route.
        /// </value>
        [ConfigurationProperty("resourceRoute", IsRequired = true, DefaultValue = "PhunCms")]
        public string ResourceRoute
        {
            get { return (string)this["resourceRoute"]; }
            set { this["resourceRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the content route.
        /// </summary>
        /// <value>
        /// The content route.
        /// </value>
        [ConfigurationProperty("contentRoute", IsRequired = true, DefaultValue = "CmsContent")]
        public string ContentRoute
        {
            get { return (string)this["contentRoute"]; }
            set { this["contentRoute"] = value; }
        }

        /// <summary>
        /// Gets or sets the content map.
        /// </summary>
        /// <value>
        /// The content map.
        /// </value>
        [ConfigurationProperty("hostAuthorization")]
        [ConfigurationCollection(typeof(HostAuthorizationCollection),
              AddItemName = "add",
              ClearItemsName = "clear",
              RemoveItemName = "remove")]
        public HostAuthorizationCollection HostAuthorizations
        {
            get { return (HostAuthorizationCollection)this["hostAuthorization"]; }
            set { this["hostAuthorization"] = value; }
        }

        /// <summary>
        /// Gets or sets the other contents.
        /// </summary>
        /// <value>
        /// The other contents.
        /// </value>
        [ConfigurationProperty("contentMap")]
        [ConfigurationCollection(typeof(HostAuthorizationCollection),
              AddItemName = "add",
              ClearItemsName = "clear",
              RemoveItemName = "remove")]
        public MapRouteCollection ContentMaps
        {
            get { return (MapRouteCollection)this["contentMap"]; }
            set { this["contentMap"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }

        /// <summary>
        /// Gets the resource route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The resource route stripped of all invalid characters.
        /// </value>
        public string ResourceRouteNormalized
        {
            get
            {
                var result = (this.ResourceRoute + string.Empty).Replace("~", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty);
                return result;
            }
        }

        /// <summary>
        /// Gets the route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The route stripped of all invalid characters.
        /// </value>
        public string ContentRouteNormalized
        {
            get
            {
                var result = (this.ContentRoute + string.Empty).Replace("~", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty);
                return result;
            }
        }

        /// <summary>
        /// Determines whether [is resource route] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is resource route] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsResourceRoute(string path)
        {
            return
                (path + string.Empty).Trim()
                           .Replace("~", string.Empty)
                           .StartsWith(
                               string.Concat("/", this.ResourceRouteNormalized, "/"), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether [is content route] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is content route] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsContentRoute(string path)
        {
            return
                (path + string.Empty).Trim()
                           .Replace("~", string.Empty)
                           .StartsWith(
                               string.Concat("/", this.ContentRouteNormalized, "/"), StringComparison.OrdinalIgnoreCase);
        }
    }
}
