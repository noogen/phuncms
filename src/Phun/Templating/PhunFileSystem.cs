namespace Phun.Templating
{
    using System.Web.Mvc;

    using Phun.Data;
    using Phun.Extensions;

    /// <summary>
    /// Default implementation for template file system.
    /// </summary>
    public class PhunFileSystem : IFileSystem
    {
        /// <summary>
        /// The controller
        /// </summary>
        private readonly PhunCmsController controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunFileSystem"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public PhunFileSystem(PhunCmsController controller)
        {
            this.controller = controller;
        }

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
            var content = new ContentModel()
            {
                Path = filename,
                Host = this.controller.GetCurrentHost(this.controller.ContentConfig, this.controller.Request.Url)
            };

            var result = string.Empty;
            controller.ContentRepository.Retrieve(content, true);
            if (content.DataLength != null)
            {
                content.SetDataFromStream();
                var dataString = System.Text.Encoding.UTF8.GetString(content.Data);

                result = dataString;
            }

            return result;
        }
    }
}
