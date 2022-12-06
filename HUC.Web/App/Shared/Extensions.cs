using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using HUC.Web.App.Courses;
using HUC.Web.App.MediaItems;
using HUC.Web.App.News.Categories;
using HUC.Web.App.News.Items;
using HUC.Web.App.Users;

namespace HUC.Web.App.Shared
{
    public static class Extensions
    {
        public static MvcHtmlString ChosenFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> options, string placeholder, bool isMultiple = false, bool isDeselectable = false)
        {
            if (!isMultiple)
            {
                return new MvcHtmlString(htmlHelper.DropDownListFor(expression, options, new Dictionary<string, object> { { "style", "width:178px;" }, { "class", "chzn-select" + (isDeselectable ? "-removable" : "") }, { "data-placeholder", placeholder } }).ToString());
            }
            else
            {
                return new MvcHtmlString(htmlHelper.ListBoxFor(expression, options, new Dictionary<string, object> { { "style", "width:178px;" }, { "class", "chzn-select" + (isDeselectable ? "-removable" : "") }, { "multiple", "" }, { "data-placeholder", placeholder } }).ToString());
            }
        }

        public static UserModel User(this HtmlHelper html)
        {
            return new UsersService().GetLoggedInUserModel();
        }

        public static string GetImage(this HtmlHelper html, string fileName)
        {
            return GetImage(html, fileName, -1, -1, FileManipulation.DefaultPaddingColor);
        }

        public static string GetImage(this HtmlHelper html, string fileName, int width, int height)
        {
            return GetImage(html, fileName, width, height, FileManipulation.DefaultPaddingColor);
        }

        public static string GetImage(this HtmlHelper html, string fileName, int width, int height, string paddingColor)
        {
            return GetImage(html, fileName, width, height, String.IsNullOrWhiteSpace(paddingColor) ? (Color?)null : ColorTranslator.FromHtml(paddingColor));
        }

        public static string GetImage(this HtmlHelper html, string fileName, int width, int height, Color? paddingColor)
        {
            try
            {
                if (width == -1 && height == -1)
                {
                    //Return raw image
                    return FileManipulation.UploadDir + "/" + fileName;
                }
                else
                {
                    //Return resized image
                    return new FileManipulation().ResizedImage(new ResizeOptions
                    {
                        FileName = fileName,
                        Width = width,
                        Height = height,
                        PaddingColor = paddingColor
                    });
                }
            }
            catch (Exception)
            {
                return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/_Content/images/noimage.jpg";
            }
        }

        public static object LinkTo(this HtmlHelper html, CourseModel curCourse)
        {
            return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/Course/" + curCourse.Name.ForUrl();
        }
        public static string LinkTo(this HtmlHelper html, NewsItemModel item)
        {
            return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/News/" + item.Title.ForUrl();
        }
        public static string LinkToNews(this HtmlHelper html, DateTime item)
        {
            return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/News?archive=" + item.ToString("MMMM-yyyy");
        }
        public static string LinkToNews(this HtmlHelper html, NewsCategoryModel item)
        {
            return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/News?category=" + item.Name.ForUrl();
        }

        public static bool IsOfType(this FileType curType, FileType compareType)
        {
            if (curType == compareType)
            {
                return true;
            }

            if (compareType == FileType.Any)
            {
                return true;
            }

            return false;
        }

        public static string EncryptPassword(this string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            if (string.IsNullOrEmpty(salt))
            {
                throw new ArgumentNullException("salt");
            }
            var value = password + salt;

            var data = Encoding.Default.GetBytes(value);
            var hash = new SHA1CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static MvcHtmlString ForNotification(this string source)
        {
            if (!String.IsNullOrWhiteSpace(source))
            {
                source = source.Replace("'", "\\'");
            }

            return new MvcHtmlString(source);
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string StringValue(this Enum value)
        {
            string output = value.ToString();
            Type type = value.GetType();

            //Check first in our cached results...

            //Look for our 'StringValueAttribute' 

            //in the field's custom attributes

            FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs =
               fi.GetCustomAttributes(typeof(StringValue),
                                       false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }

        public static string AddOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

        public static string ForUrl(this string source)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            //Basic replacing
            source = source
                .Replace("&", "and")
                .Replace(" ", "-")
                .Replace(".", "");

            //Advanced replace with Uri encoding fallback.
            return Uri.EscapeUriString(Regex.Replace(source, @"[^\w\.@-]", "").ToLower());
        }
    }
    public class StringValue : System.Attribute
    {
        private string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

    }
}