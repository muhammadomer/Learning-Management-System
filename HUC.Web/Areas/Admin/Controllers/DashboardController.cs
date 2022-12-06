using System.Web.Mvc;
using HUC.Web.Areas.Company.Controllers;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        //
        // GET: /Company/Dashboard/
        public ActionResult Index()
        {
            return View();
        }
	}
}