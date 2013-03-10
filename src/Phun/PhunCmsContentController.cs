namespace Phun
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Phun.Configuration;

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
        /// Gets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public override MapRouteConfiguration ContentConfig
        {
            get { return this.MyContentConfig; }
        }
    }
}
