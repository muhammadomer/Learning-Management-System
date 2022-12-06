using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.Models
{
    public class PageDataModel
    {
        public PageDataModel()
        {
            Title = null;
            ContentTitleDesc = null;
            ContentTitle = null;
            ActivePage = null;
            ActiveSubPage = null;
            Breadcrumb = new List<BreadcrumbItem>();
            StrippedLayout = false;
        }

        public string Title { get; set; }

        public string ContentTitleDesc { get; set; }
        public string ContentTitle { get; set; }

        public string ActivePage { get; set; }
        public string ActiveSubPage { get; set; }

        public List<BreadcrumbItem> Breadcrumb { get; set; }

        public bool StrippedLayout { get; set; }
    }
}