namespace Phun.Templating
{
    using System;
    using System.Web;

    using Newtonsoft.Json;

    using Noesis.Javascript;

    using Phun.Extensions;
    using Phun.Routing;

    /// <summary>
    /// Template handler.
    /// </summary>
    public class TemplateHandler : ITemplateHandler
    {
        /// <summary>
        /// Determines whether this instance can render the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <c>true</c> if this instance can render the specified model; otherwise, <c>false</c>.
        /// </returns>
        public bool CanRender(Data.ContentModel model)
        {
            return model.Path.EndsWith(".vash", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Executes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public string Render(Data.ContentModel model, PhunCmsController controller)
        {
            if (model.DataLength <= 0)
            {
                return null;
            }

            using (var ctx = new JavascriptContext())
            {
                var util = new ResourcePathUtility();
                var file = new ResourceVirtualFile(util.GetResourcePath("/scripts/vash.js"));
                var context = new PhunHttpContext(controller);
                context.File = model.Path;
                
                // set application start
                // set api object
                // set require method
                ctx.SetParameter("phunapi", new PhunHttpContext(controller));
                using (var stream = file.Open())
                {
                    var data = "module = { exports: {}, require: phunapi.require}; require = module.require;" + System.Text.Encoding.UTF8.GetString(stream.ReadAll());
                    ctx.Run(data);
                }

                // finally execute the script
                return (string)ctx.Run("vash = module.exports; vash.renderFile(phunapi.File, {});");
            }
        }
    }
}
