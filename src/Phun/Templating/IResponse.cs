namespace Phun.Templating
{
    /// <summary>
    /// Response object.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Writes the specified bytes.
        /// </summary>
        /// <param name="chars">The chars.</param>
        void write(string[] chars);

        /// <summary>
        /// Clears the headers.
        /// </summary>
        void clearHeaders();

        /// <summary>
        /// Clears the content.
        /// </summary>
        void clearContent();

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void addHeader(string name, string value);

        /// <summary>
        /// Ends response.
        /// </summary>
        void end();

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        void flush();

        /// <summary>
        /// Redirects the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        void redirect(string url);
    }
}
