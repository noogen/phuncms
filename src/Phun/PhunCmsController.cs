namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Extensions;
    using Phun.Routing;

    /// <summary>
    /// Default controller.
    /// </summary>
    public abstract class PhunCmsController : Controller
    {
        /// <summary>
        /// The template regular expression.
        /// </summary>
        protected static readonly Regex TemplateRegex = new Regex(@"%[^%]+%", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCmsController"/> class.
        /// </summary>
        protected PhunCmsController()
        {
            this.Config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public abstract MapRouteConfiguration ContentConfig { get; }

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
                return this.ContentConfig.ContentRepository;
            }
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        internal PhunCmsConfigurationSection Config { get; set; }

        #region "Backbone sync methods for content edit"
        /// <summary>
        /// Create the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>
        /// Action result.
        /// </returns>
        [HttpPost, ValidateInput(false)]
        public virtual ActionResult Create(string path, string data, string returnUrl)
        {
            var model = new ContentModel()
                            {
                                Path = path,
                                Data = System.Text.Encoding.UTF8.GetBytes(data),
                                CreateBy = this.User.Identity.Name,
                                ModifyBy = this.User.Identity.Name
                            };

            if (this.ContentRepository.Exists(model))
            {
                throw new ArgumentException("Cannot overwrite an existing content of path: " + path, path);
            }

            return this.Update(path, data, returnUrl);
        }

        /// <summary>
        /// Retrieves this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.HttpException">404;PhunCMS view content path not found.</exception>
        [HttpGet, AllowAnonymous]
        public virtual ActionResult Retrieve(string path)
        {
            var model = new ContentModel()
            {
                Path = path,
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url)
            };

            if (model.Path.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                return this.Redirect("/");
            }

            var content = this.ContentRepository.Retrieve(model, true);

            if (content.DataLength == null)
            {
                throw new HttpException(404, "PhunCms path not found for retrieve.");
            }

            content.SetDataFromStream();

            var resultString = System.Text.Encoding.UTF8.GetString(content.Data);
            var toLookup = new Dictionary<string, ContentModel>();

            // assuming all content are templates, attempt to identity replace patterns #partialcontentpath#
            foreach (Match match in TemplateRegex.Matches(resultString).Cast<Match>().Where(match => !toLookup.ContainsKey(match.Value.Replace("%", string.Empty))))
            {
                var matchValue = match.Value.Replace("%", string.Empty);
                if (!toLookup.ContainsKey(matchValue))
                {
                    toLookup.Add(
                        matchValue,
                        new ContentModel() { Host = content.Host, Path = matchValue });
                }
            }

            // now populate the content
            foreach (var key in toLookup.Keys)
            {
                var c = toLookup[key];
                var result = this.ContentRepository.Retrieve(c, true);
                if (result.DataLength > 0)
                {
                    result.SetDataFromStream();

                    resultString = resultString.Replace(
                        "%" + key + "%", System.Text.Encoding.UTF8.GetString(result.Data));
                }
                else
                {
                    resultString = resultString.Replace(
                        "%" + key + "%", string.Empty);
                }
            }

            // return the content
            return this.Content(resultString + string.Empty);
        }

        /// <summary>
        /// Update the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>
        /// Action result.
        /// </returns>
        [HttpPost, ValidateInput(false)]
        public virtual ActionResult Update(string path, string data, string returnUrl)
        {
            var model = new ContentModel()
            {
                Path = path,
                Data = System.Text.Encoding.UTF8.GetBytes(data),
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url),
                CreateBy = this.User.Identity.Name,
                ModifyBy = this.User.Identity.Name
            };

            this.ContentRepository.Save(model);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.Json(new { createdate = model.CreateDate, success = true });
        }

        /// <summary>
        /// Deletes the specified model.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Action result.
        /// </returns>
        [HttpDelete]
        public virtual ActionResult Delete(string path)
        {
            var model = new ContentModel()
                            {
                                Path = path,
                                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url)
                            };
            this.ContentRepository.Remove(model);

            return this.Json(new { createdate = model.CreateDate, success = true });
        }

        /// <summary>
        /// Determines whether [is content admin].
        /// </summary>
        /// <returns>JSON of success = true.  Fallback on our authorize attribute.</returns>
        [HttpGet]
        public virtual ActionResult IsContentAdmin()
        {
            return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        /// The file content or 404.
        /// </returns>
        /// <exception cref="System.Web.HttpException">404;PhunCMS download path not found.</exception>
        [AllowAnonymous]
        public virtual ActionResult Download(string path)
        {
            var content = new ContentModel()
            {
                Path = path,
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url)
            };

            if (content.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {

                var folderName = content.Path.TrimStart('/').Replace("/", ".");
                if (string.IsNullOrEmpty(folderName) || folderName == "/")
                {
                    folderName = "AllContent";
                }

                var localFilePath = this.ContentRepository.GetFolder(content);

                return this.File(localFilePath, MimeTypes.GetContentType("zip"), folderName + "zip");
            }

            var result = this.ContentRepository.Retrieve(content, true);

            if (result.DataLength == null)
            {
                throw new HttpException(404, "PhunCms download path not found.");
            }

            if (result.DataStream != null)
            {
                return this.File(result.DataStream, MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)), result.FileName);
            }

            return this.File(result.Data, MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)), result.FileName);
        }

        /// <summary>
        /// View content.
        /// </summary>
        /// <returns>
        /// The file content or 404.
        /// </returns>  
        [HttpGet, AllowAnonymous]
        public virtual ActionResult Page()
        {
            var path = Request.QueryString["path"];
            if (string.IsNullOrEmpty(path))
            {
                path = (this.Request.Path + string.Empty).Trim().TrimEnd('/');
            }

            // if somehow, CMS resource url get routed to here then we intercept
            if (this.Config.IsResourceRoute(path))
            {
                var vf = new ResourceVirtualFile(path);
                vf.WriteFile(this.HttpContext);
                return null;
            }

            if (!path.EndsWith("_default"))
            {
                path = path + "/_default";
            }

            // when content is view as a page, attempt to auto load resources
            var result = this.Retrieve(path) as ContentResult;
            var file = new ResourcePathUtility();
            if (result != null && result.Content.IndexOf("</head>", StringComparison.OrdinalIgnoreCase) > 0)
            {
                result.Content = result.Content.Replace("</head>", file.PhunCmsRenderBundles() + "</head>");
            }

            return result;
        }

        /// <summary>
        /// Edits this instance.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>
        /// Edit view.
        /// </returns>
        [HttpGet]
        public virtual ActionResult Edit(string path)
        {
            var resourceProvider = new ResourcePathUtility();
            
            return this.View(resourceProvider.GetResourcePath("edit.cshtml"));
        }

        /// <summary>
        /// Lists the DYNATREE.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>DYNATREE result.</returns>
        public virtual ActionResult FileManagerDynatree(string path)
        {
            var model = new ContentModel
                            {
                                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url),
                                Path = string.IsNullOrEmpty(path) ? "/" : "/" + path.Trim('/') + "/"
                            };

            var result = this.ContentRepository.List(model);
            var myJsonResult = result.Select(item => new DynaTreeViewModel(item.Path)).ToList();

            // return extra root path
            if (model.Path == "/")
            {
                return this.Json(new DynaTreeViewModel("/") { children = myJsonResult, expand = true });
            }

            return this.Json(myJsonResult);
        }

        /// <summary>
        /// Files the browser.
        /// </summary>
        /// <returns>
        /// View result that display a file browser.
        /// </returns>
        [HttpGet]
        public virtual ActionResult FileManager()
        {
            var resourceProvider = new ResourcePathUtility();

            return this.View(resourceProvider.GetResourcePath("filemanager.cshtml"));
        }

        /// <summary>
        /// Files the upload.
        /// </summary>
        /// <param name="upload">The upload.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Result for file upload.
        /// </returns>
        [HttpPost]
        public virtual ActionResult FileManager(HttpPostedFileBase upload, string path)
        {
            var contentModel = new ContentModel()
            {
                Path = path,
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url),
                CreateBy = this.User.Identity.Name,
                ModifyBy = this.User.Identity.Name
            };

            // if it's a file path then get the folder
            if (!contentModel.Path.EndsWith("/"))
            {
                contentModel.Path = contentModel.Path.Substring(0, contentModel.Path.LastIndexOf('/'));
            }

            var fileName = (upload.FileName + string.Empty).Replace("/", "\\").Replace("\\\\", "\\");
            contentModel.Path = string.Concat(contentModel.Path, "/", fileName.IndexOf('/') >= 0 ? System.IO.Path.GetFileName(upload.FileName) : upload.FileName);

            if (string.IsNullOrEmpty(contentModel.Path) || string.Compare(contentModel.Path, "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentException("Cannot upload to root path.", "path");
            }

            contentModel.Data = upload.InputStream.ReadAll();

            this.ContentRepository.Save(contentModel);

            return this.FileManager();
        }

        /// <summary>
        /// Gets the current host.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="url">
        /// The URL.
        /// </param>
        /// <returns>
        /// The current host.
        /// </returns>
        protected internal virtual string GetCurrentHost(MapRouteConfiguration config, Uri url)
        {
            Uri requestUrl = url;
            var currentHost = (requestUrl.Host + string.Empty).Trim().ToLowerInvariant().Replace("www.", string.Empty);

            var splitHostName = currentHost.Split('.');
            if (config.DomainLevel > 1 && splitHostName.Length > 1)
            {
                var names = new List<string>(splitHostName);
                while (names.Count > this.ContentConfig.DomainLevel)
                {
                    names.RemoveAt(0);
                }

                currentHost = string.Join(".", names);
            }

            return currentHost;
        }
    }
}
