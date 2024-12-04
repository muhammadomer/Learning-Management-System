using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HUC.Web.Models;
using HUC.Web.Controllers;
using System.Net.Http.Headers;
using HUC.Web.App.Resources.Chapters.Contents;
using AtlasDB;
using iTextSharp.text;

namespace HUC.Web.Controllers
{
    public class ChunkedUploadController : ApiController


    {
        protected AtlasDatabase Database = new AtlasDatabase();
        //[HttpPost]
        [AcceptVerbs("GET", "POST")]
        public async Task<HttpResponseMessage> UploadChunk()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Created };
            LogApp.Log4Net.WriteLog("Response: "+ response.StatusCode, LogApp.LogType.GENERALLOG);
            try
            {
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("No file uploaded or MIME multipart content not as expected!");
                    throw new HttpResponseException(response);
                }

                var meta = new DzMeta(HttpContext.Current.Request.Form);
                var chunkDirBasePath = HttpContext.Current.Server.MapPath("~/App_Data/");
                var path = $@"{chunkDirBasePath}\{meta.dzIdentifier}";
                var filename =
                    $@"{meta.dzFilename}.{(meta.intChunkNumber + 1).ToString().PadLeft(4, '0')}.{meta.dzTotalChunks.PadLeft(4, '0')}.tmp";
                Directory.CreateDirectory(path);

                Request.Content.LoadIntoBufferAsync().GetAwaiter().GetResult();

                int a = 0;


