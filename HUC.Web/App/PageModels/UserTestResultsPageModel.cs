using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;

namespace HUC.Web.App.PageModels
{
    public class UserTestResultsPageModel
    {
        public UserModel User { get; set; }
        public UserCourseModel UserCourse { get; set; }
    }
}