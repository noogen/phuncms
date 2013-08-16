[assembly: WebActivator.PostApplicationStartMethod(typeof(Phun.Demo.Web.App_Start.PhunCms), "Start")]
namespace Phun.Demo.Web.App_Start
{
    /// <summary>
    /// PhunCMS BootStrapper
    /// </summary>
    public static class PhunCms
    {
        /// <summary>
        /// PhunCMS Initialized method.
        /// </summary>
        public static void Start()
        {
            Phun.Bootstrapper.Initialize();
            
            // opportunity to register template api
            // use inside of vash template @{ var customer = yourapi.getCustomerById(customerid) }
            // Phun.Bootstrapper.RegisterTemplateApi<YourCustomerAPI>("yourapi");
        }
    }
}
