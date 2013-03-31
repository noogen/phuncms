namespace Phun.Templating
{
    using System.Web;

    /// <summary>
    /// Phun implementation for path.
    /// </summary>
    public class PhunPath : IPath
    {
        /// <summary>
        /// Normalize a string path, taking care of '..' and '.' parts.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Normalized path string.
        /// </returns>
        public string normalize(string path)
        {
            return VirtualPathUtility.ToAppRelative(path).Replace("~", string.Empty);
        }

        /// <summary>
        /// Join all arguments together and normalize the resulting path.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// Join string
        /// </returns>
        public string join(string left, string right)
        {
            return VirtualPathUtility.Combine(this.normalize(left), right);
        }

        /// <summary>
        /// Return the extension of the path, from the last '.' to end of string in the last portion of the path.
        /// If there is no '.' in the last portion of the path or the first character of it is '.', then it returns an empty string.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Name
        /// </returns>
        public string extname(string path)
        {
            return VirtualPathUtility.GetExtension(path);
        }
    }
}
