namespace Phun.Templating
{
    using System.Web;

    /// <summary>
    /// Phun request object.
    /// </summary>
    public class PhunRequest : IRequest
    {
        private readonly HttpRequestBase request;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunRequest"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public PhunRequest(HttpRequestBase request)
        {
            this.request = request;
            this.isLocal = this.request.IsLocal;
            this.url = this.request.Url;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="bool" /> is isLocal.
        /// </summary>
        /// <value>
        /// return <c>true</c> if isLocal; otherwise, <c>false</c>.
        /// </value>
        public bool isLocal { get; set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public System.Uri url { get; set; }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The cookie value.
        /// </returns>
        public string getCookie(string name)
        {
            var cookie = this.request.Cookies.Get(name);
            return cookie != null ? cookie.Value : null;
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The header value.
        /// </returns>
        public string getHeader(string name)
        {
            return this.request.Headers.Get(name);
        }
    }
}
