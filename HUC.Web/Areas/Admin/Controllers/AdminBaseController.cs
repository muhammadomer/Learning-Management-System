using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.Controllers;

namespace HUC.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBaseController : BaseController
    {

	}
}