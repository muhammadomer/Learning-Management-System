using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Companies.Courses;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.Courses.Certificate;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using HUC.Web.App.PageModels;
using System.Net.Mail;
using HUC.Web.App.Settings;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;

namespace HUC.Web.Areas.Company.Controllers
{
    public class UsageController : CompanyBaseController
    {
        private UsersService _users = new UsersService();

        public ActionResult Index()
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;

            return View(company);
        }

        


        public ActionResult Course(int id,int? year=null)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var course = CourseModel.GetIfInCompany(id, company.ID,year);
            course.Year = year;
            if (course == null)
            {
                AddError("Invalid Course Provided.");
                return RedirectToAction("Index");
            }
            string yearStr="";
            //if (year != null)
            //{
            //    yearStr = " year(assignedcoursedate)="+year + "and ";
            //}
            //else
            //{
            var userList = Database.Query<string>("SELECT [UserID] FROM [AssignedCourses] WHERE   CourseID = " + id + "and CompanyID = " + company.ID).ToList();

            //}
            ViewBag._Company = company;
            var resources = course.Resources.ToList();
            if(userList != null)
            {
                ViewBag.UserList = userList;
                var intUsers = userList.Select(int.Parse).ToArray();
                foreach (var item in resources)
                {
                    foreach (var userid in intUsers)
                    {
                        var certifalready = Database.Query<Certificate>("SELECT * FROM Certificates WHERE UserId = " + userid + "and CourseId = " + id).Count();
                        if (certifalready == 0)
                        {
                            var usercert = company.AllUsers.Where(w => w.UserID == userid).FirstOrDefault();
                            if(usercert != null)
                            {
                                usercert.IsIssueCertificate = 0;
                            }
                        }
                        else
                        {
                            var usercert = company.AllUsers.Where(w => w.UserID == userid).FirstOrDefault();
                            if (usercert != null)
                            {
                                usercert.IsIssueCertificate = 1;
                            }
                        }
                        if (item.ModuleCompleted == false)
                        {
                            var FreeText = item.Questions.Where(w => w.QuestionType == 7).ToList();
                            foreach (var q in FreeText)
                            {
                                var freetextanswers = q.Answers.Where(w => w.TestQuestionID == q.ID && w.IsCorrect == true && userid == w.CourseUserId && w.IsResult == 0).ToList();
                                if (freetextanswers.Count() > 0)
                                {
                                    var user = company.AllUsers.Where(w => w.UserID == userid).FirstOrDefault();
                                    user.HasFreeText = 1;
                                }
                            }
                        }
                    }
                }
            }
            
           
            return View(course);
        }

        public ActionResult FreetextDetail(int id , int? userid)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var course = CourseModel.GetIfInCompany(id, company.ID);
            var resources = course.Resources.ToList();
            List<QuestionViewModel> list = new List<QuestionViewModel>();
            foreach (var item in resources)
            {
                if (item.ModuleCompleted == false)
                {
                    foreach (var q in item.Questions.Where(w => w.QuestionType == 7).ToList())
                    {
                        var freetextanswers = q.Answers.Where(w => w.IsCorrect == true&& w.CourseUserId == userid && w.TestQuestionID == q.ID && w.IsResult == 0).ToList();
                        foreach (var ans in freetextanswers)
                        {
                            QuestionViewModel model = new QuestionViewModel();
                            model.ModuleName = item.Name;
                            model.Answer = ans.Answer;
                            model.ID = ans.ID;
                            model.CourseUserId = ans.CourseUserId;
                            model.IsCorrect = ans.IsCorrect;
                            model.Question= q;
                            model.TestQuestionID = ans.TestQuestionID;
                            model.Sort = ans.Sort;
                            model.IsResult = ans.IsResult;
                            list.Add(model);
                        }
                        ViewBag.Freetextanswers = list;
                    }
                    ViewBag.countlist = list.Count();
                }

            }
            return View(course);
        }

        public JsonResult FreetextresultAnswers(int questionid , int testquestion , int sort , string Answer , int userid , int btnvalue)
        {
            try
            {
                TestQuestionAnswerEditModel model = new TestQuestionAnswerEditModel();
                if(btnvalue == 1)
                {
                    model.IsCorrect = false;
                    model.IsResult = 1;
                }
                else
                {
                    model.IsCorrect = true;
                    model.IsResult = 2;
                }
                model.ID = questionid;
                model.TestQuestionID = testquestion;
                model.Sort = sort;
                model.CourseUserId = userid;
                model.Answer = Answer;
                Database.ExecuteUpdate(model);
                return Json(1);
            }
            catch (Exception ex)
            {

            }
            return Json(0) ;
        }

        public ActionResult CourseEdit(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyCourse = company.Courses.SingleOrDefault(x => x.ID == id);
            if (companyCourse == null)
            {
                AddError("Invalid Course Provided.");
                return RedirectToAction("Index");
            }

            var model = Database.GetSingle<CompanyCourseEditModel>(id);

            return View(model);
        }

        [HttpPost]
        public ActionResult CourseEdit(CompanyCourseEditModel model)
        {

            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyCourse = company.Courses.SingleOrDefault(x => x.ID == model.ID);
            if (companyCourse == null)
            {
                AddError("Invalid Course Provided.");
                return RedirectToAction("Index");
            }

            //Resets
            model.CompanyID = companyCourse.CompanyID;
            model.CourseID = companyCourse.CourseID;

            //

            if (model.ComplianceScoreMinimum > companyCourse.Course.MaxScore)
            {
                ModelState.AddModelError("ComplianceScoreMinimum", "Your compliance minimum score cannot be higher than the max attainable score for this course.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccessEdit("Course");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public JsonResult CourseData(int id,int? year=null)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var course = CourseModel.GetIfInCompany(id, company.ID,year);
            if (course == null)
            {
                return null;
            }

            

            var testTicks = new List<object[]>();
            var timeRevisingData = new List<decimal[]>();
            var averageScoreData = new List<decimal[]>();
            var timeRevisingHasData = false;
            var averageScoreHasData = false;
            var testCount = 1;
            foreach (var curTestResource in course.Resources.Where(x => x.Questions.Any()))
            {
                var testNameSmall = curTestResource.Name;
                if (testNameSmall.Length > 15)
                {
                    testNameSmall = testNameSmall.Substring(0, 15) + "...";
                }

                testTicks.Add(new object[] { testCount, testNameSmall });

                var latestTests = curTestResource.LatestUserTestsForCompany(company.ID,year);

                if (latestTests.Any())
                {
                    timeRevisingHasData = true;
                    averageScoreHasData = true;
                    timeRevisingData.Add(new[] { testCount, Math.Round((decimal)TimeSpan.FromMinutes((double)curTestResource.AverageRevisionTimeMinutes(company)).TotalMinutes, 1) });
                    averageScoreData.Add(new[] { testCount, Math.Round(latestTests.Sum(x => (x.CorrectAnswerCount * 100m) / (decimal)x.MaxScore) / latestTests.Count()) });
                }

                testCount++;
            }

            var passFailData = new List<object>();
            var passCount = 0;
            var failCount = 0;
            var notStartedCount = 0;
            var notCompletedCount = 0;

            var usersForCourse = new List<UserModel>().AsEnumerable();
            if (company.Courses.Select(x => x.CourseID).Contains(course.ID))
            {
                //All company users have access
                usersForCourse = company.AllUsers.Where(x=>x.User!=null && x.User.IsDeleted==false).Select(x => x.User);
            }
            else
            {
                //Only want the users with access within company
                usersForCourse = Database.GetAll<UserModel>(
                    "WHERE ID IN (SELECT UserID FROM UserCourses WHERE CourseID = @CourseID) AND ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID)",
                    new
                    {
                        CourseID = course.ID,
                        CompanyID = company.ID
                    });
            }


            foreach (var curUser in usersForCourse.GroupBy(x => x.ID).Select(x => x.First()))
            {

                if (year == null)
                {

                }

                var userCourse = year == null ? curUser.UserCourses.FirstOrDefault(x => x.CourseID == course.ID): curUser.UserCourses.FirstOrDefault(x => x.CourseID == course.ID && Convert.ToDateTime(x.StartedOn).Year==year)  ;

                if (userCourse != null)
                {
                    if (userCourse.IsComplete)
                    {
                        var result = ((userCourse.TotalScore * 100) / userCourse.Course.MaxScore);
                        if (result >= userCourse.Course.PassingPercentage)
                        {
                            passCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    else
                    {
                        notCompletedCount++;
                    }
                }
                else
                {
                    if(curUser.CoursesReadyToStart.Where(w => w.ID == course.ID).Count() > 0)
                    {
                        notStartedCount++;
                    }
                    //notStartedCount++;
                }
            }
            passFailData.Add(new
            {
                label = "Pass",
                data = (passCount * 100) / usersForCourse.GroupBy(x => x.ID).Select(x => x.First()).Count(),
                color = "#1aae88"
            });
            passFailData.Add(new
            {
                label = "Fail",
                data = (failCount * 100) / usersForCourse.GroupBy(x => x.ID).Select(x => x.First()).Count(),
                color = "#e40424"
            });
            passFailData.Add(new
            {
                label = "Not Started",
                data = (notStartedCount * 100) / usersForCourse.GroupBy(x => x.ID).Select(x => x.First()).Count(),
                color = "lightgrey"
            });
            passFailData.Add(new
            {
                label = "Not Complete",
                data = (notCompletedCount * 100) / usersForCourse.GroupBy(x => x.ID).Select(x => x.First()).Count(),
                color = "grey"
            });

            var timeRevising = new
            {
                barData = timeRevisingData.ToArray(),
                ticks = testTicks,
                noData = !timeRevisingHasData
            };

            var averageScore = new
            {
                barData = averageScoreData.ToArray(),
                ticks = testTicks,
                noData = !averageScoreHasData
            };

            var passFail = new
            {
                pieData = passFailData.ToArray()
            };


            return Json(new
            {
                AverageTimeRevising = timeRevising,
                AverageScore = averageScore,
                PassFail = passFail
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IssueCertificate(int? Id,int? UserId , int? CourseId , int type)
        {
            try
            {
                if(Id > 0 && CourseId > 0)
                {
                    var companyUser = Database.GetSingle<CompanyUserModel>(Id);

                    LogApp.Log4Net.WriteLog("Course ID:" + CourseId + "--Company User First Name:" + companyUser.User.FirstName, LogApp.LogType.GENERALLOG);
                    var get = companyUser.User.UserCourses.FirstOrDefault(x => x.CourseID == CourseId);

                   // LogApp.Log4Net.WriteLog("get:" + get.StartedOn , LogApp.LogType.GENERALLOG);

                  

                    var user =_users.GetLoggedInUserModel();


                    var settingsModel = Database.GetAll<SettingsModel>().Where(x => x.CompanyId == user.Company.ID).FirstOrDefault();

                    string trainingOfficerName = settingsModel.TraniningOfficerName;

                    LogApp.Log4Net.WriteLog("user id:" + Id, LogApp.LogType.GENERALLOG);

                    var userCourse = Database.GetAll<UserCourseModel>().Where(x => x.CourseID == CourseId && x.UserID == UserId).FirstOrDefault();
                    LogApp.Log4Net.WriteLog("userCourse:" + userCourse, LogApp.LogType.GENERALLOG);

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

                    LogApp.Log4Net.WriteLog("Question:" , LogApp.LogType.GENERALLOG);

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


                    var _Course = Database.GetSingle<CourseModel>(CourseId);

                    int ans = (_Course.PassingPercentage * questions) / 100;

                    ViewBag.Questions = questions;
                    ViewBag.Answers = ans;




                    LogApp.Log4Net.WriteLog("Question:"+questions, LogApp.LogType.GENERALLOG);

                    if (get != null)
                    {
                        if (get.IsComplete)
                        {
                            if (get.IsPass)
                            {
                                LogApp.Log4Net.WriteLog("get.IsPass" + get.IsPass, LogApp.LogType.GENERALLOG);


                                var curUserCourse = Database.GetSingle<CourseModel>(CourseId);
                                ViewBag.Course = curUserCourse;
                                ViewBag.align = type;
                                var certifalready = Database.Query<Certificate>("SELECT * FROM Certificates WHERE UserId = " + companyUser.User.ID + "and CourseId = " + CourseId).Count();
                                if(certifalready == 0)
                                {
                                    Certificate certify = new Certificate();
                                    certify.CourseId = curUserCourse.ID;
                                    certify.CourseName = curUserCourse.Name;
                                    certify.UserId = companyUser.User.ID;
                                    certify.UserName = companyUser.User.FirstName + " " + companyUser.User.LastName;
                                    certify.IssueDate = DateTime.Now;
                                    certify.IsIssue = true;
                                    certify.CompanyName = companyUser.Company.Name;
                                    certify.IsPass = true;
                                    var loginuser = _users.GetLoggedInUserModel();
                                    certify.CreatedBy = loginuser.FirstName + " " + loginuser.LastName;
                                    Database.ExecuteInsert(certify);
                                    WriteTexttoImage(companyUser.User.Email, companyUser.User.FirstName + " " + companyUser.User.LastName, companyUser.Company.Name, DateTime.Now.ToString(), questions.ToString(), answers.ToString(),trainingOfficerName,curUserCourse.Name,companyUser.Company.Name);

                                    return View(certify);
                       

                                    

                                }
                                else
                                {
                                    TempData["Issue"] = "Certificate Already Issue";
                                //    WriteTexttoImage(user.Email, user.FirstName + " " + user.LastName, user.Company.Name, DateTime.Now.ToString(), questions.ToString(), answers.ToString());

                                    return RedirectToAction("Course", new { Id = CourseId });
                                }
                            }
                        }
                    }
                }
            }
           catch(Exception ex)
            {
                LogApp.Log4Net.WriteLog("Question--:"+ex.Message, LogApp.LogType.GENERALLOG);

            }
            return View();
        }

        private void WriteTexttoImage(string email,string username,string companyname,string issuedate,string questions,string answers,string officerName,string courseName,string companyName)
        {



            LogApp.Log4Net.WriteLog("companyname:" + companyname + "username:" + username, LogApp.LogType.GENERALLOG);

            PointF usernameLocation = new PointF(540f, 1170f);
            PointF companynameLocation = new PointF(540f, 1450f);
            PointF issuedateLocation = new PointF(540f, 1720f);
            PointF questionsLocation = new PointF(1370f, 1960f);
            PointF answersLocation = new PointF(620f, 2040f);


            string imageFilePath = Server.MapPath("~/_Content/images/Certificate/CertificateV3.png"); 
            Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFilePath);//load the image file
            LogApp.Log4Net.WriteLog("imageFilePath--bitmapfrom:" + imageFilePath, LogApp.LogType.GENERALLOG);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                {
                    graphics.DrawString(username, arialFont, Brushes.Black, usernameLocation);
                    graphics.DrawString(companyname, arialFont, Brushes.Black, companynameLocation);
                    graphics.DrawString(issuedate, arialFont, Brushes.Black, issuedateLocation);
                    graphics.DrawString(questions, arialFont, Brushes.Black, answersLocation);
                    graphics.DrawString(answers, arialFont, Brushes.Black, questionsLocation); 

                }
            }
            string FileName = "Certificate" + DateTime.UtcNow.Ticks + ".png";
            string imgfilepath = System.Web.HttpContext.Current.Server.MapPath("~/_Content/images/certificate\\" + FileName);
            bitmap.Save(System.Web.HttpContext.Current.Server.MapPath("~/_Content/images/certificate\\" + FileName));//save the image file
           
            LogApp.Log4Net.WriteLog("imageFilePath--bitmapsave:" + FileName, LogApp.LogType.GENERALLOG);
            EmailCertificate(email, username, FileName,officerName,courseName,companyName);

        }

        private void EmailCertificate(string email,string name,string filename,string officerName,string courseName,string companyName)
        {



            string FileName = "Certificate" + DateTime.UtcNow.Ticks + ".pdf";
           Document document = new Document(PageSize.A4, 0f, 0f, 30f, 30f);
           System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();


            FileStream output = new FileStream(System.Web.HttpContext.Current.Server.MapPath("~/_Content/images/certificate\\" + FileName), FileMode.Create);
            LogApp.Log4Net.WriteLog("FileStream output:" + FileName, LogApp.LogType.GENERALLOG);

            PdfWriter writer = PdfWriter.GetInstance(document, output);
            document.Open();

          
           

         

            string imageURL =Server.MapPath("~/_Content/images/Certificate\\" + filename);
            LogApp.Log4Net.WriteLog("imageURL:" + imageURL, LogApp.LogType.GENERALLOG);

            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

            //Resize image depend upon your need

            jpg.ScaleToFit(600f, 860f);

            //Give space before image

            jpg.SpacingBefore = 10f;

            //Give some space after the image

            jpg.SpacingAfter = 1f;

            jpg.Alignment = iTextSharp.text.Image.UNDERLYING;



          //  document.Add(p);
               //  document.Add(chunk2);

            document.Add(jpg);



            document.Close();







            /////////////////////////////////////////////////////////////////////
            string dbName_SinglePointName = (string)Session["SinglePointDBName"];
            LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePointName, LogApp.LogType.GENERALLOG);
            employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePointName);

            var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();
            MailMessage mail = new MailMessage();
            mail.To.Add(email);
            mail.From = new MailAddress(model.MailUsername);

            mail.Subject = "Certificate Issued";
            mail.Body = "<p>Hi "+  name + "</p></br>";
            mail.Body += "<p>Congratulations, you have successfully completed the "+courseName+" course and appended is a certificate top authenticate that you have passed.</p></br>";
            mail.Body += "<p>Well done</p></br>";
            mail.Body += "<p>Kind Regards</p></br>";
            mail.Body += "<p>" + officerName + "</p></br>";
            mail.Body += "<p>" + companyName + "</p></br>";


            Attachment attachment = new Attachment(Server.MapPath("~/_Content/images/Certificate/" + FileName));
            LogApp.Log4Net.WriteLog("attachment:" + FileName, LogApp.LogType.GENERALLOG);
            mail.Attachments.Add(attachment);

            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = model.MailServer;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Port = model.SMTPPort;
            smtp.UseDefaultCredentials = false;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            smtp.Credentials = new System.Net.NetworkCredential(model.MailUsername, model.MailPassword); // Enter seders User name and password  
            smtp.EnableSsl = true;
            LogApp.Log4Net.WriteLog("mail.To:" + email + "MailUsername:" + model.MailUsername, LogApp.LogType.GENERALLOG);
            smtp.Send(mail);
        }


        
    }
}