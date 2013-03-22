namespace Phun.Templating
{
    /// <summary>
    /// The file interface.
    /// </summary>
    public interface IPath
    {
        /// <summary>
        /// Normalize a string path, taking care of '..' and '.' parts. 
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Normalized path string.</returns>
        string normalize(string path);

        /// <summary>
        /// Join all arguments together and normalize the resulting path. 
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// Join string
        /// </returns>
        string join(string left, string right);

        /// <summary>
        /// Return the extension of the path, from the last '.' to end of string in the last portion of the path. 
        /// If there is no '.' in the last portion of the path or the first character of it is '.', then it returns an empty string. 
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Name</returns>
        string extname(string path);
    }
}
