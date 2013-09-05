using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phun
{
    using Phun.Data;

    /// <summary>
    /// Content Connector Event Argument
    /// </summary>
    public class ContentConnectorEventArgument : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentConnectorEventArgument" /> class.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="contentModel">The content model.</param>
        public ContentConnectorEventArgument(string eventName, ContentModel contentModel)
        {
            this.EventName = eventName;
            this.Connector = contentModel;
        }

        /// <summary>
        /// Gets or sets the connector.
        /// </summary>
        /// <value>
        /// The connector.
        /// </value>
        public ContentModel Connector { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>
        /// The extra.
        /// </value>
        public object Extra { get; set; }
    }
}
