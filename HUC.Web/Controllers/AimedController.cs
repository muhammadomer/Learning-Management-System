using System.Web.Mvc;
using HUC.Web.App.Courses;

namespace HUC.Web.Controllers
{
    public class AimedController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            var model = Database.GetAll<CourseModel>("WHERE IsVisibleWebsite = 1 AND IsDeleted = 0");

            return View(model);
        }
    }
}
