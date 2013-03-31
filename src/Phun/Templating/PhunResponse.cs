namespace Phun.Templating
{
    using System.Web;

    /// <summary>
    /// Phun response.
    /// </summary>
    public class PhunResponse : IResponse
    {                        
        private readonly HttpContextBase context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunResponse"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PhunResponse(HttpContextBase context)
        {
            this.context = context;
        }

        /// <summary>
        /// Writes the specified chars.
        /// </summary>
        /// <param name="chars">The chars.</param>
        public void write(string[] chars)
        {
            if (chars == null || chars.Length <= 0)
            {
                return;
            }

            foreach (string t in chars)
            {
                this.context.Response.Write(t);
            }
        }

        /// <summary>
        /// Clears the header.
        /// </summary>
        public void clearHeaders()
        {
            this.context.Response.ClearHeaders();
        }

        /// <summary>
        /// Clears the content.
        /// </summary>
        public void clearContent()
        {
            this.context.Response.ClearContent();
        }

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void addHeader(string name, string value)
        {
            this.context.Response.AddHeader(name, value);
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void end()
        {
            this.context.Response.End();
        }

        /// <summary>
        /// Redirects the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void redirect(string url)
        {
            this.context.Response.Redirect(url);
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void flush()
        {
            this.context.Response.Flush();
        }
    }
}
