namespace Phun
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represent object to serialize back to dynatree.
    /// This is why the class is massively suppressed and funky (lowered case properties).
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class DynaTreeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynaTreeViewModel" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynaTreeViewModel(string key)
        {
            var tempTitle = (key + string.Empty).TrimEnd('/');
            this.key = key;
            this.title = tempTitle.Substring(tempTitle.LastIndexOf('/') + 1);
            this.isLazy = this.isFolder = key.EndsWith("/", StringComparison.OrdinalIgnoreCase);
            if (!this.isFolder && this.title.IndexOf('.') > 0)
            {
                var extension = this.title.IndexOf('.') > 0
                                    ? this.title.Substring(this.title.IndexOf('.')).Trim('.')
                                    : string.Empty;
                this.addClass = string.IsNullOrEmpty(extension) ? null : "ext_" + extension;
            }

            this.children = new List<DynaTreeViewModel>();
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string tooltip { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string key { get; set; }

        /// <summary>
        /// Gets or sets the add class.
        /// </summary>
        /// <value>
        /// The add class.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string addClass { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public IList<DynaTreeViewModel> children { get; set; }

        /// <summary>
        /// Gets or sets the rel.
        /// </summary>
        /// <value>
        /// The rel.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."), SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string rel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is folder; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public bool isFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is lazy.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is lazy; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public bool isLazy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DynaTreeViewModel"/> is activate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activate; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public bool activate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DynaTreeViewModel"/> is expand.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expand; otherwise, <c>false</c>.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public bool expand { get; set; }
    }
}
