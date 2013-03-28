namespace Phun
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Data;

    /// <summary>
    /// Simple CMS ContentController.
    /// </summary>
    [CmsAdminAuthorize]
    public class CmsContentController : CmsController
    {
    }
}
