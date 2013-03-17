namespace Phun.Data
{
    /// <summary>
    /// Repository use by SQL Server to store data.
    /// </summary>
    public interface ISqlDataRepository
    {
        /// <summary>
        /// Populates the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>
        /// Content model with populated datastream.
        /// </returns>
        ContentModel PopulateData(DapperContext context, ContentModel content, string cachePath);

        /// <summary>
        /// Caches the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        void CacheData(DapperContext context, ContentModel content, string cachePath);

        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="cachePath">The cache path.</param>
        void SaveData(DapperContext context, ContentModel content, string cachePath);
    }
}
