namespace Phun.Templating
{
    /// <summary>
    /// Write trace.
    /// </summary>
    public interface ITrace
    {
        /// <summary>
        /// Logs the specified info.
        /// </summary>
        /// <param name="args">The args.</param>
        void log(string[] args);

        /// <summary>
        /// Logs the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        void log(object[] args);

        /// <summary>
        /// Logs the specified args.
        /// </summary>
        /// <param name="arg">The arg.</param>
        void log(string arg);

        /// <summary>
        /// Logs the specified arg1.
        /// </summary>
        /// <param name="arg1">The arg1.</param>
        /// <param name="arg2">The arg2.</param>
        /// <param name="arg3">The arg3.</param>
        /// <param name="arg4">The arg4.</param>
        void log(object arg1, object arg2, object arg3, object arg4);
    }
}
