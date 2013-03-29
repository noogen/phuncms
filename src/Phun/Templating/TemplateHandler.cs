namespace Phun.Templating
{
    using System;
    using System.Text;
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
        private static string vashjsString;

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

                // caching vashjs
                if (vashjsString == null)
                {
                    var file = new ResourceVirtualFile(util.GetResourcePath("/scripts/vash.js"));

                    // load vash.js
                    using (var stream = file.Open())
                    {
                        vashjsString = System.Text.Encoding.UTF8.GetString(stream.ReadAll());
                    }
                }

                var context = new PhunApi(httpContext);
                context.FileModel = model;
                
                // set application start
                // set api object
                // set require method
                ctx.SetParameter("__httpcontext__", context); 
                ctx.Run(@"phun = { api: __httpcontext__ }; module = { exports: {}, require: phun.api.require }; 
require = phun.api.require;
console = { 
    log: function() {
// do not log anything for now
/*
        for(var i = 0; i < arguments.length; i++) {
            phun.api.trace.log('' + arguments[i]);
        }
*/
    }
};
");
                ctx.Run(vashjsString + Environment.NewLine + 
@"vash = module.exports;
var vashHtmlReportError = vash.helpers.constructor.reportError;
vashHtmlExceptionMessage = '';
vash.helpers.constructor.reportError = function(e, lineno, chr, orig, lb) {
    try {
        vashHtmlReportError(e, lineno, chr, orig, lb);
    }
    catch(ee) {
       vashHtmlExceptionMessage = e.stack;
    }
};
");
                // load api scripts
                foreach (var script in Bootstrapper.ApiScripts.Values)
                {
                    ctx.Run(script);
                }

                // finally execute the script
                ctx.Run(
@"try {
    vash.renderFile(
        phun.api.FileModel.Path, 
        { model : {} }, 
        function(err, html) {  
            phun.api.response.write(html.split('')); 
        }
    );
    
    // let above handle the exception.
} catch(ee) {
    throw new Error(ee + '\r\n' + vashHtmlExceptionMessage);
}
");

                httpContext.Response.Flush();
                httpContext.Response.End();
            }
        }
    }
}
