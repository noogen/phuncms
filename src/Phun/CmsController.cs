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
        /// Retrieves the secure.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Content result.
        /// </returns>
        [HttpGet]
        public virtual ActionResult RetrieveSecure(string path)
        {
            var result = this.ContentConnector.Retrieve(path, this.Request.Url);
            if (this.TrySet304(result))
            {
                this.Response.Flush();
                return new EmptyResult();
            }

            // determine if we should return text/html for different text file types so the browser doesn't complain about security issue
            var forEdit = !string.IsNullOrEmpty(this.Request.QueryString["forEdit"]);
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
        /// Histories this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Path content history.
        /// </returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.</exception>
        [HttpPost]
        public virtual ActionResult History(string path)
        {
            var result = this.ContentConnector.History(path, this.Request.Url);
            return this.Json(result.ToList());
        }

        /// <summary>
        /// Get the history data.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The history data content.</returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.
        /// or
        /// 401;Content does not exists:  + path</exception>
        [HttpPost]
        public virtual ActionResult HistoryRetrieve(ContentModel model)
        {
            var result = this.ContentConnector.HistoryData(model, this.Request.Url);

            return result.DataStream != null
                       ? (ActionResult)
                         this.File(
                             result.DataStream,
                             MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)),
                             result.FileName)
                       : this.File(
                           result.Data,
                           MimeTypes.GetContentType(System.IO.Path.GetExtension(result.Path)),
                           result.FileName);
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
            // is static content
            if (Bootstrapper.StaticContentRegEx.IsMatch(this.Request.Path))
            {
                return this.Retrieve(this.Request.Path);
            }

            this.ContentConnector.RenderPage(this.Request.RequestContext.HttpContext);
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
        /// Lists the DYNATREE.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="isRootPath">if set to <c>true</c> [is root path].</param>
        /// <returns>
        /// DYNATREE result.
        /// </returns>
        public virtual ActionResult FileManagerDynatree(string path, bool? isRootPath)
        {
            path = this.ContentConnector.Normalize(path);
            var myJsonResult = this.ContentConnector.List(path, this.Request.Url).Select(item => new DynaTreeViewModel(item.Path)).ToList();

            // return extra root path
            return (string.IsNullOrEmpty(path) || path == "/" || (isRootPath.HasValue && isRootPath.Value))
                    ? this.Json(new DynaTreeViewModel(path) { children = myJsonResult, title = path, key = path, expand = true, isFolder = true, isLazy = false }) 
                    : this.Json(myJsonResult);
        }

        /// <summary>
        /// Files the browser.
        /// </summary>
        /// <param name="hashPath">The hash path.</param>
        /// <returns>
        /// View result that display a file browser.
        /// </returns>
        [HttpGet]
        public virtual ActionResult FileManager(string hashPath)
        {
            var url = string.Format(
                "{0}?contentPath={1}&_={2}#{3}",
                this.ContentConnector.Config.FileManager.ToLowerInvariant()
                    .Replace("[resourceroute]", this.ContentConnector.Config.ResourceRouteNormalized)
                    .Replace("[contentroute]", this.ContentConnector.Config.ContentRouteNormalized),
                this.ContentConnector.Config.ContentRouteNormalized,
                DateTime.Now.Ticks,
                hashPath);

            return this.Redirect(url);
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
            return this.FileManager(this.Upload(upload, path));
        }

        /// <summary>
        /// Files the browser.
        /// </summary>
        /// <param name="upload">The upload.</param>
        /// <param name="path">The path.</param>
        /// <returns>Result for file upload.</returns>
        [HttpPost]
        public virtual ActionResult FileBrowser(HttpPostedFileBase upload, string path)
        {
            var hashPath = this.FileManager(this.Upload(upload, path));

            var url = string.Format(
                "/{0}/filebrowser.htm?contentPath={1}&_={2}#{3}",
                this.ContentConnector.Config.ResourceRouteNormalized,
                this.ContentConnector.Config.ContentRouteNormalized,
                DateTime.Now.Ticks,
                hashPath);

            return this.Redirect(url);
        }

        /// <summary>
        /// Uploads the specified upload.
        /// </summary>
        /// <param name="upload">The upload.</param>
        /// <param name="path">The path.</param>
        /// <returns>Parent path.</returns>
        protected virtual string Upload(HttpPostedFileBase upload, string path)
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

            var fileName = (upload.FileName + string.Empty).Replace("\\", "/").Replace("//", "/");
            contentModel.Path = string.Concat(contentModel.Path, "/", fileName.IndexOf('/') >= 0 ? System.IO.Path.GetFileName(upload.FileName) : upload.FileName);

            this.ContentConnector.CreateOrUpdate(contentModel.Path, this.Request.Url, upload.InputStream.ReadAll());

            return contentModel.ParentPath;
        }

        /// <summary>
        /// Tries the set304.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Attempt to set 304 response.</returns>
        protected virtual bool TrySet304(ContentModel content)
        {
            var context = this.HttpContext;

            if (this.ContentConnector.Config.DisableResourceCache || !Bootstrapper.StaticContentRegEx.IsMatch(content.FileName))
            {
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetExpires(DateTime.MinValue);
                return false;
            }

            var currentDate = content.ModifyDate ?? DateTime.Now;
            context.Response.Cache.SetLastModified(currentDate);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(1));

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
