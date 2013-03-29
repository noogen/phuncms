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
            this.ContentRepository = this.ContentConfig.ContentRepository;
            this.PathUtility = new ResourcePathUtility();
        }

        /// <summary>
        /// Gets or sets the path utility.
        /// </summary>
        /// <value>
        /// The path utility.
        /// </value>
        protected ResourcePathUtility PathUtility { get; set; }

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
                                Path = this.ApplyPathConvention(path),
                                Data = System.Text.Encoding.UTF8.GetBytes(data),
                                CreateBy = System.Threading.Thread.CurrentPrincipal.Identity.Name,
                                ModifyBy = System.Threading.Thread.CurrentPrincipal.Identity.Name
                            };

            if (this.ContentRepository.Exists(model))
            {
                throw new HttpException(500, "Cannot create or overwrite an existing content of path: " + path);
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
                Path = this.ApplyPathConvention(path),
                Host = this.GetTenantHost(uri)
            };

            if (content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                var localFilePath = this.ContentRepository.GetFolder(content);
                var fi = new FileInfo(localFilePath);

                content.DataStream = fi.OpenRead();
                content.DataLength = fi.Length;
                content.Path += ".zip";
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
            return this.CreateOrUpdate(path, System.Text.Encoding.UTF8.GetBytes(data), uri);
        }

        /// <summary>
        /// Creates the or update.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        public virtual ContentModel CreateOrUpdate(string path, byte[] data, Uri uri)
        {
            var model = new ContentModel()
            {
                Path = this.ApplyPathConvention(path),
                Data = data,
                Host = this.GetTenantHost(uri),
                CreateBy = System.Threading.Thread.CurrentPrincipal.Identity.Name,
                ModifyBy = System.Threading.Thread.CurrentPrincipal.Identity.Name
            };

            if (string.IsNullOrEmpty(model.Path) || string.Compare(model.Path, "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentException("Cannot update root path.", "path");
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
                                Path = this.ApplyPathConvention(path),
                                Host = this.GetTenantHost(uri)
                            };

            if (model.Path.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(500, "Unable to delete protected path '/'.");
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
                Path = this.ApplyPathConvention(path),
                Host = this.GetTenantHost(uri)
            };

            if (model.Path.EndsWith("/"))
            {
                throw new HttpException(500, "History can only be retrieve for file.");
            }
            else if (!this.ContentRepository.Exists(model))
            {
                throw new HttpException(401, "Content does not exists: " + path);
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
                Path = this.ApplyPathConvention(model.Path),
                Host = this.GetTenantHost(uri),
                DataIdString = model.DataIdString,
                CreateBy = model.CreateBy,
                CreateDate = model.CreateDate
            };

            if (result.Path.EndsWith("/"))
            {
                throw new HttpException(500, "History can only be retrieve for file.");
            }

            if (!this.ContentRepository.Exists(model))
            {
                throw new HttpException(401, "Content does not exists: " + model.Path);
            }

            if (!model.DataId.HasValue || string.IsNullOrEmpty(model.DataIdString))
            {
                throw new HttpException(500, "DataIdString is required for path: " + model.Path);
            }

            this.ContentRepository.PopulateHistoryData(result, model.DataId.Value);

            if (result.DataLength == null)
            {
                throw new HttpException(404, "PhunCms download path not found.");
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
            var model = new ContentModel
            {
                Host = this.GetTenantHost(uri),
                Path =  this.ApplyPathConvention(string.IsNullOrEmpty(path) ? "/" : "/" + path.Trim('/') + "/")
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
                path = (httpContext.Request.Path + string.Empty).Trim().TrimEnd('/');
            }

            // if somehow, CMS resource url get routed to here then we intercept
            if (this.Config.IsResourceRoute(path))
            {
                var vf = new ResourceVirtualFile(path);
                vf.WriteFile(httpContext.Request.RequestContext.HttpContext);
                return;
            }

            path = this.ApplyPathConvention(path);

            // page must start with page path.
            path = string.Concat("/page", path);


            if (!path.EndsWith(".vash"))
            {
                // check for vash
                var newModel = new ContentModel()
                                   {
                                       Host = this.GetTenantHost(httpContext.Request.Url),
                                       Path = path + ".vash"
                                   };

                if (this.ContentRepository.Exists(newModel))
                {
                    path += ".vash";
                }
                else 
                {
                    // file not found, attempt to search for the default file of a folder
                    path += "/_default.vash";
                }
            }

            var model = new ContentModel()
            {
                Host = this.GetTenantHost(httpContext.Request.Url),
                Path = path
            };

            // now that it is a vash file, attempt to render the file
            var engine = new TemplateHandler();
            engine.Render(model, httpContext);
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
        /// <returns>The normalized path.</returns>
        public string ApplyPathConvention(string path)
        {
            var myValue = string.Concat('/', (path + string.Empty).Replace("\\", "/").Replace("//", "/").TrimStart('/'));
            var isFolder = myValue.EndsWith("/", StringComparison.OrdinalIgnoreCase);

            if (!isFolder)
            {
                var parentPath = VirtualPathUtility.GetDirectory(myValue).ToSeoName();
                var fileName = VirtualPathUtility.GetFileName(myValue);
                return string.Concat(parentPath, fileName).TrimStart('~');
            }

            return myValue.ToSeoName();
        }
    }
}
