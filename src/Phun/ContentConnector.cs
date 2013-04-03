namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Extensions;
    using Phun.Routing;
    using Phun.Templating;

    /// <summary>
    /// This is the default content connector.  It allow for PhunCMS
    /// to control content convention.  Of course, you can always implement your
    /// own convention which is not recommended.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class ContentConnector : IContentConnector
    {
        /// <summary>
        /// The hidden folder chars.
        /// </summary>
        private static readonly Regex HiddenFolderChars = new Regex(@"[_\.]", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentConnector" /> class.
        /// </summary>
        public ContentConnector()
        {
            this.Config = Bootstrapper.Config;
            this.ContentConfig = Bootstrapper.ContentConfig;
            this.ContentRepository = (this.ContentConfig != null) ? this.ContentConfig.ContentRepository : null;
            this.PathUtility = new ResourcePathUtility();
            this.TemplateHandler = new TemplateHandler();
        }

        /// <summary>
        /// Gets or sets the content config.
        /// </summary>
        /// <value>
        /// The content config.
        /// </value>
        public IMapRouteConfiguration ContentConfig { get; set; }

        /// <summary>
        /// Gets or sets the content repository.
        /// </summary>
        /// <value>
        /// The content repository.
        /// </value>
        public IContentRepository ContentRepository { get; set; }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public ICmsConfiguration Config { get; set; }

        /// <summary>
        /// Gets or sets the path utility.
        /// </summary>
        /// <value>
        /// The path utility.
        /// </value>
        protected internal ResourcePathUtility PathUtility { get; set; }

        /// <summary>
        /// Gets or sets the template handler.
        /// </summary>
        /// <value>
        /// The template handler.
        /// </value>
        protected internal ITemplateHandler TemplateHandler { get; set; }

        /// <summary>
        /// Create the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        /// <exception cref="System.Web.HttpException">500;Cannot create or overwrite an existing content of path:  + path</exception>
        public virtual ContentModel Create(string path, string data, Uri uri)
        {
            var model = new ContentModel()
                            {
                                Path = this.Normalize(path),
                                Host = this.GetTenantHost(uri)
                            };

            if (this.ContentRepository.Exists(model))
            {
                throw new HttpException(500, "Cannot create or overwrite an existing content: " + path);
            }

            return this.CreateOrUpdate(path, data, uri);
        }

        /// <summary>
        /// Retrieves the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content result.</returns>
        /// <exception cref="System.Web.HttpException">404;PhunCms path not found.</exception>
        public virtual ContentModel Retrieve(string path, Uri uri)
        {
            var content = new ContentModel()
            {
                Path = this.Normalize(path),
                Host = this.GetTenantHost(uri)
            };

            if (content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                var localFilePath = this.ContentRepository.GetFolder(content);
                
                content.DataStream = new System.IO.FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                content.Path = content.Path.Replace("/", "_") + ".zip";
                return content;
            }

            var result = this.ContentRepository.Retrieve(content, true);

            if (result.DataLength == null)
            {
                throw new HttpException(404, "PhunCms path not found: " + path);
            }

            return result;
        }

        /// <summary>
        /// Update the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        public virtual ContentModel CreateOrUpdate(string path, string data, Uri uri)
        {
            return this.CreateOrUpdate(path, uri, data == null ? null : System.Text.Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Creates the or update.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns>
        /// Content model result.
        /// </returns>
        /// <exception cref="System.ArgumentException">Cannot update root path.;path</exception>
        public virtual ContentModel CreateOrUpdate(string path, Uri uri, byte[] data)
        {
            var model = new ContentModel()
            {
                Path = this.Normalize(path),
                Data = data,
                Host = this.GetTenantHost(uri),
                CreateBy = System.Threading.Thread.CurrentPrincipal.Identity.Name,
                ModifyBy = System.Threading.Thread.CurrentPrincipal.Identity.Name
            };

            // empty files are allowed, otherwise throw exception somewhere here
            // root folder creation is not allow
            if (string.Compare(model.Path, "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new HttpException(500, "Cannot update root path.");
            }

            this.ContentRepository.Save(model);
            return model;
        }

        /// <summary>
        /// Deletes the specified model.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        /// <exception cref="System.Web.HttpException">500;Unable to delete protected path '/'.</exception>
        public virtual ContentModel Delete(string path, Uri uri)
        {
            var model = new ContentModel()
                            {
                                Path = this.Normalize(path),
                                Host = this.GetTenantHost(uri)
                            };

            if (model.Path.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(500, "Unable to delete root path '/'.");
            }

            this.ContentRepository.Remove(model);
            return model;
        }

        /// <summary>
        /// Histories this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// Path content history.
        /// </returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.</exception>
        public virtual IQueryable<ContentModel> History(string path, Uri uri)
        {
            var model = new ContentModel()
            {
                Path = this.Normalize(path),
                Host = this.GetTenantHost(uri)
            };

            if (model.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(500, "History can only be retrieve for file.");
            }
            else if (!this.ContentRepository.Exists(model))
            {
                throw new HttpException(401, "Content does not exists for path: " + model.Path);
            }

            return this.ContentRepository.RetrieveHistory(model).Where(h => !HiddenFolderChars.IsMatch(h.ParentPath)).OrderByDescending(o => o.CreateDate);
        }

        /// <summary>
        /// Get the history the data.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// The history data content.
        /// </returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.
        /// or
        /// 401;Content does not exists:  + path</exception>
        public virtual ContentModel HistoryData(ContentModel model, Uri uri)
        {
            var result = new ContentModel()
            {
                Path = this.Normalize(model.Path),
                Host = this.GetTenantHost(uri),
                DataIdString = model.DataIdString,
                CreateBy = model.CreateBy,
                CreateDate = model.CreateDate
            };

            if (model.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(500, "History can only be retrieve for file.");
            }
            else if (!this.ContentRepository.Exists(model))
            {
                throw new HttpException(401, "Content does not exists for path: " + model.Path);
            }
            else if (!model.DataId.HasValue || string.IsNullOrEmpty(model.DataIdString))
            {
                throw new HttpException(500, "DataIdString is required for path: " + model.Path);
            }

            result = this.ContentRepository.PopulateHistoryData(result, model.DataId.Value);

            if (result.DataLength == null)
            {
                throw new HttpException(404, string.Format("History data Id '{0}' not found for path: {1}", result.DataIdString, result.Path));
            }

            return result;
        }

        /// <summary>
        /// Lists the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>List of content models.</returns>
        public IQueryable<ContentModel> List(string path, Uri uri)
        {
            // make sure it's a folder listing
            var model = new ContentModel
            {
                Host = this.GetTenantHost(uri),
                Path = this.Normalize(path).Trim().TrimEnd('/') + "/"
            };

            return this.ContentRepository.List(model).Where(h => !HiddenFolderChars.IsMatch(h.ParentPath));            
        }

        /// <summary>
        /// View content.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public virtual void RenderPage(HttpContextBase httpContext)
        {
            var path = httpContext.Request.QueryString["path"];
            if (string.IsNullOrEmpty(path))
            {
                path = (httpContext.Request.Path + string.Empty).Trim();
            }

            // if somehow, CMS Resource URL get routed here then intercept
            if (this.Config.IsResourceRoute(path))
            {
                this.Config.GetResourceFile(path).WriteFile(httpContext);
                return;
            }

            if (!path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.RedirectPermanent(path + "/");
                return;
            }

            path = path.TrimEnd('/');
            var tenantHost = this.GetTenantHost(httpContext.Request.Url);
            var model = new ContentModel()
            {
                Host = tenantHost,
                Path = this.ResolvePath(path, tenantHost, httpContext)
            };

            // now that it is a vash file, attempt to render the file
            this.TemplateHandler.Render(model, this, httpContext);
        }


        /// <summary>
        /// Resolves the path.  Also allow for caching path resolution.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="tenantHost">The tenant host.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The path last resolved to.
        /// </returns>
        public virtual string ResolvePath(string path, string tenantHost, HttpContextBase context)
        {
            var resultKey = string.Format("__PhunStaticRoutingCache__{0}__{1}", tenantHost, path);
            var result = context.Cache[resultKey] as string;

            if (string.IsNullOrEmpty(result))
            {
                // only normalize after determine that it is not a CMS Resource URL
                // it must also start with page path
                result = string.Concat("/page", this.Normalize(path));
                var newModel = new ContentModel()
                {
                    Host = tenantHost,
                    Path = result
                };

                if (!result.EndsWith(".vash", StringComparison.OrdinalIgnoreCase))
                {
                    newModel.Path += ".vash";
                    if (this.ContentRepository.Exists(newModel))
                    {
                        result = newModel.Path;
                    }
                    else
                    {
                        newModel.Path = result + "/_default.vash";
                        result = this.ContentRepository.Exists(newModel) ? newModel.Path : "==NOT FOUND==";
                    }
                }

                context.Cache.Insert(
                        resultKey, result, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(this.Config.CacheDuration));
            }

            if (string.Compare("==NOT FOUND==", result, StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new HttpException(404, "Failed on all attempt to search vash file for: " + path);
            }

            return result;
        }

        /// <summary>
        /// Gets the current host.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        /// The current host.
        /// </returns>
        public virtual string GetTenantHost(Uri url)
        {
            return this.PathUtility.GetTenantHost(url);
        }

        /// <summary>
        /// Applies the path convention.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The normalized path.
        /// </returns>
        public virtual string Normalize(string path)
        {
            return this.PathUtility.Normalize(path);
        }
    }
}
