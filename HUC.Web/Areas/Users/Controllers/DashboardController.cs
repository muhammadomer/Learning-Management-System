using HUC.Web.App;
using HUC.Web.App.Companies;
using HUC.Web.App.Courses.Certificate;
using HUC.Web.App.PageModels;
using HUC.Web.App.Settings;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using HUC.Web.App.Courses.Time;
using HUC.Web.App.Courses;
namespace HUC.Web.Areas.Users.Controllers
{
    public class DashboardController : UsersBaseController
    {
        private readonly UsersService _users = new UsersService();

        public ActionResult Index(int? CourseID = null,int? year = null)
        {

            var user = GetLoggedInUser();
            try
            {


                var result = 0;
            var passper = 0;
          



            int companyID = new UsersService().GetLoggedInUserModel().Company.ID;




            bool CourseComplete=false;




            CourseComplete = Database.Query<bool>("select EmailCourseComplete from settings where companyid=" + companyID).FirstOrDefault();



            LogApp.Log4Net.WriteLog("id: " + CourseID + "--"+ "CourseComplete:"+ CourseComplete, LogApp.LogType.GENERALLOG);


            if (CourseID != null && CourseComplete)
            {


                 var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.Where(x => x.CourseID == CourseID).FirstOrDefault();


                if (curUserCourse.Course.RetakeDuration > 30)
                {
                    Database.ExecuteUpdate("AssignedCourses", new[] { "UserId", "CompanyId", "CourseId", "ReminderStatus" }, new { ID = curUserCourse.ID, UserId = curUser.ID, CourseID = curUserCourse.CourseID, CompanyId = curUser.Company.ID, ReminderStatus = 99 });

                }

                var userCourse = user.UserCourses.FirstOrDefault(x => x.CourseID == CourseID);
                
                result = userCourse.TotalScore * 100 / userCourse.Course.MaxScore;
                passper = userCourse.Course.PassingPercentage;
                string UserName, CourseNAme;
                UserName= user.FirstName + " " + user.LastName;
                CourseNAme = userCourse.Course.Name;
                UserModel um= new UserModel();
                um.FirstName = user.FirstName;
                um.LastName = user.LastName;
                um.Email = user.Email;
                um.ID = user.ID;



     

                LogApp.Log4Net.WriteLog("User email: " + um.Email +"Result:"+result, LogApp.LogType.GENERALLOG);

   
                if (userCourse.IsComplete == true && userCourse.FeedBack == 1)
                {
                    LogApp.Log4Net.WriteLog("IsPass:"+userCourse.IsPass, LogApp.LogType.GENERALLOG);


                    LogApp.Log4Net.WriteLog("Result: "+result+" pss per:"+passper, LogApp.LogType.GENERALLOG);

                if (result >= passper && userCourse.IsPass == true )
                 
                    {


                        SendEmailOnPassedCourse(um, userCourse);
                            Database.ExecuteUpdate("UserCourses", new[] { "FeedBack" }, new { ID = userCourse.ID, FeedBack = 2 });
                            LogApp.Log4Net.WriteLog("User Passed--- " + "Course Name:" + CourseNAme, LogApp.LogType.GENERALLOG);
                    }
                    else //if(userCourse.IsPass == false)
                    {
                        SendEmailOnFailedCourse(um, CourseNAme);
                        LogApp.Log4Net.WriteLog("User Failed--- " + "Course Name:" + CourseNAme, LogApp.LogType.GENERALLOG);
                    }

            }

      

            }


            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteLog("Error----------------- " + ex.InnerException, LogApp.LogType.GENERALLOG);
                //    return View("Error");

            }
            var model = new UserDashboardPageModel
            {
                User = user,
                Year = year,

                Certificates = Database.Query<Certificate>("SELECT * FROM Certificates WHERE UserId = " + user.ID).ToList()
            };
            return View(model);

        }
        public ActionResult DashBoardView()
        {

         

            return View();
        }
        // public void FakeView()
        // {
        //    Response.Write("@RenderSection('scripts', required: false)@{ <script language='javascript'> { window.location.href = '@(HttpContext.Current.Request.ApplicationPath.Length > 1 ? HttpContext.Current.Request.ApplicationPath : string.Empty)/Users/Dashboard/Index' }</script>}");
        //}
        public ActionResult Results(int id)
        {
            var user = GetLoggedInUser();

            var userCourse = user.UserCourses.FirstOrDefault(x => x.CourseID == id);
            if (userCourse == null)
            {
                AddInfo("You have not started this course yet.");
                return RedirectToAction("Index");
            }

            var model = new UserTestResultsPageModel
            {
                User = user,
                UserCourse = userCourse
            };


            //id = 7169;


            //int ID = Database.Query<int>("select id from usercoursetests where usercourseid="+userCourse.ID).FirstOrDefault();

            
           // var times = Database.GetAll<CourseTimeModel>("WHERE UserCourseTestID = @ID", new { ID });

            var times = Database.GetAll<CourseTimeModel>("WHERE UserCourseID = @ID", new { userCourse.ID });


            TimeSpan _timeTaken = new TimeSpan(times.Sum(x => (x.EndOn - x.StartOn).Ticks));

            var tmpStr = "";

            if (_timeTaken.TotalHours >= 1)
            {
                tmpStr += Math.Floor(_timeTaken.TotalHours) + " hour" + (Math.Floor(_timeTaken.TotalHours) > 1 ? "s" : "");
            }
            if (model.UserCourse.TimeTaken.Minutes > 0)
            {
                tmpStr += " " + _timeTaken.Minutes + " minute" + (_timeTaken.Minutes > 1 ? "s" : "");
            }
            if (model.UserCourse.TimeTaken.Seconds > 0)
            {
                tmpStr += " " + _timeTaken.Seconds + " second" + (_timeTaken.Seconds > 1 ? "s" : "");
            }


            ViewBag.totalquestion = model.UserCourse.Course.MaxScore;
            ViewBag.totalanswer = model.UserCourse.TotalScore;
            ViewBag.timetaken = tmpStr;// model.UserCourse.TimeTaken.ToString();

            var result = model.UserCourse.TotalScore * 100 / model.UserCourse.Course.MaxScore;

            if (result >= model.UserCourse.Course.PassingPercentage)
            {
                ViewBag.pass =  "Passed";
            }
            else
            {
                ViewBag.pass = "Failed";
            }
        

            
            
            




            return View(model);
        }

