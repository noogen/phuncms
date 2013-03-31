namespace Phun.Data
{
    using System;
    using System.IO;
    using System.Web;

    using Phun.Extensions;

    /// <summary>
    /// Content model for data store.
    /// </summary>
    public class ContentModel : IDisposable
    {
        /// <summary>
        /// The path
        /// </summary>
        private string path = string.Empty;

        /// <summary>
        /// The stream
        /// </summary>
        private Stream stream;

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path 
        { 
            get
            {
                return this.path;
            }

            set
            {
                // normalize the path
                var myValue = (value + string.Empty).Replace("\\", "/").Replace("//", "/").TrimStart('/');
                
                this.path = string.Concat('/', myValue);
            }
        }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        /// <value>
        /// The host name.
        /// </value>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the create date.
        /// </summary>
        /// <value>
        /// The create date.
        /// </value>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the create by.
        /// </summary>
        /// <value>
        /// The create by.
        /// </value>
        public string CreateBy { get; set; }

        /// <summary>
        /// Gets or sets the modify date.
        /// </summary>
        /// <value>
        /// The modify date.
        /// </value>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// Gets or sets the modify by.
        /// </summary>
        /// <value>
        /// The modify by.
        /// </value>
        public string ModifyBy { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        /// <returns>Folder name derived by path.</returns>
        public virtual string ParentPath
        {
            get
            {
                var result = "/";

                if (this.Path.TrimEnd('/').LastIndexOf('/') > 0)
                {
                    result = VirtualPathUtility.GetDirectory(this.Path); 
                }
                
                return result;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <returns>File name derived by path.</returns>
        public string FileName
        {
            get
            {
                return VirtualPathUtility.GetFileName(this.Path);
            }
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public string FileExtension
        {
            get
            {
                return VirtualPathUtility.GetExtension(this.Path);
            }
        }

        /// <summary>
        /// Gets or sets the length of the data.
        /// </summary>
        /// <value>
        /// The length of the data.
        /// </value>
        public long? DataLength { get; set; }

        /// <summary>
        /// Gets or sets the data id.
        /// </summary>
        /// <value>
        /// The data id.
        /// </value>
        public System.Guid? DataId { get; set; }

        /// <summary>
        /// Gets or sets the data id string.
        /// </summary>
        /// <value>
        /// The data id string.
        /// </value>
        public string DataIdString
        {
            get
            {
                return this.DataId.HasValue ? this.DataId.ToString() : null;
            }

            set
            {
                this.DataId = string.IsNullOrWhiteSpace(value) ? (Guid?)null : Guid.Parse(value);
            }
        }

        /// <summary>
        /// Gets or sets the data stream.  This provide support
        /// for new SQL server FileStream or FileTable.
        /// This property is related to Data.  When Data is null,
        /// the controller would fallback on this property.  Therefore,
        /// DataStream is use for retrieving purpose only.
        /// </summary>
        /// <value>
        /// The data stream.
        /// </value>
        public Stream DataStream
        {
            get
            {
                return stream;
            }

            set
            {
                stream = value;

                if (stream != null)
                {
                    DataLength = stream.Length;
                    if (stream.Length < 10000000)
                    {
                        this.SetDataFromStream();
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var stream = this.DataStream;
            this.DataStream = null;

            if (stream != null && stream.CanRead)
            {
                stream.Dispose();
            }
        }

        /// <summary>
        /// Sets the data from stream.
        /// </summary>
        /// <returns>The content model.</returns>
        public ContentModel SetDataFromStream()
        {
            if (this.DataStream != null)
            {
                var dataStream = this.DataStream;
                this.Data = dataStream.ReadAll();
                this.DataStream = null;
                dataStream.Dispose();
            }

            return this;
        }
    }
}
