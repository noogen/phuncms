namespace Phun
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;

    using Phun.Configuration;

    /// <summary>
    /// Get virtual file from resource.
    /// </summary>
    public class ResourceVirtualFile : VirtualFile
    {
        #region "Virtual file string"

        /// <summary>
        /// The scripts phuncms config js - Phun.Properties.scripts.phuncms.config.js
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private const string ScriptsphuncmsConfigJs = @"
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
        /// The resource path
        /// </summary>
        private readonly string resourcePath;

        /// <summary>
        /// The virtual file path, this is use for debugging purposes.
        /// </summary>
        private string virtualFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceVirtualFile" /> class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        /// <param name="resourcePath">The resource path.</param>
        public ResourceVirtualFile(string virtualPath, string resourcePath)
            : base(virtualPath)
        {
            this.resourcePath = resourcePath;
            this.virtualFilePath = virtualPath;
            this.Config = ConfigurationManager.GetSection("phuncms") as PhunCmsConfigurationSection;
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        protected internal PhunCmsConfigurationSection Config { get; set; }

        /// <summary>
        /// When overridden in a derived class, returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>
        /// A read-only stream to the virtual file.
        /// </returns>
        public override System.IO.Stream Open()
        {
            var result = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.resourcePath);
            if (result == null && this.resourcePath.EndsWith("scripts.phuncms.config.js"))
            {
                var fileString =
                    string.Format(ScriptsphuncmsConfigJs, this.Config.ResourceRouteNormalized, this.Config.RouteNormalized)
                        .Replace("[", "{")
                        .Replace("]", "}");

                result = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(fileString));
            }

            if (result == null)
            {
                throw new HttpException(404, "Resource virtual file not found.");
            }

            return result;
        }


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
        public string PhunCmsRenderBundles(bool includeJquery = true, bool includeJqueryui = true, bool includeBackbone = true, bool includeCkEditor = true, bool includeEditorInit = true)
        {
            // this code is in here because it needs to be dynamic base on resource and content route
            var files = new StringBuilder(string.Format(@"
<link rel='stylesheet' href='/{0}/css/create_ui/css/create-ui.css'/>
<link rel='stylesheet' href='/{0}/css/midgard_notifications/midgardnotif.css'/>
<link rel='stylesheet' href='/{0}/css/font_awesome/css/font-awesome.css'/>", this.Config.ResourceRouteNormalized));

            if (includeJquery)
            {
                files.AppendLine(
                    string.Format("<script type='text/javascript' src='/{0}/Scripts/jquery.js'></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeJqueryui)
            {
                files.AppendLine(string.Format("<script type='text/javascript' src='/{0}/Scripts/jqueryui.js'></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeBackbone)
            {
                files.AppendLine(string.Format("<script type='text/javascript' src='/{0}/Scripts/underscore.js'></script>", this.Config.ResourceRouteNormalized));
                files.AppendLine(string.Format("<script type='text/javascript' src='/{0}/Scripts/backbone.js'></script>", this.Config.ResourceRouteNormalized));
            }

            if (includeCkEditor)
            {
                files.AppendLine(string.Format("<script type='text/javascript' src='/{0}/Scripts/ckeditor/ckeditor.js'></script>", this.Config.ResourceRouteNormalized));
            }

            files.AppendLine(string.Format(@"
<script type='text/javascript' src='/{0}/Scripts/phuncms.config.js'></script>
<script type='text/javascript' src='/{0}/Scripts/vie.js'></script>
<script type='text/javascript' src='/{0}/Scripts/create.js'></script>
<script type='text/javascript' src='/{0}/Scripts/phuncms.js'></script>", this.Config.ResourceRouteNormalized));

            if (includeEditorInit)
            {
                files.AppendLine(@"<script type='text/javascript'>$(document).ready(function() { if (typeof(PhunCms) != 'undefined' && PhunCms.contentsToLoad <= 0) PhunCms.initEditor() });</script>");
            }

            return files.ToString();
        }                                              
    }
}
