namespace Phun.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Phun.Routing;

    /// <summary>
    /// For reading configuration from web.config.
    /// </summary>
    public class PhunCmsConfigurationSection : ConfigurationSection, ICmsConfiguration
    {
        /// <summary>
        /// Gets or sets the static content extension.
        /// </summary>
        /// <value>
        /// The static content extension.
        /// </value>
        [ConfigurationProperty("staticContentExtension", IsRequired = false, DefaultValue = "ico|pdf|flv|jpg|jpeg|png|gif|js|css|htm|html|mp4|avi")]
        public string StaticContentExtension
        {
            get { return (string)this["staticContentExtension"]; }
            set { this["staticContentExtension"] = value; }
        }

        /// <summary>
        /// Gets or sets the duration of the cache.
        /// </summary>
        /// <value>
        /// The duration of the cache.
        /// </value>
        [ConfigurationProperty("cacheDuration", IsRequired = false, DefaultValue = 2)]
        public int CacheDuration
        {
            get { return (int)this["cacheDuration"]; }
            set { this["cacheDuration"] = value; }
        }


          /// <summary>
        /// Gets or sets the file manager.
        /// </summary>
        /// <value>
        /// The file manager.
        /// </value>
        [ConfigurationProperty("fileManager", IsRequired = false, DefaultValue = "/[resourceroute]/filemanager.htm")]
        public string FileManager
        {
            get { return (string)this["fileManager"]; }
            set { this["fileManager"] = value; }
        }

        /// <summary>
        /// Gets or sets the file editor.
        /// </summary>
        /// <value>
        /// The file editor.
        /// </value>
        [ConfigurationProperty("fileEditor", IsRequired = false, DefaultValue = "/[resourceroute]/edit.htm")]
        public string FileEditor
        {
            get { return (string)this["fileEditor"]; }
            set { this["fileEditor"] = value; }
        }

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
        /// Gets or sets a value indicating whether [disable resource cache].
        /// </summary>
        /// <value>
        /// <c>true</c> if [disable resource cache]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("disableResourceCache", IsRequired = false, DefaultValue = false)]
        public bool DisableResourceCache
        {
            get { return (bool)this["disableResourceCache"]; }
            set { this["disableResourceCache"] = value; }
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
        public HostAuthorizationCollection HostAuthorizationCollection
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
        public MapRouteCollection ContentMapCollection
        {
            get { return (MapRouteCollection)this["contentMap"]; }
            set { this["contentMap"] = value; }
        }

        /// <summary>
        /// Gets or sets the content map.
        /// </summary>
        /// <value>
        /// The content map.
        /// </value>
        public ICollection<IHostAuthorizationConfiguration> HostAuthorizations
        {
            get
            {
                return this.HostAuthorizationCollection as ICollection<IHostAuthorizationConfiguration>;
            }

            set
            {
                this.HostAuthorizationCollection = value as HostAuthorizationCollection;
            }
        }

        /// <summary>
        /// Gets or sets the other contents.
        /// </summary>
        /// <value>
        /// The other contents.
        /// </value>
        public ICollection<IMapRouteConfiguration> ContentMaps
        {
            get
            {
                return this.ContentMapCollection as ICollection<IMapRouteConfiguration>;
            }

            set
            {
                this.ContentMapCollection = value as MapRouteCollection;
            }
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
        /// Gets or sets the domain level.
        /// </summary>
        /// <value>
        /// The domain level.
        /// </value>
        [ConfigurationProperty("domainLevel", IsRequired = false, DefaultValue = 1)]
        public virtual int DomainLevel
        {
            get { return (int)this["domainLevel"]; }
            set { this["domainLevel"] = value; }
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


        /// <summary>
        /// Gets the resource file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The resource virtual file.
        /// </returns>
        public Routing.ResourceVirtualFile GetResourceFile(string path)
        {
            return new ResourceVirtualFile(path);
        }
    }
}
