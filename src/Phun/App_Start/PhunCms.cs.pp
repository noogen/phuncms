[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.PhunCms), "Start")]
namespace $rootnamespace$.App_Start
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
            Phun.Bootstrapper.Default.Initialize();
            
            // opportunity to register template api
            // use inside of vash template @{ var customer = yourapi.getCustomerById(customerid) }
            // Phun.Bootstrapper.Default.RegisterTemplateApi<YourCustomerAPI>("yourapi");
        }
    }
}
