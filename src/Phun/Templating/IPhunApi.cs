namespace Phun.Templating
{
    using System.Security.Principal;

    /// <summary>
    /// Provide a http context for the scripting template.
    /// </summary>
    public interface IPhunApi
    {
        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        IRequest request { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        IResponse response { get; set; }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        ICache cache { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        IPrincipal user { get; set; }

        /// <summary>
        /// Gets or sets the trace.
        /// </summary>
        /// <value>
        /// The trace.
        /// </value>
        ITrace trace { get; set; }

        /// <summary>
        /// Requires the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        object require(string name);

        /// <summary>
        /// Gets the tenant host.
        /// </summary>
        /// <value>
        /// The tenant host.
        /// </value>
        string TenantHost { get; }

        /// <summary>
        /// Partials the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Render partial without going through the view engine.</returns>
        string partial(string name);

        /// <summary>
        /// Partialeditables the specified name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns>
        /// Render editable partial without going through the view engine.
        /// </returns>
        string partialeditable(string tagName, string contentName, object attributes);

        /// <summary>
        /// Bundleses the specified include jquery.
        /// </summary>
        /// <returns>CK editor partial edit bundles</returns>
        string bundles();

        /// <summary>
        /// URLs the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>return resource url.</returns>
        string resourceurl(string path);

        /// <summary>
        /// Contenturls the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string contenturl(string path);

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        Phun.Data.ContentModel FileModel { get; set; }

        /// <summary>
        /// Exceptions the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void exception(string message);
    }

    /// <summary>
    /// Cache object.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        object get(string key);

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void set(string key, object value);
    }

    /// <summary>
    /// Request object.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="bool" /> is isLocal.
        /// </summary>
        /// <value>
        /// return <c>true</c> if isLocal; otherwise, <c>false</c>.
        /// </value>
        bool isLocal { get; set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        System.Uri url { get; set; }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The cookie value.</returns>
        string getCookie(string name);

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The header value.</returns>
        string getHeader(string name);
    }
}
