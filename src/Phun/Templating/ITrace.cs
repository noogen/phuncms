namespace Phun.Templating
{
    /// <summary>
    /// Write trace.
    /// </summary>
    public interface ITrace
    {
        /// <summary>
        /// Logs the specified info.
        /// </summary>
        /// <param name="info">The info.</param>
        void log(string info);
    }
}
