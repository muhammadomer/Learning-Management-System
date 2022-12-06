using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.Courses.Certificate;
using HUC.Web.App.Users;

namespace HUC.Web.App.PageModels
{
    public class UserDashboardPageModel:DashboardPageModel
    {
        public UserModel User { get; set; }
        public List<Certificate> Certificates { get; set; }
    }
}