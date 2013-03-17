namespace Phun.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Mvc;

    using Microsoft.WindowsAzure.ServiceRuntime;

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
                    string basePath = (this.RepositoryCache + string.Empty).Replace("~", string.Empty).Trim('/').Replace("/", "\\");
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        basePath = this.ResolveLocalPath(basePath);
                    }

                    var dataRepo = DependencyResolver.Current != null ? DependencyResolver.Current.GetService<ISqlDataRepository>() : null;
                    if (dataRepo == null)
                    {
                        dataRepo = new SqlDataRepository();
                    }

                    var repo = new SqlContentRepository(dataRepo, this.RepositorySource, this.RepositoryTable, basePath);
                    result = repo;
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    string basePath = this.RepositorySource.Replace("~", string.Empty).Trim('/').Replace("/", "\\");
                    basePath = this.ResolveLocalPath(basePath);
                    var repo = new FileContentRepository(basePath);
                    result = repo;
                }
                else if ((this.RepositoryType + string.Empty).Trim().Equals("ioc", StringComparison.OrdinalIgnoreCase))
                {
                    result = DependencyResolver.Current != null ? DependencyResolver.Current.GetService<IContentRepository>() : null;
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
        /// Resolves the local path.
        /// </summary>
        /// <param name="basePath">The path.</param>
        /// <returns>A valid base path.</returns>
        private string ResolveLocalPath(string basePath)
        {
            // relative path either start with ~ or not contain colon such, i.e. not c:\
            if (basePath.Contains(":"))
            {
                if (basePath.StartsWith("localstorage:", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = basePath.Replace("localstorage:", string.Empty).TrimStart('/', '\\');
                    basePath = RoleEnvironment.GetLocalResource(basePath).RootPath;
                }
            }
            else
            {
                basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
            }

            // create directory if not exists
            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }

            return basePath;
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
