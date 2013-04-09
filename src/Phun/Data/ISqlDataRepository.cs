namespace Phun.Data
{
    using System.Linq;

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
        /// <param name="tableName">Name of the table.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <returns>
        /// Content model with populated data stream.
        /// </returns>
        ContentModel PopulateData(DapperContext context, ContentModel content, string tableName, string cachePath);

        /// <summary>
        /// Caches the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="cachePath">The cache path.</param>
        void CacheData(DapperContext context, ContentModel content, string tableName, string cachePath);

        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="cachePath">The cache path.</param>
        /// <param name="keepHistory">if set to <c>true</c> [keep history].</param>
        void SaveData(DapperContext context, ContentModel content, string tableName, string cachePath, bool keepHistory);

        /// <summary>
        /// Retrieves the history.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The content history.</returns>
        IQueryable<ContentModel> RetrieveHistory(DapperContext context, ContentModel content, string tableName);

        /// <summary>
        /// Populates the history data.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        /// <param name="tableName">Name of the table.</param>
        void PopulateHistoryData(DapperContext context, ContentModel content, string tableName);
    }
}
