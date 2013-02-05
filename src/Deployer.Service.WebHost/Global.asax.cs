using System.Web.Mvc;
using System.Web.Routing;

namespace Deployer.Service.WebHost
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute("Deploys", "", new {controller = "Home", action = "Index"});
            routes.MapRoute("Logs", "l/{file}", new {controller = "Home", action = "Log", file = UrlParameter.Optional});
        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}