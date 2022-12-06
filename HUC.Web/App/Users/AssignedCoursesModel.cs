using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.App.Users
{
    public class AssignedCoursesModel
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public int CompanyID { get; set; }
        public int CourseID { get; set; }
      
    }
}