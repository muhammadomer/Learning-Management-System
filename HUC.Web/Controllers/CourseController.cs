using System.Linq;
using System.Web.Mvc;
using HUC.Web.App.Courses;
using HUC.Web.App.Shared;

namespace HUC.Web.Controllers
{
    public class CourseController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            var model = Database.GetAll<CourseModel>("WHERE IsVisibleWebsite = 1 AND IsDeleted = 0");

            return View(model);
        }

        public ActionResult Single(string slug)
        {
            var item =
                Database.GetAll<CourseModel>("WHERE IsVisibleWebsite = 1 AND IsDeleted = 0")
                    .SingleOrDefault(x => x.Name.ForUrl() == slug);

            if (item == null)
            {
                AddError("Invalid Course");
                return RedirectToAction("Index");
            }

            return View(item);
        }
    }
}
