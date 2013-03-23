﻿namespace Phun.Templating
{
    using System.Security.Principal;

    /// <summary>
    /// Provide a http context for the scripting template.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        IRequest request { get; set; }

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