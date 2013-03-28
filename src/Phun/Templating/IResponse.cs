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
    }
}
