namespace Phun.Templating
{
    /// <summary>
    /// The file system.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Reads the file sync.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// Returns the contents of the filename.
        /// </returns>
        string readFileSync(string filename, string options);
    }
}
