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
        /// <summary>
        /// The vashjs string
        /// </summary>
        private static string vashjsString;

        /// <summary>
        /// The utility
        /// </summary>
        protected internal ResourcePathUtility Utility;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateHandler"/> class.
        /// </summary>
        public TemplateHandler()
        {
            this.Utility = new ResourcePathUtility();
        }

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
        /// <param name="connector">The connector.</param>
        /// <param name="httpContext">The HTTP context.</param>
        public void Render(Data.ContentModel model, IContentConnector connector, HttpContextBase httpContext)
        {
            using (var ctx = new JavascriptContext())
            {
                // caching vashjs
                if (vashjsString == null)
                {
                    var file = this.Utility.Config.GetResourceFile(this.Utility.GetResourcePath("/scripts/vash.js"));

                    // load vash.js
                    using (var stream = file.Open())
                    {
                        vashjsString = System.Text.Encoding.UTF8.GetString(stream.ReadAll());
                    }
                }

                var context = new PhunApi(httpContext, connector);
                context.FileModel = model;
                
                // set application start
                // set api object
                // set require method
                ctx.SetParameter("__httpcontext__", context); 
                ctx.Run(@"phun = { api: __httpcontext__ }; module = { exports: {}, require: phun.api.require }; 
require = phun.api.require;
console = { 
    log: function() {
        // we need this for vash and everybody else
        for(var i = 0; i < arguments.length; i++) {
            phun.api.trace.log('' + arguments[i]);
        }
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

                // finally execute the script, render directly to chars array
                ctx.Run(
@"try {
    vash.renderFile(
        phun.api.FileModel.Path, 
        { model : {} }, 
        function(err, html) {  
            phun.api.response.write(html.split(''));
            phun.api.response.flush();
        }
    );
} catch(ee) {
    // provide better exception message
    throw new Error(ee + '\r\n' + vashHtmlExceptionMessage);
}
");
                httpContext.Response.End();
            }
        }
    }
}
