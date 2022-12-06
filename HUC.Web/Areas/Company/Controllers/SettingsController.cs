using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Settings;
using HUC.Web.App;
using HUC.Web.App.Users;
using System.Collections.Generic;

namespace HUC.Web.Areas.Company.Controllers
{
    // [SessionExpireAttribute]
    public class SettingsController : CompanyBaseController
    {
        // GET: Company/Settings

   

        private UsersService _users = new UsersService();
        public ActionResult Index()
        {


            SettingsModel settingsModel = new SettingsModel();

            try
            {

           


            //ViewBag.Settings = Database.GetAll<SettingsModel>("where companyid=" + companyID);

            int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
            var model = Database.GetAll<SettingsModel>("where companyid="+companyID ).FirstOrDefault();



           // if model null
          // insert into setting table


            if (model == null)
            {
             
                Database.ExecuteInsert("settings", new[] { "CompanyId", "TrainingCoursesWeeks", "DaysReminder1", "DaysReminder2", "DaysReminder3", "EmailReminder", "EmailCourseComplete", "EmailAssignCourse", "EmailCompliance" }, new {  CompanyId = companyID, TrainingCoursesWeeks = 4, DaysReminder1 = 1, DaysReminder2 = 1, DaysReminder3 = 1, EmailReminder = false, EmailCourseComplete = false, EmailAssignCourse = false, EmailCompliance = false });

                List<EmailConfigurationModel> EmailConfigurations = new List<EmailConfigurationModel>();


                EmailConfigurations = Database.GetAll<EmailConfigurationModel>("where companyid=-1").ToList();


                foreach (var item in EmailConfigurations)
                {
                    Database.ExecuteInsert("EmailConfigurations", new[] { "CompanyId", "Title", "Subject", "Body", "Description" }, new { CompanyId = companyID, Title = item.Title, Subject = item.Subject, Body = item.Body, Description = item.Description });

                }



            }




            //if (Database.Query<int>("select companyid from settings where companyid=" + companyID).Count() > 0)
            //{
            //    CompCount = Database.Query<int>("select companyid from settings where companyid=" + companyID).FirstOrDefault();
            //}




            var newModel = Database.GetAll<SettingsModel>("where companyid="+ companyID).FirstOrDefault();

                settingsModel = Database.GetAll<SettingsModel>("where companyid=" + companyID).FirstOrDefault();


              


               
              
            }
            catch(Exception ex) { LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG); }

            return View(settingsModel);

        }

        [HttpPost]
        public ActionResult Create(SettingsModel model)
        {
            

            LogApp.Log4Net.WriteLog("create method", LogApp.LogType.GENERALLOG);
            try
            {
               

                int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
                if (companyId == 0)
                {
                    var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                    companyId = GetLoggedInUser.Company.ID;
                }
                model.CompanyId = companyId;

           


                    

              

                    int SettingID = Database.Query<int>("select id from settings where companyid=" + companyId).First();
                    LogApp.Log4Net.WriteLog("going to update settings", LogApp.LogType.GENERALLOG);

                  

                    Database.ExecuteUpdate("settings", new[] { "TraniningOfficerName", "TrainingOfficerEmail" }, new { id = SettingID, TraniningOfficerName = model.TraniningOfficerName, TrainingOfficerEmail = model.TrainingOfficerEmail, });

                   AddSuccessEdit("Settings");

             



                return RedirectToAction("Index");
               
            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG);
            }


            // AddErrorModel();
            return View(model);
        }


        [HttpPost]
        public ActionResult UpdateReminder(SettingsModel model)
        {



            try
            {
              

                int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
                if (companyId == 0)
                {
                    var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                    companyId = GetLoggedInUser.Company.ID;
                }
                model.CompanyId = companyId;


                    int SettingID = Database.Query<int>("select id from settings where companyid=" + companyId).First();
                    LogApp.Log4Net.WriteLog("going to update settings", LogApp.LogType.GENERALLOG);

                    Database.ExecuteUpdate("settings", new[] { "TrainingCoursesWeeks", "DaysReminder1", "DaysReminder2", "DaysReminder3" }, new { id = SettingID, TrainingCoursesWeeks = model.TrainingCoursesWeeks, DaysReminder1 = model.DaysReminder1, DaysReminder2 = model.DaysReminder2, DaysReminder3 = model.DaysReminder3 });

                    AddSuccessEdit("Settings");

     


              
              
            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG);
            }
            return RedirectToAction("Index");

            // AddErrorModel();
            //  return View(model);
        }

        [HttpPost]
        public ActionResult UpdateEmailSent(SettingsModel model)
        {



            try
            {
               

                int companyId = Convert.ToInt32(HttpContext.Session["CompanyId"]);
                if (companyId == 0)
                {
                    var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                    companyId = GetLoggedInUser.Company.ID;
                }
                model.CompanyId = companyId;




                int SettingID = Database.Query<int>("select id from settings where companyid=" + companyId).FirstOrDefault();
                    LogApp.Log4Net.WriteLog("going to update settings", LogApp.LogType.GENERALLOG);

                    Database.ExecuteUpdate("settings", new[] { "EmailReminder", "EmailCourseComplete", "EmailAssignCourse", "EmailCompliance" }, new { id = SettingID, EmailReminder = model.EmailReminder, EmailCourseComplete = model.EmailCourseComplete, EmailAssignCourse = model.EmailAssignCourse, EmailCompliance = model.EmailCompliance });

                    AddSuccessEdit("Settings");

      
            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG);
            }
            return RedirectToAction("Index");

            // AddErrorModel();
            //  return View(model);
        }



        public JsonResult getEmailConfiguration(int EmailConfigId)
        {

        //    try
         //   {
                var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                int companyId = GetLoggedInUser.Company.ID;
                companyId = GetLoggedInUser.Company.ID;



                var emailConfig = Database.GetAll<EmailConfigurationModel>("where id=" + EmailConfigId + "and companyid=" + companyId);
                return Json(emailConfig, JsonRequestBehavior.AllowGet);
         //   }

          


         //catch (Exception ex)
         //   {

         //   }

        

        }

        [ValidateInput(false)]
     
        public JsonResult updateEmailConfig(EmailConfigurationModel emailModel)
        {
              bool isOfficerAdd = true;
            try
            {


                var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                int companyId = GetLoggedInUser.Company.ID;
                //companyId = GetLoggedInUser.Company.ID;


              

             

                    Database.ExecuteUpdate("EmailConfigurations", new[] { "Subject", "Body", "Description" }, new { id = emailModel.Id, Subject = emailModel.Subject, Body = emailModel.Body, Description = emailModel.Description });
                   AddSuccessEdit("Configurations");
             
                



            }
            catch(Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG);
            }
            return Json(isOfficerAdd, JsonRequestBehavior.AllowGet);

        }
    }
    }