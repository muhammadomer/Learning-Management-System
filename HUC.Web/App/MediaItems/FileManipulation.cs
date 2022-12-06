using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HUC.Web.App.MediaItems
{
    public enum FileType
    {
        Any = 0,

        Image = 1,
        Audio = 2,
        Video = 3,

        Zip = 4,
        PDF = 5,
        CSV = 6,

        Unknown = -1
    }

    public static class Extensions
    {
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
                return "/_Content/images/noimage.jpg";
            }
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
    }

    public class FileManipulation
    {
        public static readonly Color? DefaultPaddingColor = Color.White;
        public const string UploadDir = "/_Media/Uploads";
        public const string CacheDir = "/_Media/Cache";

        //Error Messages (For same error messages that are used in different places
        public const string ErrorInvalidFile = "File provided is not valid for the file type required.";



        public bool TryUpload(HttpPostedFileBase file, out string fileName, out string errorMessage)
        {
            return TryUpload(file, FileType.Any, out fileName, out errorMessage);
        }

        public bool TryUpload(HttpPostedFileBase file, FileType type, out string fileName, out string errorMessage)
        {
            var fileRestrictions = new string[] { };

            if (!IsFileOfType(file, type))
            {
                fileName = "";
                errorMessage = FileManipulation.ErrorInvalidFile;

                return false;
            }

            return TryUpload(file, fileRestrictions, out fileName, out errorMessage);
        }

        public bool TryUpload(HttpPostedFileBase file, string[] fileRestrictions, out string fileName, out string errorMessage)
        {
            return UploadFile(file, fileRestrictions, out fileName, out errorMessage);
        }

        public bool IsFileOfType(HttpPostedFileBase file, FileType type)
        {
            var fileExtension = GetExtension(file).ToLower();

            switch (type)
            {
                case FileType.Any:
                    return true;

                case FileType.Image:
                    try
                    {
                        Image.FromStream(file.InputStream).Dispose();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                case FileType.Audio:
                    var audioExtensions = new[] { ".mp3", ".wav" };

                    return audioExtensions.Contains(fileExtension);

                case FileType.Video:
                    var videoExtensions = new[] { ".mpg", ".wmv", ".mp4", ".avi", ".3gp" };

                    return videoExtensions.Contains(fileExtension);

                case FileType.Zip:
                    return fileExtension == ".zip";

                case FileType.PDF:
                    var DocExtensions = new[] { ".xlsx", ".pdf", ".docx",".doc",".pptx",".ppt",".odp" };
                    return DocExtensions.Contains(fileExtension);
                    //if (fileExtension == ".xlsx")
                    //{
                    //    return fileExtension == ".xlsx";
                    //}else if (fileExtension == ".docx")
                    //{
                    //  return  fileExtension == ".docx";
                    //} 
                    //else
                    //return fileExtension == ".pdf";

                case FileType.CSV:
                    return fileExtension == ".csv";

                case FileType.Unknown:
                    return
                        !IsFileOfType(file, FileType.Image) &&
                        !IsFileOfType(file, FileType.Audio) &&
                        !IsFileOfType(file, FileType.Video) &&
                        !IsFileOfType(file, FileType.Zip) &&
                        !IsFileOfType(file, FileType.PDF) &&
                        !IsFileOfType(file, FileType.CSV);

                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public string GetExtension(HttpPostedFileBase file)
        {
            try
            {
                return Path.GetExtension(file.FileName);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public FileType GetFileType(HttpPostedFileBase file)
        {
            foreach (FileType curType in (FileType[])Enum.GetValues(typeof(FileType)))
            {
                if (curType != FileType.Any && this.IsFileOfType(file, curType))
                {
                    return curType;
                }
            }

            return FileType.Unknown;
        }

        public void RotateImage(string fileName, RotateFlipType flipType)
        {
            //Get extension and generate a unique file name.
            var extension = Path.GetExtension(fileName);
            var filePath = HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName);
            var newPath = HttpContext.Current.Server.MapPath("~" + CacheDir + "/" + Guid.NewGuid() + extension);


            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            using (var img = Image.FromStream(stream))
            {
                img.RotateFlip(flipType);
                img.Save(newPath);
            }

            File.Copy(newPath, filePath, true);
            File.Delete(newPath);

            //Remove all files in 'cache' for this image
            RemoveFileRecursively(fileName, HttpContext.Current.Server.MapPath("~" + CacheDir));
        }

        public void RemoveImageFromCache(string fileName)
        {
            RemoveFileRecursively(fileName, HttpContext.Current.Server.MapPath("~" + CacheDir));
        }

        private void RemoveFileRecursively(string fileName, string directory)
        {
            var allFiles = Directory.GetFiles(directory);

            foreach (var curFilePath in allFiles)
            {
                var curFileName = Path.GetFileName(curFilePath);

                if (curFileName == fileName)
                {
                    File.Delete(curFilePath);
                }
            }

            var allDirectories = Directory.GetDirectories(directory);
            foreach (var curDirectoryPath in allDirectories)
            {
                RemoveFileRecursively(fileName, curDirectoryPath);
            }
        }
        public void RemoveImage(string fileName)
        {
            FileInfo f1 = new FileInfo(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));



            if (f1.Exists)
            {
                f1.Delete();
            }

        }   
            
        public bool UploadFile(HttpPostedFileBase file, string[] fileRestrictions, out string fileName, out string errorMessage)
        {

            if (fileRestrictions.Any() && !fileRestrictions.Contains(GetExtension(file)))
            {
                fileName = "";
                errorMessage = ErrorInvalidFile;

                return false;
            }

            try
            {
                var fileInfo = new FileInfo(file.FileName);
                fileName = Guid.NewGuid() + fileInfo.Extension;
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + UploadDir)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + UploadDir));
                }
                file.SaveAs(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));

                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                fileName = "";
                errorMessage = ex.Message;
                return false;
            }
        }
        public bool UploadImage(HttpPostedFileBase file , out string fileName , out string errorMessage)
        {

           
            try
            {
                var fileInfo = new FileInfo(file.FileName);
                fileName = Guid.NewGuid() + fileInfo.Extension;
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + UploadDir)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + UploadDir));
                }
                file.SaveAs(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));

                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                fileName = "";
                errorMessage = ex.Message;
                return false;
            }
        }
        public bool SaveAsNewFile(out string fileName ,string file)
       {
            FileInfo f1 = new FileInfo(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + file));
          
            try
           {                             
                fileName = file;
                if (f1.Exists)
                {
                    var fileInfo = f1.Name;
                    fileName = Guid.NewGuid() + f1.Extension;
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + UploadDir)))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + UploadDir));
                    }
                    f1.CopyTo(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));
                }
                else
                {
                    fileName = "";
                    return false;
                }
               // var fileInfo = new FileInfo(file.FileName);
               //fileName = Guid.NewGuid() + fileInfo.Extension;
               //if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + UploadDir)))
               //{
               //    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + UploadDir));
               //}
               //file.SaveAs(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));
              
             //  errorMessage = "";
               return true;
           }
           catch (Exception ex)
           {
               fileName = "";
             //  errorMessage = ex.Message;
               return false;
           }
       }

        public string ResizedImage(ResizeOptions options)
        {
            string fileName = options.FileName;
            int? width = options.Width;
            int? height = options.Height;
            Color? paddingColor = options.PaddingColor;

            string cacheDimensionDir;
            if (paddingColor.HasValue)
            {
                cacheDimensionDir = CacheDir + "/" + width + "x" + height + "-" + paddingColor.Value.R.ToString("X2") +
                                    paddingColor.Value.G.ToString("X2") + paddingColor.Value.B.ToString("X2");
            }
            else
            {
                cacheDimensionDir = CacheDir + "/" + width + "x" + height;
            }
            //First check if file exists in the cache already
            if (!File.Exists(HttpContext.Current.Server.MapPath("~" + cacheDimensionDir + "/" + fileName)))
            {
                //Resize file
                Image image = Image.FromFile(HttpContext.Current.Server.MapPath("~" + UploadDir + "/" + fileName));
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                /*
                 * original height / original width x new width = new height
                 * 1200 / 1600 x 400 = 300
                 * 
                 */

                //Check for the widths & heights.
                if (width.HasValue && height.HasValue)
                {
                    //Both have values, use the provided values
                }
                else
                {
                    if (!width.HasValue && !height.HasValue)
                    {
                        //Both are null, we need to just return the original dimensions
                        width = originalWidth;
                        height = originalHeight;
                    }
                    else
                    {
                        if (!width.HasValue)
                        {
                            //Just width null
                            height = originalHeight;
                            width = (originalHeight / originalWidth) * height;
                        }
                        else
                        {
                            //Just height null
                            width = originalWidth;
                            height = (originalHeight / originalWidth) * width;
                        }
                    }
                }

                Image resizedImage = new Bitmap(width.Value, height.Value); // changed parm names
                Graphics graphic = Graphics.FromImage(resizedImage);

                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.AntiAlias;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;

                /* ------------------ new code --------------- */

                // Figure out the ratio
                double ratioX = width.Value / (double)originalWidth;
                double ratioY = height.Value / (double)originalHeight;
                // use whichever multiplier is smaller
                double ratio = ratioX < ratioY ? ratioX : ratioY;

                // now we can get the new height and width
                int newHeight = Convert.ToInt32(originalHeight * ratio);
                int newWidth = Convert.ToInt32(originalWidth * ratio);

                // Now calculate the X,Y position of the upper-left corner 
                // (one of these will always be zero)
                int posX = Convert.ToInt32((width - (originalWidth * ratio)) / 2);
                int posY = Convert.ToInt32((height - (originalHeight * ratio)) / 2);

                if (paddingColor.HasValue)
                {
                    graphic.Clear(paddingColor.Value); // padding color
                }
                graphic.DrawImage(image, posX, posY, newWidth, newHeight);
                graphic.Dispose();

                /* ------------- end new code ---------------- */

                ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~" + cacheDimensionDir)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~" + cacheDimensionDir));
                }

                if (!paddingColor.HasValue || Equals(image.RawFormat, ImageFormat.Png))
                {
                    resizedImage.Save(HttpContext.Current.Server.MapPath("~" + cacheDimensionDir + "/" + fileName),
                                      ImageFormat.Png);
                }
                else
                {
                    resizedImage.Save(HttpContext.Current.Server.MapPath("~" + cacheDimensionDir + "/" + fileName),
                                      info[1], encoderParameters);
                }

                image.Dispose();
            }

            return cacheDimensionDir + "/" + fileName;
        }
    }

    public class ResizeOptions
    {
        public string FileName { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public Color? PaddingColor { get; set; }
    }
}
