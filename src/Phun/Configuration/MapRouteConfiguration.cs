namespace Phun.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Mvc;

    using Phun.Data;

    /// <summary>
    /// Route mapping configuration.
    /// </summary>
    public class MapRouteConfiguration : ConfigurationElement
    {
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
        [ConfigurationProperty("controller", IsRequired = false, DefaultValue = "PhunCmsContent")]
        public virtual string Controller
        {
            get { return (string)this["controller"]; }
            set { this["controller"] = value; }
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
        /// Gets or sets the storage or table name for SQL.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        [ConfigurationProperty("repositoryStorage", IsRequired = false, DefaultValue = "CmsContent")]
        public virtual string RepositoryStorage
        {
            get { return (string)this["repositoryStorage"]; }
            set { this["repositoryStorage"] = value; }
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
                    var repo = new SqlContentRepository(this.RepositorySource, this.RepositoryStorage);
                    result = repo;
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    string baseBath = this.RepositorySource.Replace("~", string.Empty).Trim('/').Replace("/", "\\");

                    // relative path either start with ~ or not contain colon such, i.e. not c:\
                    if (this.RepositorySource.StartsWith("~") || !this.RepositorySource.Contains(":"))
                    {
                        baseBath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseBath);
                    }

                    // append subfolder
                    if (!string.IsNullOrEmpty(this.RepositoryStorage))
                    {
                        baseBath = System.IO.Path.Combine(baseBath, this.RepositoryStorage);
                    }

                    if (!System.IO.Directory.Exists(baseBath))
                    {
                        System.IO.Directory.CreateDirectory(baseBath);
                    }

                    var repo = new FileContentRepository(baseBath);
                    result = repo;
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("ioc", StringComparison.OrdinalIgnoreCase))
                {
                    result = DependencyResolver.Current.GetService<IContentRepository>();
                }
                else
                {
                    var type = Type.GetType(this.RepositoryType);
                    if (type != null)
                    {
                        result = Activator.CreateInstance(type) as IContentRepository;
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
