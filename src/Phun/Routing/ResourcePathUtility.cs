namespace Phun.Routing
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;

    using Phun.Configuration;

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
        /// Gets the resource path.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>Resource path.</returns>
        protected internal virtual string GetResourcePath(string resource)
        {
            return string.Format("/{0}/{1}", this.Config.ResourceRouteNormalized, resource.Trim('/'));
        }
    }
}
