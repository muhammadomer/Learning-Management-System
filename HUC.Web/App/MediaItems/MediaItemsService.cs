using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using AtlasDB;
using HUC.Web.App.MediaItems.Models;
using HUC.Web.App.Shared;

namespace HUC.Web.App.MediaItems
{
    public class MediaItemsService
    {

    }

    public static class MediaItemsHelper
    {
        public static MvcHtmlString MediaSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool isMultiple = false, bool isRemovable = false)
        {
            return MediaSelectFor(htmlHelper, expression, FileType.Any, isMultiple, isRemovable);
        }

        public static MvcHtmlString MediaSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, FileType mediaType, bool isMultiple = false, bool isRemovable = false)
        {
            return MediaSelectFor(htmlHelper, expression, new[] { mediaType }, isMultiple, isRemovable);
        }

        public static MvcHtmlString MediaSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<FileType> mediaTypes, bool isMultiple = false, bool isRemovable = false)
        {
            //Create IDS and customValue if needed
            string customCommaValue = null;
            var mediaIDs = new List<int>();
            var member = expression.Body as MemberExpression;
            if (member == null) { throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", expression)); }
            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null) { throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", expression.ToString())); }
            var func = expression.Compile();
            var model = htmlHelper.ViewData.Model;
            if (model != null)
            {
                if (propInfo.GetCustomAttributes().OfType<CommaSeparatedAttribute>().Any())
                {
                    var value = func(model) as IEnumerable<int>;
                    if (value != null)
                    {
                        customCommaValue = String.Join(",", value);
                        mediaIDs = value.ToList();
                    }
                }
                else
                {
                    var value = func(model) as int?;
                    if (value.HasValue)
                    {
                        mediaIDs.Add(value.Value);
                    }
                }
            }
            //--

            var sb = new StringBuilder();
            //Wrapper
            sb.AppendLine("<div>");

            //Nice Display Wrapper
            sb.AppendLine("<div class=\"niceDisplayWrapper\">");
            if (mediaIDs.Any())
            {
                var db = new AtlasDatabase();

                foreach (var curMediaID in mediaIDs)
                {
                    var mediaItem = db.GetSingle<MediaItemModel>(curMediaID);
                    if (mediaItem != null)
                    {
                        sb.AppendLine("<div class=\"niceDisplay\" style=\"float:left;\">" + mediaItem.SimpleOutput(50, 50) + "</div>");
                    }
                }

            }
            sb.AppendLine("</div>");


            //Render hidden value.
            sb.AppendLine(customCommaValue == null
                ? htmlHelper.HiddenFor(expression).ToString()
                : htmlHelper.Hidden(propInfo.Name, customCommaValue).ToString());

            //Render Controls.
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            sb.AppendLine("<a class=\"mediaPicker\" href=\"#\" rel=\"" + urlHelper.Action("MediaPicker", "MediaItems", new { area = "Admin" }) + "\" " +
                          "data-targetelement=\"#" + htmlHelper.IdFor(expression) + "\"" +
                          "data-currentselections=\"" + (String.IsNullOrWhiteSpace(customCommaValue) ? htmlHelper.ValueFor(expression).ToString() : customCommaValue) + "\"" +
                          "data-filetypes=\"" + String.Join(",", mediaTypes.Select(x => (int)x)) + "\"" +
                          "data-ismultiple=\"" + isMultiple + "\"" +
                          "data-isremovable=\"" + isRemovable + "\"" +
                          ">Click here</a> to select media item" + (isMultiple ? "s" : ""));
            if (isRemovable)
            {
                sb.AppendLine("<a class=\"mediaPickerClear\" href=\"#\">Clear Selection</a>");
            }

            //End wrapper
            sb.AppendLine("</div>");

            return new MvcHtmlString(sb.ToString());
        }
    }
}