using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phun.Templating
{
    /// <summary>
    /// Tracing object.
    /// </summary>
    public class Trace : ITrace
    {
        /// <summary>
        /// Logs the specified info.
        /// </summary>
        /// <param name="info">The info.</param>
        public void log(string info)
        {
            System.Diagnostics.Trace.WriteLine("PhunCMS js trace: " + info);
        }
    }
}