        public ActionResult Edit()
        {
            var model = Database.GetSingle<UserEditModel>(GetLoggedInUser().ID);
            model.Password = null;
            model.Salt = null;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserEditModel model)
        {
            var checkUser = GetLoggedInUser();

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                if (!model.Email.IsValidEmail())
                {
                    ModelState.AddModelError("Email", "The email address provided is in an incorrect format.");
                }
                else
                {
                    var prevUser = Database.GetSingle<UserModel>(model.Email, "Email");
                    if (prevUser != null && prevUser.ID != checkUser.ID)
                    {
                        ModelState.AddModelError("Email", "The email address provided is already in use");
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(model.Password) && !String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The 'Password' field and 'Confirm Password' field must match.");
            }

            if (ModelState.IsValid)
            {
                model.ID = checkUser.ID;
                model.IsActive = checkUser.IsActive;
                model.IsDeleted = checkUser.IsDeleted;
                model.ActivateKey = checkUser.ActivateKey;
                model.ResetPasswordKey = checkUser.ResetPasswordKey;
                model.RoleIDs = new List<int>();
                _users.Edit(model);

                AddSuccessEdit("Your Profile");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public JsonResult PageData(int? year , int next = 0)
        {
            var user = GetLoggedInUser();

            var courseTicks = new List<object[]>();
            var timePerCourseData = new List<decimal[]>();
            var scorePerCourseData = new List<int[]>();
            var timePerCourseHasData = false;
            var scorePerCourseHasData = false;
            var courseCount = 0;


            foreach (var curCourse in user.AllCourses(year).Where(x => x.IsDeleted == false).OrderBy(w => w.Name).Skip(next*10).Take(10))
            {
                courseCount++;
                var userCourse = user.UserCourses.FirstOrDefault(x => x.CourseID == curCourse.ID);

                var courseNameSmall = curCourse.Name;
                if (courseNameSmall.Length > 11)
                {
                    courseNameSmall = courseNameSmall.Substring(0, 10) + "...";
                }

                courseTicks.Add(new object[] { courseCount, courseNameSmall });

                if (userCourse == null)
                {
                    timePerCourseData.Add(new[] { courseCount, 0m });
                    scorePerCourseData.Add(new[] { courseCount, 0 });
                }
                else
                {
                    timePerCourseHasData = true;
                    scorePerCourseHasData = true;

                    timePerCourseData.Add(new[] { courseCount,Math.Round((decimal)userCourse.TimeTaken.TotalMinutes) });
                    scorePerCourseData.Add(new[] { courseCount, (( userCourse.TotalScore * 100) / curCourse.MaxScore) });
                }
            }
            bool checker = timePerCourseData.Count() > 0 ? true : false;
            var timePerCourse = new
            {
                barData = timePerCourseData.ToArray(),
                ticks = courseTicks,
                noData = !timePerCourseHasData,
                existdata = checker
            };
            bool check = scorePerCourseData.Count() > 0 ? true : false;
            var scorePerCourse = new
            {
                barData = scorePerCourseData.ToArray(),
                ticks = courseTicks,
                noData = !scorePerCourseHasData,
                existdata = check
            };


            return Json(new
            {
                TimePerCourse = timePerCourse,
                ScorePerCourse = scorePerCourse,
                TotalRecordScoreInGraph = scorePerCourseData.Count(),
                TotalRecordTimeInGraph = timePerCourseData.Count()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ManageRetake(int CourseID, bool IsDurationEnd=false)
        {
            var curUser = GetLoggedInUser();
            var curUserCourse = curUser.UserCourses.Where(x => x.CourseID == CourseID).FirstOrDefault();
          
            if (curUserCourse != null)
            {
                var curUserCourseTest = curUserCourse.CourseStage.Resource.LatestUserTestFor(curUserCourse.ID);
                foreach (var curResource in curUserCourse.Course.Resources)
                {
                    var curTest = curResource.LatestUserTestFor(curUserCourse.ID);
                  

                    if (curTest != null && curTest.UserQuestions.Count() > 0)
                    {
                        
                        foreach (var item in curTest.UserQuestions)
                        {
                            if (item.UserAnswers.Count() > 0)
                            {
                                foreach (var ans in item.UserAnswers)
                                {
                                    Database.HardDelete("UserCourseTestQuestionAnswers", ans.ID);
                                }
                            }
                            Database.HardDelete("UserCourseTestQuestions", item.ID);


                            //OMER: Code to delete free text answers on Retake
                            if (item.TestQuestion.QuestionType == 7)
                            {


                                int TestQuestionAnswersId = Database.Query<int>("select id from TestQuestionAnswers where testquestionid=" + item.TestQuestionID + "and courseuserid="+curUser.ID).FirstOrDefault();
                                LogApp.Log4Net.WriteLog("testQuestionanswerid" + TestQuestionAnswersId, LogApp.LogType.GENERALLOG);
                                Database.HardDelete("TestQuestionAnswers", TestQuestionAnswersId);
                            }
                          
                        }
                        var userCourse =curUser.UserCourses.Where(x => x.CourseID == CourseID && x.UserID == curUser.ID).FirstOrDefault();
                        Database.ExecuteUpdate("UserCourses", new[] { "FeedBack" }, new { ID = userCourse.ID, FeedBack=false });

                       

                    }
                }
               



                //foreach (var item in curUserCourseTest.UserQuestions)
                //{
                //    if (item.UserAnswers.Count() > 0)
                //    {
                //        foreach (var ans in item.UserAnswers)
                //        {
                //            Database.HardDelete("UserCourseTestQuestionAnswers", ans.ID);
                //        }
                //    }
                //    Database.HardDelete("UserCourseTestQuestions", item.ID);
                //}
                if (IsDurationEnd)
                {
                   
                    Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn", "RetakeDate", "CourseStageID" }, new { ID = curUserCourse.ID, IsComplete = false, CompleteOn = DateTime.Now, RetakeDate = DateTime.Now.AddDays(Convert.ToDouble(curUserCourse.Course.RetakeDuration)), CourseStageID = curUserCourse.Course.FirstStage.ID });
                }
                else
                {
                 Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn", "CourseStageID" }, new { ID = curUserCourse.ID, IsComplete = false, CompleteOn = DateTime.Now, CourseStageID = curUserCourse.Course.FirstStage.ID });
                }
               // Database.ExecuteUpdate("UserCourseTests", new[] {"StartOn" },new {ID=curUserCourseTest.ID,StartOn=DateTime.Now});
                CompleteTest(curUserCourseTest.ID);
            }
            return RedirectToAction("Modules","Courses", new { id = CourseID});
        }
        private void CompleteTest(int id)
        {
            var now = DateTime.Now;

            var userCourseTestEdit = Database.GetSingle<UserCourseTestModel>(id);
            userCourseTestEdit.IsComplete = false;
            userCourseTestEdit.CompleteOn = now;
           // userCourseTestEdit.StartOn = now;
            Database.ExecuteUpdate(userCourseTestEdit);
            Database.Execute("Insert into UserComletedCourseDates (UserCourseID, CompletedDate) Values(@UserCourseID, @CompletedDate)",
               new { UserCourseID = userCourseTestEdit.UserCourseID, CompletedDate = now });
            if (/*userCourseTestEdit.CorrectAnswerCount != userCourseTestEdit.UserQuestions.Count() &&*/ userCourseTestEdit.UserCourse.Course.ReTake != null && userCourseTestEdit.UserCourse.Course.ReTake == true && userCourseTestEdit.UserCourse.Course.CoolDownHours != null)
            {
                Database.Execute("update UserCourses set CoolDownHoursTime = @CoolDownHoursTime where ID = @ID", new { CoolDownHoursTime = DateTime.Now.AddHours(Convert.ToDouble(userCourseTestEdit.UserCourse.Course.CoolDownHours)), ID = userCourseTestEdit.UserCourseID });
            }
        }

        public ActionResult IssueCertificate(int? Id, int? CourseId, int type)
        {
            try
            {

                var user = GetLoggedInUser();

                var userCourse = user.UserCourses.FirstOrDefault(x => x.CourseID == CourseId);
                if (userCourse == null)
                {
                    AddInfo("You have not started this course yet.");
                    return RedirectToAction("Index");
                }

                var model = new UserTestResultsPageModel
                {
                    User = user,
                    UserCourse = userCourse
                };


                int questions = 0;
                int answers = 0;
                foreach (var testresults in model.UserCourse.Course.Resources.Where(x => x.Questions.Any()))
                {
                    var userCourseTests = testresults.UserTestsFor(model.UserCourse.ID);

                    foreach (var userCourseTest in userCourseTests.OrderByDescending(x => x.StartOn))
                    {
                        questions += userCourseTest.MaxScore;
                        answers += userCourseTest.CorrectAnswerCount;

                    }

                }


                var curUserCourse = Database.GetSingle<CourseModel>(CourseId);
                int ans = (curUserCourse.PassingPercentage * questions) / 100;

                ViewBag.Questions = questions;
                ViewBag.Answers = ans;




                if (Id > 0 && CourseId > 0)
                {
                    var certificates = Database.Query<Certificate>("SELECT * FROM Certificates WHERE UserId = " + Id).ToList();
                    var getcertifiy = certificates.Where(w => w.CourseId == CourseId).FirstOrDefault();
                    if(getcertifiy != null)
                    {
                        return View(getcertifiy);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View();
        }


        public void SendEmailOnPassedCourse(UserModel um,UserCourseModel uc)
        {

            try
            {

                string query = "select RetakeDate,RetakeDuration," +
                            "  DATEADD(day, -s.TrainingCoursesWeeks*7, RetakeDate) as compliancedate " +
                           " from Courses c inner" +
                           " join UserCourses uc" +
                           " on c.id = uc.courseid" +
                          "  inner " +
                           " join AssignedCourses ac " +
                          "  on ac.UserID = uc.UserID and ac.CourseID = uc.CourseID " +
                          "  inner join Settings s " +
                          "  on s.CompanyID = ac.CompanyID " +
                           " where c.ID = "+uc.Course.ID +
                         " and uc.userid="+um.ID;


                var data = Database.Query(query, new { ID = 113 });

                LogApp.Log4Net.WriteLog("Retake Date: " + data.Select(x => x.RetakeDate).FirstOrDefault(), LogApp.LogType.GENERALLOG);
                LogApp.Log4Net.WriteLog("duration: " + data.Select(x => x.RetakeDuration).FirstOrDefault(), LogApp.LogType.GENERALLOG);


                DateTime retakedate = data.Select(x => x.RetakeDate).FirstOrDefault();
                DateTime compliancedate = data.Select(x => x.compliancedate).FirstOrDefault();


                int companyID = new UsersService().GetLoggedInUserModel().Company.ID;

                var emailConfigurationModel = Database.GetAll<EmailConfigurationModel>("where companyid=" + companyID + " and trim(title)='Course Passed'").FirstOrDefault();

              


              
            var company = Database.GetAll<CompanyModel>("WHERE ID = " + companyID).FirstOrDefault();


                string Body = emailConfigurationModel.Body;

                Body = Body.Replace("[Email To]", um.FirstName + " "+um.LastName);


                Body = Body.Replace("[Course Title]", uc.Course.Name);
                Body = Body.Replace("[Date_succesful_completion]", retakedate.ToString("dd/MM/yyyy"));

                int nyears = data.Select(x => x.RetakeDuration).FirstOrDefault();
                string nnyears = compliancedate.ToString("dd/MM/yyyy");

                Body = Body.Replace("[nyears days]",nyears.ToString() );
                Body = Body.Replace("[n_years]", nnyears);

                var TrainingOfficer = Database.GetAll<SettingsModel>("WHERE CompanyID = " + companyID).FirstOrDefault();



                //  Body += "<p><b>" + TrainingOfficer.TraniningOfficerName + "</b></p></br>";

                //   Body += "<b>" + company.Name + "</b>";

                Body = Body.Replace("[Training Officer Name]", TrainingOfficer.TraniningOfficerName);

                Body = Body.Replace("[Company Name]", company.Name);

            LogApp.Log4Net.WriteLog("Company ID:"+ companyID + " company:"+company.Name, LogApp.LogType.GENERALLOG);

                //string connectionString = ConfigurationManager.ConnectionStrings["SinglePoint_Entities"].ConnectionString;
                //System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

                //string clouddb = builder.InitialCatalog;

                //query = "select AccountId from "+clouddb+".dbo.Accounts where TrainingCoursesCompanyId = " + companyID;
                //var data1 = Database.Query(query);


                //int dbName_SinglePointName= data1.Select(x => x.AccountId).FirstOrDefault();
                //string singlepoint = ConfigurationManager.AppSettings["singlepoint"];
                  string dbName_SinglePointName = (string)Session["SinglePointDBName"];
                LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePointName.ToString(), LogApp.LogType.GENERALLOG);
                employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager",  dbName_SinglePointName.ToString());



                var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();

                LogApp.Log4Net.WriteLog("Mail server: " + model.MailServer +"Mail User Name :" + model.MailUsername, LogApp.LogType.GENERALLOG);



                MailMessage mail = new MailMessage();
                mail.To.Add(um.Email);


                if (TrainingOfficer.TrainingOfficerEmail != "")
                {
               //     mail.To.Add(TrainingOfficer.TrainingOfficerEmail);
                }

                string appemail = ConfigurationManager.AppSettings["ApplicationEmails"];

                foreach (var address in appemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
              //      mail.To.Add(address);
                }


                mail.From = new MailAddress(model.MailUsername);
                mail.Subject = emailConfigurationModel.Subject;

                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = model.MailServer;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Port = model.SMTPPort;
                smtp.UseDefaultCredentials = false;
             //   System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                smtp.Credentials = new System.Net.NetworkCredential(model.MailUsername, model.MailPassword); // Enter seders User name and password  
                smtp.EnableSsl = true;
                smtp.Send(mail);



            }
            catch(Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.Message, LogApp.LogType.GENERALLOG);
            }

        }
        public void SendEmailOnFailedCourse(UserModel um, string CourseName)
        {


            int companyID = GetLoggedInUser().Company.ID;

            var emailConfigurationModel = Database.GetAll<EmailConfigurationModel>("where companyid=" + companyID + " and trim(title)='Course Failed'").FirstOrDefault();

            string Body = emailConfigurationModel.Body;

            Body = Body.Replace("[Email To]", um.FirstName + " " + um.LastName);


            Body = Body.Replace("[Course Title]", CourseName);


            var company = Database.GetAll<CompanyModel>("WHERE ID = " + companyID).FirstOrDefault();

            var TrainingOfficer = Database.GetAll<SettingsModel>("WHERE CompanyID = " + companyID).FirstOrDefault();

           

            Body = Body.Replace("[Training Officer Name]", TrainingOfficer.TraniningOfficerName);

            Body = Body.Replace("[Company Name]", company.Name);

            //string connectionString = ConfigurationManager.ConnectionStrings["SinglePoint_Entities"].ConnectionString;
            //System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

            //string clouddb = builder.InitialCatalog;

            //string query = "select AccountId from "+clouddb+".dbo.Accounts where TrainingCoursesCompanyId = " + companyID;
            //var data1 = Database.Query(query);


           // int dbName_SinglePointName = data1.Select(x => x.AccountId).FirstOrDefault();

              string dbName_SinglePointName = (string)Session["SinglePointDBName"];
            LogApp.Log4Net.WriteLog("sending email for database : " + "test_singlepoint_" + dbName_SinglePointName.ToString(), LogApp.LogType.GENERALLOG);
            employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager",  dbName_SinglePointName.ToString());



            var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();

            MailMessage mail = new MailMessage();
            mail.To.Add(um.Email);


            if (TrainingOfficer.TrainingOfficerEmail != "")
            {
          //      mail.To.Add(TrainingOfficer.TrainingOfficerEmail);
            }
            //string appemail = ConfigurationManager.AppSettings["ApplicationEmails"];


            //foreach (var address in appemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    mail.To.Add(address);
            //}


            mail.From = new MailAddress(model.MailUsername);
            mail.Subject = emailConfigurationModel.Subject;
            mail.Body = Body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = model.MailServer;
            smtp.Port = model.SMTPPort;
            smtp.UseDefaultCredentials = false;
       //     System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new System.Net.NetworkCredential
                    (model.MailUsername, model.MailPassword);
            //Or your Smtp Email ID and Password
            smtp.EnableSsl =true;
            smtp.Send(mail);

        }

    }
}