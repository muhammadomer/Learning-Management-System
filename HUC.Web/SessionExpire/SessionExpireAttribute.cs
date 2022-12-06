using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HUC.Web.SessionExpire
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["CourseAllowAdmin"] == null)
            {
                filterContext.Result = new RedirectResult("~/Auth/Logout");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}