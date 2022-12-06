using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.Models
{
    public class CommonClass
    {
    }
    public class EmailSenders
    {
        public string name { get; set; }
        public List<string> courses { get; set; }
        public string emails { get;set; }
    }

    public class CountRecords
    {
        public int ID { get; set; }
        public int CourseId { get; set; }
        public int TotalCount { get; set; }

    }
}