[assembly: WebActivator.PostApplicationStartMethod(typeof($rootnamespace$.App_Start.PhunCms), "Start")]
namespace $rootnamespace$.App_Start
{
	using Phun;

    public static class PhunCms
    {
        public static void Start()
        {
            PhunCmsBootStrapper.Initialize();
        }
    }
}
