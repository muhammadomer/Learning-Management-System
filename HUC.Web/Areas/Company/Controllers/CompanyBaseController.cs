using System.Web.Mvc;
using HUC.Web.App.Shared;
using HUC.Web.Controllers;

namespace HUC.Web.Areas.Company.Controllers
{
    [Authorize]
    [AuthorizeCompany(true)]
    public class CompanyBaseController : BaseController
    {

	}
}