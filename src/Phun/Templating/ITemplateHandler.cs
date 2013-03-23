namespace Phun.Templating
{
    using Phun.Data;

    /// <summary>
    /// Template handler.
    /// </summary>
    public interface ITemplateHandler
    {
        /// <summary>
        /// Determines whether this instance can render the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified model; otherwise, <c>false</c>.
        /// </returns>
        bool CanRender(ContentModel model);

        /// <summary>
        /// Executes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="controller">The controller.</param>
        /// <returns>The rendered string.</returns>
        string Render(ContentModel model, PhunCmsController controller);
    }
}
