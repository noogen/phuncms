namespace Phun
{
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Routing;

    /// <summary>
    /// Resource route handler.  This allow for mapping phumcms static content when runAllManagedModulesForAllRequests is false.
    /// </summary>
    /// <code>
    /// <add name="phuncms" path="phuncms/*" verb="*" type="Phun.ResourceRouteHandler" preCondition="integratedMode,runtimeVersionv4.0"/>
    /// </code>
    public class ResourceRouteHandler : IHttpHandler
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            VirtualFile vf = null;
            VirtualPathProvider vpp = new ResourcePathProvider();
            string vpath = request.Path;

            vf = vpp.GetFile(vpath);
            var stream = vf.Open();

            if (stream == null)
            {
                throw new HttpException(404, "Path '" + vpath + "' cannot be found.");
            }

            response.ContentType = MimeTypes.GetContentType(System.IO.Path.GetExtension(vpath));
            stream.CopyTo(response.OutputStream);
            response.OutputStream.Flush();
            response.End();
        }
    }
}
