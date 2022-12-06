using System.Web.Mvc;

namespace HUC.Web.Areas.Scale
{
    public class ScaleAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Scale";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Scale_default",
                "Scale/{controller}/{action}/{id}",
                new { action = "Index", controller = "Dashboard", id = UrlParameter.Optional }
            );
        }
    }
}