namespace Phun.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    using Phun.Data;

    /// <summary>
    /// Route mapping configuration.
    /// </summary>
    public class MapRouteConfiguration : ConfigurationElement, IMapRouteConfiguration
    {
        /// <summary>
        /// The static reflection
        /// </summary>
        private static readonly Dictionary<Type, ConstructorInfo> staticReflection = new Dictionary<Type, ConstructorInfo>();

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        [ConfigurationProperty("route", IsRequired = true)]
        public virtual string Route
        {
            get { return (string)this["route"]; }
            set { this["route"] = value; }
        }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        [ConfigurationProperty("controller", IsRequired = false, DefaultValue = "CmsContent")]
        public virtual string Controller
        {
            get { return (string)this["controller"]; }
            set { this["controller"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the repository.
        /// </summary>
        /// <value>
        /// The type of the repository.
        /// </value>
        [ConfigurationProperty("repositoryType", IsRequired = true)]
        public virtual string RepositoryType
        {
            get { return (string)this["repositoryType"]; }
            set { this["repositoryType"] = value; }
        }

        /// <summary>
        /// Gets or sets the source or connection string
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        [ConfigurationProperty("repositorySource", IsRequired = false, DefaultValue = "App_Data")]
        public virtual string RepositorySource
        {
            get { return (string)this["repositorySource"]; }
            set { this["repositorySource"] = value; }
        }

        /// <summary>
        /// Gets or sets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        [ConfigurationProperty("repositoryTable", IsRequired = false, DefaultValue = "CmsContent")]
        public virtual string RepositoryTable
        {
            get { return (string)this["repositoryTable"]; }
            set { this["repositoryTable"] = value; }
        }

        /// <summary>
        /// Gets or sets the cache location.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        [ConfigurationProperty("repositoryCache", IsRequired = false)]
        public virtual string RepositoryCache
        {
            get { return (string)this["repositoryCache"]; }
            set { this["repositoryCache"] = value; }
        }

        /// <summary>
        /// Gets or sets the history file extension.
        /// </summary>
        /// <value>
        /// The history file extension.
        /// </value>
        [ConfigurationProperty("historyFileExtension", IsRequired = false, DefaultValue = "")]
        public virtual string HistoryFileExtension
        {
            get { return (string)this["historyFileExtension"]; }
            set { this["historyFileExtension"] = value; }
        }

        /// <summary>
        /// Gets the content repository.
        /// </summary>
        /// <value>
        /// The content repository.
        /// </value>
        public virtual IContentRepository ContentRepository
        {
            get
            {
                IContentRepository result = null;

                if ((this.RepositoryType + string.Empty).Trim().Equals("sql", StringComparison.OrdinalIgnoreCase))
                {
                    result = new SqlContentRepository(this);
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    result = new FileContentRepository(this);
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("ioc", StringComparison.OrdinalIgnoreCase))
                {
                    result = DependencyResolver.Current != null ? DependencyResolver.Current.GetService<IContentRepository>() : null;
                }
                else if (!string.IsNullOrEmpty(this.RepositoryType))
                {
                    var type = Type.GetType(this.RepositoryType);
                    if (type != null)
                    {
                        // attempt to locate default constructor
                        ConstructorInfo defaultConstructor = null;

                        if (!staticReflection.TryGetValue(type, out defaultConstructor))
                        {
                            foreach (var ctor in type.GetConstructors().Where(c => c.IsPublic))
                            {
                                var parameterTypes = ctor.GetParameters();
                                if (parameterTypes.Length > 1)
                                {
                                    continue;
                                }
                                else if (parameterTypes.Length == 0)
                                {
                                    defaultConstructor = ctor;
                                }

                                if (parameterTypes[0].ParameterType == typeof(IMapRouteConfiguration))
                                {
                                    defaultConstructor = ctor;
                                    break;
                                }
                            }

                            if (defaultConstructor == null)
                            {
                                throw new ApplicationException(
                                    this.RepositoryType + " is invalid PhunCms repository configuration.");
                            }

                            staticReflection.Add(type, defaultConstructor);
                        }

                        result = (defaultConstructor.GetParameters().Length > 0 ? Activator.CreateInstance(type, this) : Activator.CreateInstance(type)) as IContentRepository;
                    }
                }

                if (result == null)
                {
                    throw new ApplicationException("Cms content repository configuration is invalid.");
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the route stripped of all invalid characters.
        /// </summary>
        /// <value>
        /// The route stripped of all invalid characters.
        /// </value>
        public virtual string RouteNormalized
        {
            get
            {
                var result = (this.Route + string.Empty).Replace("~", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty);
                return result;
            }
        }
    }  
}
