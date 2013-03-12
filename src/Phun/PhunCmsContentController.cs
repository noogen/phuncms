namespace Phun
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Data;

    /// <summary>
    /// Simple CMS ContentController.
    /// </summary>
    [CmsAdminAuthorize]
    public class PhunCmsContentController : PhunCmsController
    {
        /// <summary>
        /// My content config
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        internal MapRouteConfiguration MyContentConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCmsContentController"/> class.
        /// </summary>
        public PhunCmsContentController() : base()
        {
            this.MyContentConfig =
                this.Config.ContentMaps.FirstOrDefault(
                    c =>
                    string.Compare(c.RouteNormalized, this.Config.ContentRouteNormalized, StringComparison.OrdinalIgnoreCase)
                    == 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunCmsContentController" /> class.
        /// </summary>
        /// <param name="contentPathAdminHandler">The content path admin handler.</param>
        public PhunCmsContentController(IContentPathPermissionHandler contentPathAdminHandler)
            : this()
        {
            this.ContentPathPermissionHandler = contentPathAdminHandler;
        }

        /// <summary>
        /// Gets or sets the content path admin handler.
        /// </summary>
        /// <value>
        /// The content path admin handler.
        /// </value>
        protected IContentPathPermissionHandler ContentPathPermissionHandler { get; set; }

        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public override MapRouteConfiguration ContentConfig
        {
            get { return this.MyContentConfig; }
        }

        /// <summary>
        /// Special update and insert method for micro/path-based permission management.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <returns>
        /// Allow for updating of path.
        /// </returns>
        /// <exception cref="System.Web.HttpException">401;Request update to path ' + path + ' is unauthorized.</exception>
        [ValidateInput(false), AllowAnonymous]
        public virtual ActionResult Upsert(string path, string data)
        {
            var model = new ContentModel()
            {
                Path = path,
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url),
                Data = System.Text.Encoding.UTF8.GetBytes(data)
            };

            if (this.ContentPathPermissionHandler != null && this.ContentPathPermissionHandler.IsAdmin(this, model))
            {
                return this.Update(path, data, null);
            }

            throw new HttpException(401, "Request upsert to path '" + path + "' is unauthorized.");
        }

        /// <summary>
        /// Uploads the specified file.
        /// </summary>
        /// <param name="upload">The upload.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.HttpException">401;Request upload to path ' + path + ' is unauthorized.</exception>
        [AllowAnonymous]
        public virtual ActionResult Upload(HttpPostedFileBase upload, string path)
        {
            var contentModel = new ContentModel()
            {
                Path = path,
                Host = this.GetCurrentHost(this.ContentConfig, this.Request.Url)
            };

            // if it's a file path then get the folder
            if (!contentModel.Path.EndsWith("/"))
            {
                contentModel.Path = contentModel.Path.Substring(0, contentModel.Path.LastIndexOf('/'));
            }

            var fileName = (upload.FileName + string.Empty).Replace("/", "\\").Replace("\\\\", "\\");
            contentModel.Path = string.Concat(contentModel.Path, "/", fileName.IndexOf('/') >= 0 ? System.IO.Path.GetFileName(upload.FileName) : upload.FileName);

            if (this.ContentPathPermissionHandler != null && this.ContentPathPermissionHandler.IsAdmin(this, contentModel))
            {
                return this.FileManager(upload, path);
            }

            throw new HttpException(401, "Request upload to path '" + path + "' is unauthorized.");
        }
    }
}
