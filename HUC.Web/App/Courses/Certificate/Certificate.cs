using HUC.Web.App.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.App.Courses.Certificate
{
    public class Certificate:BaseModel
    {
        public DateTime IssueDate { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsIssue { get; set; }
        public string CreatedBy { get; set; }
        public string CompanyName { get; set; }
        public bool IsPass { get; set; }

    }
}