                await Request.Content.ReadAsMultipartAsync(new CustomMultipartFormDataStreamProvider(path, filename))
                    .ContinueWith((task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            response.StatusCode = HttpStatusCode.InternalServerError;
                            response.Content = new StringContent("Chunk upload task is faulted or canceled!");
                            throw new HttpResponseException(response);
                        }
                    });
            }
            catch (HttpResponseException ex)
            {
                // nothing
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent($"Error uploading/saving chunk to filesystem: {ex.Message}");
                LogApp.Log4Net.WriteException(ex);
            }

            return response;
        }


        //  [HttpPut]
        // [HttpPost]
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage CommitChunks([FromUri] string dzIdentifier, [FromUri] string fileName,
            [FromUri] int expectedBytes, [FromUri] int totalChunks, [FromUri] string ChapterId, [FromUri] string contentType)
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var path = "";

            try
            {
                var chunkDirBasePath = HttpContext.Current.Server.MapPath("~/App_Data/");


                if(fileName.Contains(".mp4"))
                {
                    fileName = Guid.NewGuid() + ".mp4";
                }
                else if (fileName.Contains(".mp3"))
                {
                    fileName = Guid.NewGuid() + ".mp3";
                }
                else if (fileName.Contains(".pdf"))
                {
                    fileName = Guid.NewGuid() + ".pdf";
                }
                else if (fileName.Contains(".docx"))
                {
                    fileName = Guid.NewGuid() + ".docx";
                }
                else if (fileName.Contains(".ppt"))
                {
                    fileName = Guid.NewGuid() + ".ppt";
                }
                else if (fileName.Contains(".pptx"))
                {
                    fileName = Guid.NewGuid() + ".pptx";
                }
                else if (fileName.Contains(".xlsx"))
                {
                    fileName = Guid.NewGuid() + ".xlsx";
                }

                path = $@"{chunkDirBasePath}\{dzIdentifier}";
              //  fileName =Guid.NewGuid()+".mp4";
                var dest = Path.Combine(path, HttpUtility.UrlDecode(fileName));

                // Get all files in directory and combine in filestream
                var files = Directory.EnumerateFiles(path).Where(s => !s.Equals(dest)).OrderBy(s => s);

                // Check that the number of chunks is as expected
                if (files.Count() != totalChunks)
                {
                    response.Content = new StringContent(
                        $"Total number of chunks: {files.Count()}. Expected: {totalChunks}!");
                    throw new HttpResponseException(response);
                }

                // Merge chunks into one file
                using (var fStream = new FileStream(dest, FileMode.Create))
                {
                    foreach (var file in files)
                    {
                        using (var sourceStream = System.IO.File.OpenRead(file))
                        {
                            sourceStream.CopyTo(fStream);
                        }
                    }

                    fStream.Flush();
                }

                // Check that merged file length is as expected.
                var info = new FileInfo(dest);
                

                if (info != null)
                {
                    if (info.Length == expectedBytes)
                    {
                        var fn = Path.GetFileName(info.FullName);

                        // info.CopyTo(Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/Final"), fn));
                        info.CopyTo(Path.Combine(HttpContext.Current.Server.MapPath("~/_Media/Uploads"), fn));

                        // Save the file in the database
                        // tTempAtt file = tTempAtt.NewInstance();
                        // file.ContentType = MimeMapping.GetMimeMapping(info.Name);
                        // file.File = System.IO.File.ReadAllBytes(info.FullName);
                        // file.FileName = info.Name;
                        // file.Title = info.Name;
                        // file.TemporaryID = userID;
                        // file.Description = info.Name;
                        // file.User = userID;
                        // file.Date = SafeDateTime.Now;
                        // file.Insert();
                    }
                    else
                    {
                        response.Content = new StringContent(
                            $"Total file size: {info.Length}. Expected: {expectedBytes}!");
                        throw new HttpResponseException(response);
                    }
                }
                else
                {
                    response.Content = new StringContent("Chunks failed to merge and file not saved!");
                    throw new HttpResponseException(response);
                }
            }
            catch (HttpResponseException ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent($"Error merging chunked upload: {ex.Message}");
            }
            finally
            {
                // No matter what happens, we need to delete the temporary files if they exist
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            // RedirectToRoute("", "");



            var model = new ChapterContentModel();

            var prevContents = Database.GetAll<ChapterContentModel>("WHERE ChapterID = @ChapterID", new { ChapterID = ChapterId });
            var lastSort = prevContents.Any() ? prevContents.Max(x => x.Sort) : 0;
            model.Sort = lastSort + 1;
            model.Value = fileName;
            
            model.ChapterID = System.Convert.ToInt32(ChapterId);
            if (fileName.Contains(".mp4"))
            {
                model.ContentType = ContentType.Video;
            }
            else if (fileName.Contains(".mp3"))
            {
                model.ContentType = ContentType.Audio;
            }
            else //if (fileName.Contains(".pdf"))
            {
                model.ContentType = ContentType.PDF;
            }



            Database.ExecuteInsert(model);













            return response;
        }

        [HttpDelete]
        public HttpResponseMessage DeleteCanceledChunks([FromUri] string dzIdentifier, [FromUri] string fileName,
            [FromUri] int expectedBytes, [FromUri] int totalChunks)
        {
            HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            try
            {
                var chunkDirBasePath = HttpContext.Current.Server.MapPath("~/App_Data/");
                var path = $@"{chunkDirBasePath}\{dzIdentifier}";

                // Delete abandoned chunks if they exist
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Content = new StringContent($"Error deleting canceled chunks: {ex.Message}");
            }

            return response;
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage FileUploaded([FromUri] string fileName, [FromUri] string ChapterId)
        {
            var model = new ChapterContentModel();

            var prevContents = Database.GetAll<ChapterContentModel>("WHERE ChapterID = @ChapterID", new { ChapterID = ChapterId });
            var lastSort = prevContents.Any() ? prevContents.Max(x => x.Sort) : 0;
            model.Sort = lastSort + 1;
            model.Value = fileName;
            model.Value = Guid.NewGuid() + ".mp4";
            model.ChapterID =System.Convert.ToInt32(ChapterId);
            model.ContentType =ContentType.Video ;
            Database.ExecuteInsert(model);

          




            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            return response;
        }


        }
    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public readonly string _filename;
        public CustomMultipartFormDataStreamProvider(string path, string filename) : base(path)
        {
            _filename = filename;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return _filename;
        }
    }
}