namespace Phun.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;

    /// <summary>
    /// Interface for storing content.
    /// </summary>
    public interface IContentRepository
    {
        /// <summary>
        /// Populate or gets the content provided specific host, path, and name property.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        /// <param name="includeData">if set to <c>true</c> [include data].</param>
        /// <returns>
        /// The <see cref="ContentModel" /> that was passed in.
        /// </returns>
        ContentModel Retrieve(ContentModel content, bool includeData = true);

        /// <summary>
        /// Check for exist of content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        /// <returns>
        /// true if content exists.
        /// </returns>
        bool Exists(ContentModel content);

        /// <summary>
        /// Saves the specified content.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.</param>
        void Save(ContentModel content);

        /// <summary>
        /// Removes the specified content path or route.
        /// </summary>
        /// <param name="content">The content - requires host, path, and name property.  If name is set to "*" then removes all for host and path.</param>
        void Remove(ContentModel content);

        /// <summary>
        /// Lists the specified content.Path
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Enumerable to content model.</returns>
        IQueryable<ContentModel> List(ContentModel content);

        /// <summary>
        /// Gets the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>Path to temp file that is a zip of the folder content.</returns>
        string GetFolder(ContentModel folder);

        /// <summary>
        /// Histories the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Specific content change history.</returns>
        IQueryable<ContentModel> RetrieveHistory(ContentModel content);

        /// <summary>
        /// Popuplates the history data.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="historyDataId">The history data id.</param>
        void PopulateHistoryData(ContentModel content, System.Guid historyDataId);
    }
}
