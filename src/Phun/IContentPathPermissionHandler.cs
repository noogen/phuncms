namespace Phun
{
    using Phun.Data;

    /// <summary>
    /// This interface allow user to have a special content update permission by path.
    /// Ultimately, you can setup your own configuration to allow specific user or role to 
    /// update a particular path.  You have to implement your own configuration and permission.
    /// </summary>
    /// <remarks>
    /// Benefit: You just want to grant this one user or group to update a single path.
    /// You will need to define the implementation for this handler and use ioc to inject
    /// this into the provided PhunCmsContentController.
    /// </remarks>
    public interface IContentPathPermissionHandler
    {
        /// <summary>
        /// Determines whether the specified path is admin.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <c>true</c> if the specified path is admin; otherwise, <c>false</c>.
        /// </returns>
        bool IsAdmin(PhunCmsContentController controller, ContentModel model);
    }
}
