using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phun
{
    /// <summary>
    /// Allow for plug-able singleton.
    /// </summary>
    /// <typeparam name="TImplementation">implementation class</typeparam>
    public abstract class Singleton<TImplementation> : MarshalByRefObject
        where TImplementation : class, new()
    {
        /// <summary>
        /// This make sure that implementations are Singleton.
        /// </summary>
        private static TImplementation defaultTImplementation = new TImplementation();

        /// <summary>
        /// Gets default implementation.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static TImplementation Default
        {
            get { return defaultTImplementation; }
        }
    }
}
