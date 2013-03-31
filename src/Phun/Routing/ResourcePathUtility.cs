namespace Phun.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Extensions;

    /// <summary>
    /// CMS resource path utility.
    /// </summary>
    public class ResourcePathUtility 
    {
        #region "Virtual file string"

        /// <summary>
        /// The scripts phuncms config js - Phun.Properties.scripts.phuncms.config.js
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        internal const string ScriptsphuncmsConfigJs = @"
window.PhunCms = (function (PhunCms, $, undefined) [
    // pouplate global object 
    PhunCms.contentsToLoad = 0;
    PhunCms.resourceRoute = '{0}';
    PhunCms.contentRoute = '{1}';
    PhunCms.editor = 'ckeditor';
    PhunCms.editorInitialized = false;

    PhunCms.initEditor = function()[
        if (PhunCms.editorInitialized) return false;

        PhunCms.editorInitialized = true;

        // make sure have permission to edit
        $.ajax([
            url: '/' + PhunCms.contentRoute + '/IsContentAdmin',
            type: 'GET',
            async: false,
            cache: false,
            success: function(data) [
                if (typeof(data) === 'undefined' || typeof(data.success) === 'undefined' || data.success === false) return;

                $('body').midgardCreate([
                    url: function () [
                        return 'javascript:false;';
                    ]
                ]);

                if (window.PhunCms.editor == 'hallo') [
                    // hallo is default, do nothing
                ]
                else if (window.PhunCms.editor == 'ckeditor') [
                    $('body').midgardCreate('configureEditor', 'ckeditor', 'ckeditorWidget', []);
                    $('body').midgardCreate('setEditorForProperty', 'default', 'ckeditor');
                ] // other editors? you're on your own buddy, for now
            ]
        ]);

        return true;  
    ];

    return PhunCms;
])(window.PhunCms || [], jQuery);";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePathUtility"/> class.
        /// </summary>
        public ResourcePathUtility()
        {
            this.Config = Bootstrapper.Config;
            this.ContentConfig = Bootstrapper.ContentConfig;
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        protected internal virtual ICmsConfiguration Config { get; set; }

        /// <summary>
        /// Gets or sets the content config.
        /// </summary>
        /// <value>
        /// The content config.
        /// </value>
        protected internal virtual IMapRouteConfiguration ContentConfig { get; set; }

        /// <summary>
        /// Renders the simple CMS bundles.
        /// </summary>
        /// <param name="includeJquery">if set to <c>true</c> [include jquery].</param>
        /// <param name="includeJqueryui">if set to <c>true</c> [include jqueryui].</param>
        /// <param name="includeBackbone">if set to <c>true</c> [include backbone].</param>
        /// <param name="includeCkEditor">if set to <c>true</c> [include ck editor].</param>
        /// <param name="includeEditorInit">if set to <c>true</c> [include editor init].</param>
        /// <returns>
        /// Simple cms resource bundles with default ckeditor.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public virtual string PhunCmsRenderBundles(bool includeJquery = true, bool includeJqueryui = true, bool includeBackbone = true, bool includeCkEditor = true, bool includeEditorInit = true)
        {
            // this code is in here because it needs to be dynamic base on resource and content route
            var files = new StringBuilder(string.Format("<link rel=\"stylesheet\" href=\"/{0}/css/create_ui/css/create-ui.css\"/><link rel=\"stylesheet\" href=\"/{0}/css/midgard_notifications/midgardnotif.css\"/><link rel=\"stylesheet\" href=\"/{0}/css/font_awesome/css/font-awesome.css\"/>", this.Config.ResourceRouteNormalized));

            if (includeJquery)
            {
                files.AppendLine(
                    string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/jquery.js\"></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeJqueryui)
            {
                files.AppendLine(string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/jqueryui.js\"></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeBackbone)
            {
                files.AppendLine(string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/underscore.js\"></script>", this.Config.ResourceRouteNormalized));
                files.AppendLine(string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/backbone.js\"></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeCkEditor)
            {
                files.AppendLine(string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/ckeditor/ckeditor.js\"></script>", this.Config.ResourceRouteNormalized));
            }

            files.AppendLine(string.Format("<script type=\"text/javascript\" src=\"/{0}/scripts/phuncms.config.js\"></script><script type=\"text/javascript\" src=\"/{0}/scripts/vie.js\"></script><script type=\"text/javascript\" src=\"/{0}/scripts/create.js\"></script><script type=\"text/javascript\" src=\"/{0}/scripts/phuncms.js\"></script>", this.Config.ResourceRouteNormalized));

            if (includeEditorInit)
            {
                files.AppendLine("<script type=\"text/javascript\">$(document).ready(function() { if (typeof(PhunCms) != 'undefined' && PhunCms.contentsToLoad <= 0) PhunCms.initEditor() });</script>");
            }

            return files.ToString();
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
            var config = this.Config ?? Bootstrapper.Config;
            Uri requestUrl = url;
            var currentHost = (requestUrl.Host + string.Empty).Trim().ToLowerInvariant().Replace("www.", string.Empty);

            var splitHostName = currentHost.Split('.');
            if (config.DomainLevel > 1 && splitHostName.Length > 1)
            {
                var names = new List<string>(splitHostName);
                while (names.Count > config.DomainLevel)
                {
                    names.RemoveAt(0);
                }

                currentHost = string.Join(".", names);
            }

            return currentHost;
        }

        /// <summary>
        /// Gets the resource path.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>Resource path.</returns>
        protected internal virtual string GetResourcePath(string resource)
        {
            return string.Format("/{0}/{1}", this.Config.ResourceRouteNormalized, resource.Trim('/'));
        }

        /// <summary>
        /// Phuns the partial.
        /// </summary>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="url">The URL.</param>
        /// <returns>
        /// Partial content.
        /// </returns>
        /// <exception cref="System.ArgumentException">contentName is required.</exception>
        protected internal virtual string PhunPartial(string contentName, Uri url)
        {
            if (string.IsNullOrEmpty(contentName))
            {
                throw new ArgumentException("contentName is required.");
            }

            var result = string.Empty;
            var config = this.ContentConfig ?? Bootstrapper.ContentConfig;
            var content = new ContentModel()
            {
                Path = this.Normalize(
                          "/page" + (contentName.Contains("/") ? contentName : url.AbsolutePath + "/" + contentName)),
                Host = this.GetTenantHost(url)
            };

            config.ContentRepository.Retrieve(content, true);
            if (content.DataLength != null)
            {
                content.SetDataFromStream();
                result = System.Text.Encoding.UTF8.GetString(content.Data).GetHtmlBody();
            }

            return result;
        }

        /// <summary>
        /// Applies the path convention.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The normalized path.</returns>
        public virtual string Normalize(string path)
        {
            // make sure path has all valid forward slash
            var myValue = string.Concat('/', (path + string.Empty).Replace("\\", "/").Replace("//", "/").TrimStart('/'));

            // if path is a folder, then normalize or slug the entire path
            if (myValue.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return myValue.ToSeoName();
            }

            // if it is a file, normalized the directory name only
            var parentPath = VirtualPathUtility.GetDirectory(myValue).ToSeoName();
            var fileName = VirtualPathUtility.GetFileName(myValue);

            return string.Concat(parentPath, fileName).TrimStart('~');
        }

        /// <summary>
        /// Phuns the partial editable.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>Partial editable.</returns>
        public virtual string PhunPartialEditable(Uri url, string tagName, string contentName, object htmlAttributes)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<{0} about=\"{1}\"", tagName, contentName);
            if (htmlAttributes != null)
            {
                var myAttributes = new RouteValueDictionary(htmlAttributes);
                foreach (var key in myAttributes.Keys)
                {
                    sb.AppendFormat("{0}=\"{1}\"", key, myAttributes[key]);
                }
            }

            sb.Append(">");
            var data = this.PhunPartial(contentName, url);
            sb.Append(string.Concat("<div property=\"content\">", data, "</div>"));
            sb.AppendFormat("</{0}>", tagName);
            return sb.ToString();
        }
    }
}
