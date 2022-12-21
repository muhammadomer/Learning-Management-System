using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.Controllers;

namespace HUC.Web.Areas.Users.Controllers
{
    [Authorize]
    [AuthorizeCompany]
    public class UsersBaseController : BaseController
    {
        private UsersService _users = new UsersService();

        public UserModel GetLoggedInUser()
        {
            return _users.GetLoggedInUserModel();
        }

        public UsersBaseController()
        {

          

            var user = new UsersService().GetLoggedInUserModel();

            
            var count = Database.Query<int>("select count(*) from CompanyUsers where CompanyID=" + user.Company.ID +
                         " and UserId = " + user.ID +
                          " and(isadmin = 1 or isbackupadmin = 1) " +
                          " group by UserId ").FirstOrDefault();

            LogApp.Log4Net.WriteLog("Company User Count : "+count, LogApp.LogType.GENERALLOG);
            if (count == 0)
            {
                ViewBag._IsSimpleUser = 1;
            }
            else
            {
                ViewBag._IsSimpleUser = 0;
            }
           
        }
        
    }
}