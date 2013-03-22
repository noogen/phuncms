namespace Phun.Templating
{
    using System.Web.Mvc;

    /// <summary>
    /// Default implementation for template file system.
    /// </summary>
    public class PhunFileSystem : IFileSystem
    {
        /// <summary>
        /// Reads the file sync.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the contents of the filename.
        /// </returns>
        public string readFileSync(string filename, string options)
        {
            return null;
        }
    }
}
