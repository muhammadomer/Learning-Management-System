using System.Web.Mvc;
using HUC.Web.App.Testimonials;

namespace HUC.Web.Controllers
{
    public class AboutController : BaseController
    {
        //
        // GET: /Site/

        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Services()
        //{
        //    return View();
        //}

        public ActionResult WhyUs()
        {
            return View();
        }

        public ActionResult Testimonials()
        {
            var model = Database.GetAll<TestimonialModel>("WHERE IsDeleted = 0");

            return View(model);
        }
    }
}
