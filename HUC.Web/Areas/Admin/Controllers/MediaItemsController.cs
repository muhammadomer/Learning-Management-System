using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.MediaItems;
using HUC.Web.App.MediaItems.Models;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class MediaItemsController : AdminBaseController
    {
        private MediaItemsService _mediaItems;
        private FileManipulation _fileManipulation;

        public MediaItemsController()
        {
            _mediaItems = new MediaItemsService();
            _fileManipulation = new FileManipulation();
        }

        private const string Type = "Media Item";

        public ActionResult Index()
        {
            var model = Database.GetAll<MediaItemModel>("WHERE IsDeleted = 0");

            return View(model);
        }

        [HttpGet]
        public ActionResult Create(FileType? type = null)
        {
            if (type.HasValue)
            {
                var model = new MediaItemAddModel
                {
                    Type = type.Value
                };

                return View(model);
            }
            else
            {
                AddError("Invalid type specified!");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult CreatePost(MediaItemAddModel model)
        {
            if (model.IsLink)
            {
                if (String.IsNullOrWhiteSpace(model.Link))
                {
                    ModelState.AddModelError("Link", "The Link field is required");
                }
            }
            else
            {
                if (model.File == null)
                {
                    ModelState.AddModelError("File", "The File field is required");
                }
                else if (!_fileManipulation.IsFileOfType(model.File, model.Type))
                {
                    ModelState.AddModelError("File", "The File provided is not a valid " + model.Type.ToString());
                }
                else
                {
                    string rawName;
                    string error;
                    if (_fileManipulation.TryUpload(model.File, out rawName, out error))
                    {
                        model.RawFileName = rawName;
                        model.DisplayFileName = model.File.FileName;
                    }
                    else
                    {
                        ModelState.AddModelError("File", error);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteInsert(model);

                AddSuccessCreate(Type);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View("Create", model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<MediaItemEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(MediaItemEditModel model)
        {
            if (model.IsLink)
            {
                if (String.IsNullOrWhiteSpace(model.Link))
                {
                    ModelState.AddModelError("Link", "The Link field is required");
                }
            }
            else
            {
                if (model.File == null)
                {
                    ModelState.AddModelError("File", "The File field is required");
                }
                else if (!_fileManipulation.IsFileOfType(model.File, model.Type))
                {
                    ModelState.AddModelError("File", "The File provided is not a valid " + model.Type.ToString());
                }
                else
                {
                    string rawName;
                    string error;
                    if (_fileManipulation.TryUpload(model.File, out rawName, out error))
                    {
                        model.RawFileName = rawName;
                        model.DisplayFileName = model.File.FileName;
                    }
                    else
                    {
                        ModelState.AddModelError("File", error);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit(Type);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            Database.SoftDelete("MediaItems", id, undo);

            if (undo)
            {
                AddDeleteUndone(Type);
            }
            else
            {
                AddDeleted(Type, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }

        public ActionResult MediaPicker(MediaItemPickerPageModel model, string fileTypesString = null)
        {
            if (TempData["MediaPageModel"] != null)
            {
                model = (MediaItemPickerPageModel)TempData["MediaPageModel"];
            }
            if (!String.IsNullOrWhiteSpace(fileTypesString))
            {
                model.FileTypes = fileTypesString.Split(',').Select(x => (FileType)int.Parse(x)).ToList();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult MediaPicker(MediaItemPickerPageModel model)
        {
            var actualFileType = FileType.Unknown;

            if (model.Add.IsLink)
            {
                if (String.IsNullOrWhiteSpace(model.Add.Link))
                {
                    ModelState.AddModelError("Add.Link", "The Link field is required");
                }
            }
            else
            {
                if (model.Add.File == null)
                {
                    ModelState.AddModelError("Add.File", "The File field is required");
                }
                else if (!_fileManipulation.IsFileOfType(model.Add.File, model.Add.Type))
                {
                    ModelState.AddModelError("Add.File", "The File provided is not a valid " + model.Add.Type.ToString());
                }
                else
                {
                    if (model.Add.Type == FileType.Any)
                    {
                        var fileType = _fileManipulation.GetFileType(model.Add.File);

                        if (fileType == FileType.Unknown)
                        {
                            ModelState.AddModelError("Add.File", "Unknown file type provided!");
                        }
                        else
                        {
                            actualFileType = fileType;
                        }
                    }

                    if (model.Add.Type != FileType.Any || actualFileType != FileType.Unknown)
                    {
                        string rawName;
                        string error;
                        if (_fileManipulation.TryUpload(model.Add.File, out rawName, out error))
                        {
                            model.Add.RawFileName = rawName;
                            model.Add.DisplayFileName = model.Add.File.FileName;
                        }
                        else
                        {
                            ModelState.AddModelError("File", error);
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (model.Add.Type == FileType.Any)
                {
                    model.Add.Type = actualFileType;
                }

                var newID = Database.ExecuteInsert(model.Add, true);

                AddInfo("Successfully created item");

                model.Add = null;
                model.UploadedID = newID;
                TempData["MediaPageModel"] = model;
                return RedirectToAction("MediaPicker");
            }

            ViewBag.UploadTab = model.Add.Type;
            AddError("Correct the errors below");
            return View(model);
        }

        //JSON!
        public JsonResult RedactorUpload()
        {
            var file = Request.Files["file"];
            if (file != null)
            {
                var rawFileName = "";
                var errorMessage = "";
                if (_fileManipulation.TryUpload(file, FileType.Image, out rawFileName, out errorMessage))
                {
                    Database.ExecuteInsert(new MediaItemAddModel
                    {
                        RawFileName = rawFileName,
                        DisplayFileName = file.FileName,
                        IsLink = false,
                        Link = null,
                        Title = Path.GetFileNameWithoutExtension(file.FileName),
                        Type = FileType.Image
                    });

                    return Json(new
                    {
                        filelink = _fileManipulation.ResizedImage(new ResizeOptions
                        {
                            FileName = rawFileName
                        })
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        error = errorMessage
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new
                {
                    error = "No File Provided"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult RedactorList()
        {
            var objList = new List<object>();

            var images = Database.GetAll<MediaItemModel>().Where(x => x.Type == FileType.Image);
            foreach (var curImage in images)
            {
                objList.Add(new
                {
                    thumb = _fileManipulation.ResizedImage(new ResizeOptions { FileName = curImage.RawFileName, Width = 90, Height = 90 }),
                    image = _fileManipulation.ResizedImage(new ResizeOptions { FileName = curImage.RawFileName }),
                    title = curImage.Title
                });
            }

            return Json(objList.ToArray(), JsonRequestBehavior.AllowGet);
        }
	}
}