using System;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Users;
using System.Configuration;
using HUC.Web.Models;
using HUC.Web.Models.SinglePoint;
using System.Linq;
using System.Collections.Generic;

namespace HUC.Web.Controllers
{
    public class BaseController : Controller
    {
        protected AtlasDatabase Database = new AtlasDatabase();
        public const string NotificationKey = "Notification";
        public DentonsEmployeesEntities employeesEntities;
        public BaseController()
        {
            ViewBag._ActiveUser = new UsersService().GetLoggedInUserModel();
            ViewBag.OtherApplication = ConfigurationManager.AppSettings["OtherApplication"];
            employeesEntities = new DentonsEmployeesEntities();

          

             




            
            

           
        }
        public class CompanyUserRoleModel
        {
            public int ID { get; set; }
            public int CompanyID { get; set; }
            public int UserID { get; set; }
            public bool IsAdmin { get; set; }
            public bool IsBackupAdmin { get; set; }
            public bool IsBackupAdminCourseUsable { get; set; }
        }
        public void AddNotification(NoteType type, string message, string title = null)
        {
            var notification = new NotificationModel
            {
                Message = message,
                Title = title,
                Type = type
            };

            TempData[NotificationKey] = notification;
        }

        public void ClearNotification()
        {
            TempData[NotificationKey] = null;
        }

        public void AddError(string message, string title = null)
        {
            AddNotification(NoteType.Error, message, title);
        }

        public void AddInfo(string message, string title = null)
        {
            AddNotification(NoteType.Info, message, title);
        }

        public void AddSuccess(string message, string title = null)
        {
            AddNotification(NoteType.Success, message, title);
        }

        public void AddNote(string message, string title = null)
        {
            AddNotification(NoteType.Note, message, title);
        }

        public void AddErrorModel()
        {
            this.AddError("Please correct the errors below.");
        }

        public void AddSuccessCreate(string type)
        {
            this.AddSuccess("Successfully created '" + type + "'.");
        }

        public void AddSuccessEdit(string type)
        {
            this.AddSuccess("Successfully updated '" + type + "'.");
        }

        public void AddDeleteConfirm(string type, string confirmLink, bool isDestructive = true)
        {
            this.AddNote("You are about to delete this '" + type + "'. " + (isDestructive ? "This is an action that cannot be reverted, please" : "Please") + " <a href=\"" + confirmLink + "\">click here</a> to confirm.");
        }

        public void AddDeleted(string type, string undoLink = null)
        {
            this.AddInfo("The '" + type + "' has been deleted." + (String.IsNullOrWhiteSpace(undoLink) ? "" : " <a href=\"" + undoLink + "\">Click here</a> if you would like to revert this action."));
        }

        public void AddDeleteUndone(string type)
        {
            this.AddInfo("The '" + type + "' has been reverted.");
        }
    }
}
