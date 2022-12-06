using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HUC.Web.Models
{
    public class BreadcrumbItem
    {
        public BreadcrumbItem(string name)
        {
            Name = name;
            Link = null;

            Icon = null;
        }
        public BreadcrumbItem(string name, string link)
        {
            Name = name;
            Link = link;

            Icon = null;
        }
        public BreadcrumbItem(string name, string link, string icon)
        {
            Name = name;
            Link = link;

            Icon = icon;
        }

        public string Name { get; set; }
        public string Link { get; set; }
        public string Icon { get; set; }
    }
}