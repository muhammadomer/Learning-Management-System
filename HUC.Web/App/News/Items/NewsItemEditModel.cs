using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AtlasDB;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Labels;
using HUC.Web.App.Shared;

namespace HUC.Web.App.News.Items
{
    public class NewsItemEditModel : NewsItemModel
    {
        [Required]
        [DBIgnore]
        [UIHint("DateString")]
        [Display(Name = "Publish On Date")]
        public string PublishOnDateString { get; set; }
        [Required]
        [DBIgnore]
        [UIHint("TimeString")]
        [Display(Name = "Publish On Time")]
        public string PublishOnTimeString { get; set; }
        public DateTime PublishOn { get; set; }
    }
}