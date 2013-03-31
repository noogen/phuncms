namespace Phun
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Data;

    /// <summary>
    /// The content connector interface.
    /// </summary>
    public interface IContentConnector
    {
        /// <summary>
        /// Gets or sets the content config.
        /// </summary>
        /// <value>
        /// The content config.
        /// </value>
        IMapRouteConfiguration ContentConfig { get; set; }

        /// <summary>
        /// Gets or sets the content repository.
        /// </summary>
        /// <value>
        /// The content repository.
        /// </value>
        IContentRepository ContentRepository { get; set; }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        ICmsConfiguration Config { get; set; }

        /// <summary>
        /// Create the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        /// <exception cref="System.Web.HttpException">500;Cannot create or overwrite an existing content of path:  + path</exception>
        ContentModel Create(string path, string data, Uri uri);

        /// <summary>
        /// Retrieves this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// The content.
        /// </returns>
        /// <exception cref="System.Web.HttpException">404;PhunCMS view content path not found.</exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpGet, AllowAnonymous]
        ContentModel Retrieve(string path, Uri uri);

        /// <summary>
        /// Update the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="data">The data.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        ContentModel CreateOrUpdate(string path, string data, Uri uri);

        /// <summary>
        /// Creates the or update.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="data">The data.</param>
        /// <returns>Content model result.</returns>
        ContentModel CreateOrUpdate(string path, Uri uri, byte[] data);

        /// <summary>
        /// Deletes the specified model.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content model result.</returns>
        /// <exception cref="System.Web.HttpException">500;Unable to delete protected path '/'.</exception>
        ContentModel Delete(string path, Uri uri);

        /// <summary>
        /// Histories this instance.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// Path content history.
        /// </returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.</exception>
        IQueryable<ContentModel> History(string path, Uri uri);

        /// <summary>
        /// Get the history the data.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// The history data content.
        /// </returns>
        /// <exception cref="System.Web.HttpException">500;History can only be retrieve for file.
        /// or
        /// 401;Content does not exists:  + path</exception>
        ContentModel HistoryData(ContentModel model, Uri uri);

        /// <summary>
        /// Lists the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>Content list.</returns>
        IQueryable<ContentModel> List(string path, Uri uri);

        /// <summary>
        /// View content.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context.
        /// </param>
        void RenderPage(HttpContextBase httpContext);

        /// <summary>
        /// Gets the current host.
        /// </summary>                                 
        /// <param name="url">The URL.</param>
        /// <returns>
        /// The current host.
        /// </returns>
        string GetTenantHost(Uri url);

        /// <summary>
        /// Applies the path convention.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Normalized path.</returns>
        string Normalize(string path);
    }
}
