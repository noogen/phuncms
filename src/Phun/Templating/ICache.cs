namespace Phun.Templating
{
    /// <summary>
    /// Cache object.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        object get(string key);

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void set(string key, object value);
    }
}
