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
        protected CmsController()
        {
            this.ContentConnector = new ContentConnector();
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
            var result = this.ContentConnector.Retrieve(path, this.Request.Url);

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
                "/{0}/edit.htm?contentPath={1}&path={2}&_={3}",
                    this.ContentConnector.Config.ResourceRouteNormalized,
                    this.ContentConnector.Config.ContentRouteNormalized,
                    this.ContentConnector.ApplyPathConvention(path),
                    DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            return this.Redirect(url);
        }

        /// <summary>
        /// Lists the DYNATREE.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>DYNATREE result.</returns>
        public virtual ActionResult FileManagerDynatree(string path)
        {
            path = this.ContentConnector.ApplyPathConvention(path);
            var myJsonResult = this.ContentConnector.List(path, this.Request.Url).Select(item => new DynaTreeViewModel(item.Path)).ToList();

            // return extra root path
            return (string.IsNullOrEmpty(path) || path == "/") 
                    ? this.Json(new DynaTreeViewModel("/") { children = myJsonResult, title = "/", key = "/", expand = true, isFolder = true, isLazy = false }) 
                    : this.Json(myJsonResult);
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
            var url = string.Format(
                "/{0}/filemanager.htm?contentPath={1}&_={2}",
                this.ContentConnector.Config.ResourceRouteNormalized,
                this.ContentConnector.Config.ContentRouteNormalized,
                DateTime.Now.Ticks);

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
            var contentModel = new ContentModel()
            {
                Path = this.ContentConnector.ApplyPathConvention(path),
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

            this.ContentConnector.CreateOrUpdate(contentModel.Path, upload.InputStream.ReadAll(), this.Request.Url);

            return this.FileManager();
        }
    }
}
