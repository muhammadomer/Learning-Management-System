using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HUC.Web.App.Users;

namespace HUC.Web.App.Shared
{
    public class AuthorizeCompanyAttribute : ActionFilterAttribute
    {
        public bool EnsureAdmin { get; set; }


        public AuthorizeCompanyAttribute(bool ensureAdmin = false)
        {
            EnsureAdmin = ensureAdmin;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isAllowed = false;

            var _users = new UsersService();

            var userModel = _users.GetLoggedInUserModel();

            if (userModel != null)
            {
                var company = userModel.Company;
                isAllowed = true;
                //if (company != null)
                //{
                //    //var companyUser = company.AllUsers.Where(x => x.UserID == userModel.ID ).ToList();
                //    var companyUser = company.AllUsers.SingleOrDefault(x => x.UserID == userModel.ID );
                //    if (companyUser != null)
                //    {
                //        if (EnsureAdmin)
                //        {
                //            //We want ONLY admin accounts to access this area
                //            if (companyUser.IsAdmin || companyUser.IsBackupAdmin)
                //            {
                //                isAllowed = true;
                //            }
                //        }
                //        else
                //        {
                //            //We want ONLY user accounts to access this area
                //            if (companyUser.IsBackupAdmin || companyUser.IsAdmin)
                //            {
                //                if (companyUser.IsBackupAdminCourseUsable)
                //                {
                //                    isAllowed = true;
                //                }
                //            }
                //            else
                //            {
                //                isAllowed = true;
                //            }
                //        }
                //    }
                //}
            }

            if (!isAllowed)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                area = "",
                                controller = "Auth",
                                action = "Login",
                                ReturnUrl = filterContext.HttpContext.Request.Path
                            }));
            }
        }
    }



}