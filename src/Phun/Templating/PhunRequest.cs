namespace Phun.Templating
{
    using System.IO;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Phun request object.
    /// </summary>
    public class PhunRequest : IRequest
    {
        private readonly HttpRequestBase request;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunRequest" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PhunRequest(HttpContextBase context)
        {
            this.request = context.Request;
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
            return cookie != null ? cookie.Value : string.Empty;
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
            return this.request.Headers.Get(name) + string.Empty;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The query string value.
        /// </returns>
        public string getQuery(string name)
        {
            return this.request.QueryString.Get(name) + string.Empty;
        }

        /// <summary>
        /// Gets the form value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// Form value.
        /// </returns>
        public string getForm(string name)
        {
            return this.request.Form.Get(name) + string.Empty;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <returns>
        /// The string content of post or put.
        /// </returns>
        public string getContent()
        {
            string documentContents = string.Empty;
            if (this.request.InputStream != null)
            {
                using (var receiveStream = this.request.InputStream)
                {
                    receiveStream.Seek(0, SeekOrigin.Begin);
                    using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }
            }

            return documentContents;
        }
    }
}
