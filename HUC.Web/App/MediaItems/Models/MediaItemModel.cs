using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HUC.Web.App.MediaItems.Models
{
    public class MediaItemModel
    {
        public int ID { get; set; }

        public FileType Type { get; set; }

        public bool IsLink { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string RawFileName { get; set; }
        public string DisplayFileName { get; set; }

        public string DownloadLink
        {
            get { return HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty + "/Download?file=" + RawFileName; }
        }

        //--Lazy Functions--
        private readonly Dictionary<string, string> _sourceReferenceDictionary = new Dictionary<string, string>();
        private readonly Dictionary<string, HtmlString> _simpleOutputDictionary = new Dictionary<string, HtmlString>();
        private readonly Dictionary<string, MvcHtmlString> _youtubeEmbedDictionary = new Dictionary<string, MvcHtmlString>();

        /// <summary>
        /// This is meant to just return a link to be used either on it's own or from other functions. Width, Height and PaddingColor only used for some filetypes
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="paddingColor"></param>
        /// <returns></returns>
        public string SourceReference(int? width = null, int? height = null, Color? paddingColor = null)
        {
            var key = String.Format("{0}-{1}-{2}", width, height, paddingColor);
            if (!_sourceReferenceDictionary.ContainsKey(key))
            {
                switch (this.Type)
                {
                    case FileType.Image:
                        if (IsLink)
                        {
                            _sourceReferenceDictionary[key] = Link;
                        }
                        else
                        {
                            if (width == null && height == null)
                            {
                                _sourceReferenceDictionary[key] = FileManipulation.UploadDir + "/" + this.RawFileName;
                            }
                            else
                            {
                                _sourceReferenceDictionary[key] = new FileManipulation().ResizedImage(new ResizeOptions
                                {
                                    FileName = this.RawFileName,
                                    Width = width,
                                    Height = height,
                                    PaddingColor = paddingColor
                                });
                            }
                        }
                        break;
                    default:
                        //Anything here is just a link to the file

                        if (IsLink)
                        {
                            _sourceReferenceDictionary[key] = Link;
                        }
                        else
                        {
                            _sourceReferenceDictionary[key] = FileManipulation.UploadDir + "/" + RawFileName;
                        }
                        break;
                }
            }
            
            return _sourceReferenceDictionary[key];
        }

        /// <summary>
        /// This is to output the simplest method of presenting the media item type. Example, for an image it will be an img tag, pdf will be an a tag etc etc...
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="paddingColor"></param>
        /// <returns></returns>
        public HtmlString SimpleOutput(int? width = null, int? height = null, Color? paddingColor = null)
        {
            var key = String.Format("{0}-{1}-{2}", width, height, paddingColor);
            if (!_simpleOutputDictionary.ContainsKey(key))
            {
                var sb = new StringBuilder();

                var sourceRef = SourceReference(width, height, paddingColor);

                switch (this.Type)
                {
                    case FileType.Image:
                        sb.Append("<img src=\"" + sourceRef + "\">");
                        break;
                    default:
                        //Anything here is just a link to the file

                        sb.Append("<a href=\"" + sourceRef + "\" target=\"_blank\">");
                        if (String.IsNullOrWhiteSpace(this.Title))
                        {
                            sb.Append("View File");
                        }
                        else
                        {
                            sb.Append(this.Title);
                        }
                        sb.Append("</a>");
                        break;
                }
                _simpleOutputDictionary[key] = new HtmlString(sb.ToString());
            }

            return _simpleOutputDictionary[key];
        }

        public MvcHtmlString YouTubeEmbed(string styleWidth, string styleHeight)
        {
            var key = String.Format("{0}-{1}", styleWidth, styleHeight);
            if (!_youtubeEmbedDictionary.ContainsKey(key))
            {
                if (!this.IsLink || this.Type != FileType.Video)
                {
                    return new MvcHtmlString("");
                }

                var pattern =
                    @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
                var replacement =
                    "<iframe title='YouTube video player' style=\"width:" + styleWidth + "; height:" + styleHeight + ";\" src='http://www.youtube.com/embed/$1?showinfo=0' frameborder='0' allowfullscreen='1'></iframe>";

                var rgx = new Regex(pattern);
                var result = rgx.Replace(this.Link, replacement);

                _youtubeEmbedDictionary[key] = new MvcHtmlString(result);
            }

            return _youtubeEmbedDictionary[key];
        }
    }
}