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
        public void Render(Data.ContentModel model, HttpContextBase httpContext)
        {
            if (model.DataLength <= 0)
            {
                return;
            }

            using (var ctx = new JavascriptContext())
            {
                var util = new ResourcePathUtility();
                var file = new ResourceVirtualFile(util.GetResourcePath("/scripts/vash.js"));
                var context = new PhunApi(httpContext);
                context.FileModel = model;
                
                // set application start
                // set api object
                // set require method
                ctx.SetParameter("httpcontext", context); 
                ctx.SetParameter("filepath", model.Path);
                ctx.Run(@"
phun = { api: httpcontext };
module = { exports: {}, require: phun.api.require }; 
require = phun.api.require;
");
                
                using (var stream = file.Open())
                {                  
                    var data = System.Text.Encoding.UTF8.GetString(stream.ReadAll());
                    ctx.Run(data);
                }

                // register all the api objects
                foreach (var api in Bootstrapper.ApiList)
                {
                    ctx.SetParameter(api.Key.ToLowerInvariant(), Activator.CreateInstance(api.Value));
                }

                // finally execute the script
                ctx.Run(@"vash = module.exports; 
vash.renderFile(
    filepath, 
    { model : {} }, 
    function(err, html) {  
        phun.api.response.write(html.split('')); 
    }
);");
                httpContext.Response.Flush();
                httpContext.Response.End();
            }
        }
    }
}
