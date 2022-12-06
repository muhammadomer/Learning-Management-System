using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Items;
using HUC.Web.App.Testimonials;

namespace HUC.Web.App.PageModels
{
    public class NewsItemPageModel
    {
        public NewsItemModel NewsItem { get; set; }
        public IEnumerable<DateTime> Archive { get; set; }
        public IEnumerable<NewsCategoryModel> Categories { get; set; }
    }
}