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
        /// <param name="args">The args.</param>
        public void log(params string[] args)
        {
            foreach (var info in args)
            {
                this.log(info);
            }
        }

        /// <summary>
        /// Logs the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        public void log(object[] args)
        {
            foreach (var info in args)
            {
                this.log(info + string.Empty);
            }
        }

        /// <summary>
        /// Logs the specified args.
        /// </summary>
        /// <param name="arg">The arg.</param>
        public void log(string arg)
        {
            System.Diagnostics.Trace.WriteLine("PhunCMS: " + arg);
        }

        /// <summary>
        /// Logs the specified arg1.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        public void log(object arg1, object arg2, object arg3, object arg4)
        {
            this.log(new object[] { arg1, arg2, arg3, arg4 });
        }
    }
}
