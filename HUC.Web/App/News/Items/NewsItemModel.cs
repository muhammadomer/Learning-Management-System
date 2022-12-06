using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.MediaItems.Models;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Labels;
using HUC.Web.App.Shared;
using HUC.Web.App.Testimonials;

namespace HUC.Web.App.News.Items
{
    public class NewsItemModel : BaseModel
    {
        [Required, StringLength(500), Display(Name = "Title")]
        public string Title { get; set; }

        [Required, StringLength(1000), Display(Name = "Intro Copy")]
        public string IntroCopy { get; set; }

        [AllowHtml, Required, Display(Name = "Body Copy")]
        public string BodyCopy { get; set; }

        [Required, Display(Name = "Publish On")]
        public DateTime PublishOn { get; set; }

        [Required, Display(Name = "Published By")]
        public string PublishBy { get; set; }

        [Display(Name = "Image")]
        public int? ImageID { get; set; }

        [Display(Name = "Category")]
        public int? CategoryID { get; set; }

        [Display(Name = "Testimonial")]
        public int? TestimonialID { get; set; }

        [DBIgnore, Display(Name = "Labels")]
        public IEnumerable<int> LabelIDs { get; set; }

        public List<SelectListItem> CategoryOptions()
        {
            var sli = new List<SelectListItem>();
            sli.Add(new SelectListItem
            {
                Text = "No Category",
                Value = ""
            });

            var cats = Database.GetAll<NewsCategoryModel>("WHERE IsDeleted = 0");
            foreach (var curCat in cats)
            {
                sli.Add(new SelectListItem
                {
                    Text = curCat.Name,
                    Value = curCat.ID.ToString()
                });
            }

            return sli;
        }

        public List<SelectListItem> LabelOptions()
        {
            var sli = new List<SelectListItem>();

            var labels = Database.GetAll<NewsLabelModel>("WHERE IsDeleted = 0");
            foreach (var curLabel in labels)
            {
                sli.Add(new SelectListItem
                {
                    Text = curLabel.Name,
                    Value = curLabel.ID.ToString()
                });
            }

            return sli;
        }

        public List<SelectListItem> TestimonialOptions()
        {
            var sli = new List<SelectListItem>();
            sli.Add(new SelectListItem
            {
                Text = "", Value = ""
            });

            var labels = Database.GetAll<TestimonialModel>("WHERE IsDeleted = 0");
            foreach (var curTestimonial in labels)
            {
                sli.Add(new SelectListItem
                {
                    Text = curTestimonial.FullName + (String.IsNullOrWhiteSpace(curTestimonial.CompanyName) ? "" : " @ " + curTestimonial.CompanyName),
                    Value = curTestimonial.ID.ToString()
                });
            }

            return sli;
        }

        //Lazy
        private NewsCategoryModel _category;
        private IEnumerable<NewsLabelModel> _labels;
        private MediaItemModel _image;
        private string _slug;
        private TestimonialModel _testimonial;

        [DBIgnore]
        public NewsCategoryModel Category
        {
            get
            {
                if (_category == null)
                {
                    _category = Database.GetSingle<NewsCategoryModel>(this.CategoryID);
                }
                return _category;
            }
            set { _category = value; }
        }

        [DBIgnore]
        public IEnumerable<NewsLabelModel> Labels
        {
            get
            {
                if (_labels == null)
                {
                    _labels =
                        Database.GetAll<NewsLabelModel>(
                            "WHERE ID IN (SELECT NewsLabelID FROM NewsItemLabels WHERE NewsItemID = @ID)",
                            new { ID = this.ID });
                }
                return _labels;
            }
            set { _labels = value; }
        }

        [DBIgnore]
        public MediaItemModel Image
        {
            get
            {
                if (_image == null)
                {
                    _image = Database.GetSingle<MediaItemModel>(this.ImageID);
                }
                return _image;
            }
            set { _image = value; }
        }

        [DBIgnore]
        public TestimonialModel Testimonial
        {
            get
            {
                if (_testimonial == null)
                {
                    _testimonial = Database.GetSingle<TestimonialModel>(this.TestimonialID);
                }
                return _testimonial;
            }
            set { _testimonial = value; }
        }
    }
}
