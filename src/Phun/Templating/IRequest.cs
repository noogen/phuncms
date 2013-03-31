namespace Phun.Templating
{
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

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The query string value.</returns>
        string getQuery(string name);

        /// <summary>
        /// Gets the form value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Form value.</returns>
        string getForm(string name);

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <returns>The string content of post or put.</returns>
        string getContent();
    }
}
