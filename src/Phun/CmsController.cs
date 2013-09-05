namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
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
    using Phun.Templating;

    /// <summary>
    /// Default controller.
    /// </summary>
    public abstract class CmsController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmsController" /> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        protected CmsController(IContentConnector connector)
        {
            this.ContentConnector = connector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmsController"/> class.
        /// </summary>
        protected CmsController() : this(new ContentConnector())
        {
        }

        /// <summary>
        /// Gets or sets the content repository.
        /// </summary>
        /// <value>
        /// The content repository.
        /// </value>
        public IContentConnector ContentConnector { get; set; }

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
            var result = this.ContentConnector.Create(path, data, this.Request.Url);

            return !string.IsNullOrEmpty(returnUrl)
                       ? (ActionResult)this.Redirect(returnUrl)
                       : this.Json(new { CreateDate = result.CreateDate, success = true });
        }

        /// <summary>
        /// Retrieves this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The content.</returns>
        /// <exception cref="System.Web.HttpException">404;PhunCMS view content path not found.</exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpGet, AllowAnonymous]
        public virtual ActionResult Retrieve(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(403, "Invalid download path: " + path);
            }

            if (path.EndsWith(".vash", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException(404, "Path not found: " + path);
            }

            return this.RetrieveSecure(path);
        }

        /// <summary>
        /// Securely retrieve the content template.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Content result.
        /// </returns>
        [HttpGet]
        public virtual ActionResult RetrieveSecure(string path)
        {
            var forEdit = !string.IsNullOrEmpty(this.Request.QueryString["forEdit"]);

            // cannot edit folder, must ment vash
            if (forEdit && (path + string.Empty).EndsWith("/"))
            {
                path += "_default.vash";
            }

            var result = this.ContentConnector.Retrieve(path, this.Request.Url);
            if (this.TrySet304(result))
            {
                this.Response.Flush();
                return new EmptyResult();
            }

            // determine if we should return text/html for different text file types so the browser doesn't complain about security issue
            this.Response.AddHeader("Content-Disposition", "attachment; filename=" + result.FileName);

            return result.DataStream != null
                       ? (ActionResult)
                         this.File(
                             result.DataStream,
                             forEdit ? "text/html" : MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)))
                       : this.File(
                           result.Data,
                           forEdit ? "text/html" : MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)));
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
            var result = this.ContentConnector.CreateOrUpdate(path, data, this.Request.Url);

            return !string.IsNullOrEmpty(returnUrl)
                       ? (ActionResult)this.Redirect(returnUrl)
                       : this.Json(new { ModifyDate = result.ModifyDate, success = true });
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
            this.ContentConnector.Delete(path, this.Request.Url);

            return this.Json(new { success = true });
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
        /// View content.
        /// </summary>
        /// <returns>
        /// The file content or 404.
        /// </returns>  
        [HttpGet, AllowAnonymous]
        public virtual ActionResult Page()
        {
            if (!string.IsNullOrWhiteSpace(this.Request.QueryString["redirectToEdit"]))
            {
                return this.Edit(this.Request.Path);
            }

            // is static content
            if (Bootstrapper.Default.ContentRegEx.IsMatch(this.Request.Path))
            {
                return this.Retrieve(this.Request.Path);
            }

            var result = this.ContentConnector.RenderPage(this.Request.RequestContext.HttpContext);

            // cache page for 10th of one hour or 6 minutes
            if (result != null && !this.TrySet304(result, 0.1))
            {
                return result.DataStream != null
                            ? (ActionResult)this.File(result.DataStream, "text/html")
                            : this.File(result.Data, "text/html");
            }

            return new EmptyResult();
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
            // cannot edit folder, must ment default page
            if ((path + string.Empty).EndsWith("/"))
            {
                path += "_default.vash";
            }

            var url = string.Format(
                "{0}?contentPath={1}&path={2}&_={3}",
                this.ContentConnector.Config.FileEditor.ToLowerInvariant()
                    .Replace("[resourceroute]", this.ContentConnector.Config.ResourceRouteNormalized)
                    .Replace("[contentroute]", this.ContentConnector.Config.ContentRouteNormalized),
                this.ContentConnector.Config.ContentRouteNormalized,
                    this.ContentConnector.Normalize(path),
                    DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            return this.Redirect(url);
        }

        /// <summary>
        /// Lists the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public virtual ActionResult List(string path)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            path = this.ContentConnector.Normalize(path);
            var myJsonResult = this.ContentConnector.List(path, this.Request.Url).ToList();

            // return extra root path
            return this.Json(myJsonResult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <returns>Result of created page</returns>
        [HttpPost, ValidateInput(false)]
        public virtual ActionResult CreatePage(string path, string data)
        {
            if (!path.ToLowerInvariant().EndsWith("_default.htm") || !path.ToLowerInvariant().EndsWith("_default.vash"))
            {
                throw new HttpException(500, "Page path must ends with _default.htm or _default.vash: " + path);
            }

            var model = new ContentModel() { Path = this.ContentConnector.Normalize(path) };
            var folderPath = model.ParentPath;

            this.Create(folderPath + "app.js", string.Empty, null);
            this.Create(folderPath + "app.css", string.Empty, null);
            return this.Create(model.Path, data, null);
        }

        /// <summary>
        /// Files the browser.
        /// </summary>
        /// <returns>
        /// View result that display a file browser.
        /// </returns>
        [HttpGet]
        public virtual ActionResult FileManager(string mode)
        {
            var url = string.Format(
                "{0}?contentPath={1}&mode={2}&_={3}",
                this.ContentConnector.Config.FileManager.ToLowerInvariant()
                    .Replace("[resourceroute]", this.ContentConnector.Config.ResourceRouteNormalized)
                    .Replace("[contentroute]", this.ContentConnector.Config.ContentRouteNormalized),
                this.ContentConnector.Config.ContentRouteNormalized,
                string.IsNullOrWhiteSpace(mode) ? "all" : mode,
                DateTime.Now.Ticks);

            return this.Redirect(url);
        }

        /// <summary>
        /// Get the Page manager.
        /// </summary>
        /// <returns>Page Manager redirect</returns>
        [HttpGet]
        public virtual ActionResult PageManager()
        {
            return this.FileManager("page");
        }

        /// <summary>
        /// Get the File Browser.
        /// </summary>
        /// <returns>File Browser redirect</returns>
        [HttpGet]
        public virtual ActionResult FileBrowser()
        {
            return this.FileManager("browser");
        }

        /// <summary>
        /// Put file from file browser.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult FileManagerUpload(HttpPostedFileBase[] files, string path)
        {
            var resultUrl = this.Upload(this.Request.Files[0], path);

            return this.Json(new {success = true});
        }

        /// <summary>
        /// Uploads the specified upload.
        /// </summary>
        /// <param name="file">The upload.</param>
        /// <param name="path">The path.</param>
        /// <returns>Parent path.</returns>
        protected virtual string Upload(HttpPostedFileBase file, string path)
        {
            var contentModel = new ContentModel()
            {
                Path = this.ContentConnector.Normalize(path),
                CreateBy = this.User.Identity.Name,
                ModifyBy = this.User.Identity.Name
            };


            // if it's a file path then get the folder
            if (!contentModel.Path.EndsWith("/"))
            {
                contentModel.Path = contentModel.Path.Substring(0, contentModel.Path.LastIndexOf('/'));
            }

            var fileName = (file.FileName + string.Empty).Replace("\\", "/").Replace("//", "/");
            contentModel.Path = string.Concat(contentModel.Path, "/", fileName.IndexOf('/') >= 0 ? System.IO.Path.GetFileName(file.FileName) : file.FileName);

            this.ContentConnector.CreateOrUpdate(contentModel.Path, this.Request.Url, file.InputStream.ReadAll());

            return contentModel.ParentPath;
        }

        /// <summary>
        /// Tries the set304.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>True if content has not been modified since last retrieve.</returns>
        protected virtual bool TrySet304(ContentModel content, double hours = 24)
        {
            var context = this.HttpContext;

            if (this.ContentConnector.Config.DisableResourceCache || !Bootstrapper.Default.ContentRegEx.IsMatch(content.FileName))
            {
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetExpires(DateTime.MinValue);
                return false;
            }

            // allow static file to access core
            context.Response.AddHeader("Access-Control-Allow-Origin", "*"); 
            var currentDate = content.ModifyDate ?? DateTime.Now;
            context.Response.Cache.SetLastModified(currentDate);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddHours(hours));

            DateTime previousDate;
            string data = context.Request.Headers["If-Modified-Since"] + string.Empty;
            if (DateTime.TryParse(data, out previousDate))
            {
                if (currentDate > previousDate.AddMilliseconds(100))
                {
                    context.Response.StatusCode = 304;
                    context.Response.StatusDescription = "Not Modified";
                    return true;
                }
            }

            return false;
        }
    }
}
