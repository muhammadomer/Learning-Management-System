using System.Web.Mvc;
using System.Web.Routing;

namespace HUC.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*botdetect}",
                                    new { botdetect = @"(.*)BotDetectCaptcha\.ashx" });

            routes.MapRoute(
                "News Item",
                "News/{slug}",
                new { controller = "News", action = "Single" },
                new[] { "HUC.Web.Controllers" }
            );

            routes.MapRoute(
                "Course Item",
                "Course/{slug}",
                new { controller = "Course", action = "Single" },
                new[] { "HUC.Web.Controllers" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Site", action = "Index", id = UrlParameter.Optional },
                new[] { "HUC.Web.Controllers" }
            );
        }
    }
}
