using HUC.Web.App.Shared;
using HUC.Web.Models;
using HUC.Web.Models.SinglePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using HUC.Web.App.Companies;
using HUC.Web.App.Settings;



namespace HUC.Web.App.Users
{
    public class UsersService : BaseService
    {
        private readonly IPrincipal _principal;

        public UsersService()
        {
            _principal = HttpContext.Current.User;
        }

        public int Create(UserAddModel model)
        {
            model.Salt = Guid.NewGuid().ToString();
            model.Password = model.Password.EncryptPassword(model.Salt);

            var newID = Database.ExecuteInsert(model, true);

            UpdateRoles(newID, model.RoleIDs);

            return newID;
        }

        private void UpdateRoles(int userID, IEnumerable<int> roleIDs)
        {
            Database.Execute("DELETE FROM UserRoles WHERE UserID = @ID", new { ID = userID });

            foreach (var curID in roleIDs)
            {
                Database.Execute("INSERT INTO UserRoles(UserID, RoleID) VALUES(@UserID, @RoleID)", new { UserID = userID, RoleID = curID });
            }
        }
        public void SendEmail(EmailSenders sender)
        {
            try
            {
                if(sender.courses.Count > 0)
                {
                    foreach (var item in sender.courses)
                    {
                        MailMessage mail = new MailMessage();
                        mail.To.Add(sender.emails);
                        mail.From = new MailAddress("webmaster@ghost-digital.com");
                        mail.Subject = "A Course has been assigned to you";
                        string Body = "Dear " + sender.name + ",<br/><br/> ";

                        Body += " The following course has been assigned to you. Please log into Compendium Framework with your user details and click ";
                        Body += "on the “HUC” logo and this will take you to a screen where you can see which course (or courses) have ready for you to start.<br/><br/>";
                        Body += "<b> Course Name : </b> " + item + "<br/>";                            Body += "<br/>Administrator";
                        mail.Body = Body;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "auth.smtp.1and1.co.uk";
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                        smtp.Credentials = new System.Net.NetworkCredential("webmaster@ghost-digital.com", "em@1lsend"); // Enter seders User name and password  
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
           
            }catch(Exception ex)
            {

            }
        }

        //public void SendEmailByCompany(EmailSenders sender , DentonsEmployeesSettings modelsmtp)
        //{
        //    try
        //    {
        //        if (sender.courses.Count > 0)
        //        {
        //            LogApp.Log4Net.WriteLog("Total no of course: " + sender.courses.Count , LogApp.LogType.GENERALLOG);
        //            LogApp.Log4Net.WriteLog("smptp host:  " + modelsmtp.MailServer+ " and smtp username "+modelsmtp.MailUsername  , LogApp.LogType.GENERALLOG);


        //            DateTime startdt= DateTime.Now.Date;

        //          DateTime  finishdt =startdt.AddMonths(1).Date;

        //            foreach (var item in sender.courses)
        //            {
        //                MailMessage mail = new MailMessage();
        //                mail.To.Add(sender.emails);
        //                mail.From = new MailAddress(modelsmtp.MailUsername);
        //                mail.Subject = "A Course has been assigned to you";
        //                string Body = "Dear " + sender.name + ",<br/><br/> ";

        //                Body += " The following course has been assigned to you. Please log into HUC with your user details and click ";
        //                Body += "on the “HUC” logo and this will take you to a screen where you can see which course (or courses) have ready for you to start.<br/><br/>";
        //                Body += "<b> Course Name : </b> " + item + "<br/>"; Body += "<br/>Administrator";

        //                mail.Body = Body;
        //                mail.IsBodyHtml = true;
        //                SmtpClient smtp = new SmtpClient();
        //                smtp.Host = modelsmtp.MailServer;
        //                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //                smtp.Port = modelsmtp.SMTPPort;
        //                smtp.UseDefaultCredentials = false;
        //                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        //                smtp.Credentials = new System.Net.NetworkCredential(modelsmtp.MailUsername, modelsmtp.MailPassword); // Enter seders User name and password  
        //                smtp.EnableSsl = true;
        //                smtp.Send(mail);
        //            }
        //        }
        //        else
        //        {
        //            LogApp.Log4Net.WriteLog("no course found: " , LogApp.LogType.GENERALLOG);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        LogApp.Log4Net.WriteLog(ex.InnerException.Message, LogApp.LogType.GENERALLOG);
        //    }
        //}
        public void SendEmailByCompany(EmailSenders sender, DentonsEmployeesSettings modelsmtp,string username)
        {
            try
            {
                if (sender.courses.Count > 0)
                {
                    LogApp.Log4Net.WriteLog("Total no of course: " + sender.courses.Count, LogApp.LogType.GENERALLOG);
                  //  LogApp.Log4Net.WriteLog("smptp host:  " + modelsmtp.MailServer + " and smtp username " + modelsmtp.MailUsername, LogApp.LogType.GENERALLOG);


                    DateTime startdt = DateTime.Now.Date;

                    DateTime finishdt = startdt.AddDays(28).Date;

                    MailMessage mail = new MailMessage();
                        mail.To.Add(sender.emails);
                        mail.From = new MailAddress(modelsmtp.MailUsername);

                    int companyID = new UsersService().GetLoggedInUserModel().Company.ID;

                  //  EmailConfigurationModel emailConfigurationModel = new EmailConfigurationModel();


                    var emailConfigurationModel = Database.GetAll<EmailConfigurationModel>("where companyid="  +companyID + " and trim(title)='Assigned Course'").FirstOrDefault();

                    mail.Subject = emailConfigurationModel.Subject;


                    //     mail.Subject = "Asignment of courses";


                    string Body = emailConfigurationModel.Body;

                    Body = Body.Replace("[Email To]", sender.name);




                  //      string Body = "Dear <b>" + sender.name + "</b></br> ";

                 //       Body += "<p> The following courses have been assigned to you.</p></br>";


                    string _Courses = "";

                    foreach (var item in sender.courses)
                    {
                        if (sender.courses.Count > 1)
                        {
                            _Courses +=  item + " Course <br/>";
                        }
                        else
                        {
                            _Courses +=  item + " Course ";
                        }

                    }
                    //if (sender.courses.Count > 1)
                    //{
                    //    _Courses = _Courses.Substring(0, _Courses.Length - 5);
                    //    _Courses = _Courses + "</b>";
                    //}





                    //  Body = Body.Replace("[Course List Assigned]", string.Join<string>(", ",sender.courses.ToList()));
                    Body = Body.Replace("[Course List Assigned]", _Courses);





                    var company = Database.GetAll<CompanyModel>("WHERE ID = " + companyID).FirstOrDefault();

                    var TrainingOfficer = Database.GetAll<SettingsModel>("WHERE CompanyID = " + companyID).FirstOrDefault();



                   



                    Body = Body.Replace("[Account Id]", company.Name);
                    Body = Body.Replace("[User Name]", username);

                    Body = Body.Replace("[URL]", "<a href=" + modelsmtp.DentonsEmployeesURL + ">" + modelsmtp.DentonsEmployeesURL + "</a>");


                    Body = Body.Replace("[start_date]", startdt.ToString("dd/MM/yyyy"));
                    Body = Body.Replace("[finish_date]", finishdt.ToString("dd/MM/yyyy"));
                    Body = Body.Replace("[Training Courses Weeks]", TrainingOfficer.TrainingCoursesWeeks.ToString());





                    //      Body += "<p><b>"+TrainingOfficer.TraniningOfficerName+"</b></p></br>";
                    //         Body += "<b>" + company.Name + "</b>";

                    Body = Body.Replace("[Training Officer Name]", TrainingOfficer.TraniningOfficerName);
                    Body = Body.Replace("[Company Name]", company.Name);



                        mail.Body = Body;
                        mail.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = modelsmtp.MailServer;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Port = modelsmtp.SMTPPort;
                        smtp.UseDefaultCredentials = false;
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        smtp.Credentials = new System.Net.NetworkCredential(modelsmtp.MailUsername, modelsmtp.MailPassword); // Enter seders User name and password  
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                  
                }
                else
                {
                    LogApp.Log4Net.WriteLog("no course found: ", LogApp.LogType.GENERALLOG);
                }

            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteLog(ex.InnerException.Message, LogApp.LogType.GENERALLOG);
            }
        }
        public void AssignCourse(int userID,int companyID,List<int> UserCourses)
        {
            Database.Execute("DELETE FROM [dbo].[AssignedCourses] WHERE CompanyID = @CompanyID AND UserID = @UserID", new { CompanyID = companyID, UserID = userID});

            foreach (var courseID in UserCourses)
            {
                Database.Execute("INSERT INTO [dbo].[AssignedCourses] ([UserID], [CompanyID], [CourseID]) VALUES (@UserID, @CompanyID, @CourseID)", 
                    new { UserID = userID, CompanyID = companyID, CourseID = courseID });
            }
        } 
        public void UpdateAssignCourseByDepartment(int userID,int companyID,List<int> UserCourses)
        {
            // Database.Execute("DELETE FROM [dbo].[AssignedCourses] WHERE CompanyID = @CompanyID AND UserID = @UserID", new { CompanyID = companyID, UserID = userID});
          
            foreach (var courseID in UserCourses)
            {

                var AssignCourse = Database.GetAll<AssignedCoursesModel>().Where(x=> x.CourseID==courseID && x.UserID == userID.ToString()).FirstOrDefault();
                if(AssignCourse == null)
                {
                    LogApp.Log4Net.WriteLog("User ID: "+userID + "-Course Id:"+courseID, LogApp.LogType.GENERALLOG);
                    Database.Execute("INSERT INTO [dbo].[AssignedCourses] ([UserID], [CompanyID], [CourseID]) VALUES (@UserID, @CompanyID, @CourseID)", 
                    new { UserID = userID, CompanyID = companyID, CourseID = courseID });
                }
            }
        }

        public void DeleteAssignCourse(int userID,  List<int> UserCourses)
        {
          

            foreach (var courseID in UserCourses)
            {
                Database.Execute("DELETE FROM [dbo].[AssignedCourses] WHERE CourseID = @CompanyID AND UserID = @UserID", new { CompanyID = courseID, UserID = userID });
            }
        }

        public void AddingDepartmentCourse(int DeptID, List<int> UserCourses,int companyID)
        {
            Database.Execute("DELETE FROM [dbo].[DepartmentWithCourses] WHERE DeptID = @DeptID and companyID=@companyID", new { DeptID = DeptID, CompanyID = companyID });

            foreach (var courseID in UserCourses)
            {
                Database.Execute("INSERT INTO [dbo].[DepartmentWithCourses] ([DeptID], [CourseID],[CompanyID]) VALUES (@DeptID, @CourseID,@companyID)",
                    new { DeptID = DeptID, CourseID = courseID, CompanyID = companyID });
            }
        }
        public void Edit(UserEditModel model)
        {
            if (!String.IsNullOrWhiteSpace(model.Password))
            {
                model.Salt = Guid.NewGuid().ToString();
                model.Password = model.Password.EncryptPassword(model.Salt);
            }
            else
            {
                var prevModel = Database.GetSingle<UserModel>(model.ID);

                model.Salt = prevModel.Salt;
                model.Password = prevModel.Password;
            }

            Database.ExecuteUpdate(model);

            UpdateRoles(model.ID, model.RoleIDs);
        }

        public void Logout()
        {
            FormsAuthentication.SignOut();
        }

        public bool LogIn(UserLoginModel model)
        {
            var userModel = Authenticate(model);

            if (userModel != null)
            {
                ProcessLogin(userModel);
                return true;
            }
            return false;
        }

        public UserModel Authenticate(UserLoginModel model,string whereClause = "")
        {
           
            LogApp.Log4Net.WriteLog("Apply filter: " + whereClause, LogApp.LogType.GENERALLOG);
            //var user = Database.GetAll<UserModel>(" INNER JOIN CompanyUsers ON Users.ID = CompanyUsers.UserID WHERE Email = @Email", new { Email = model.Email });//(model.Email, "Email");
            UserModel user = Database.GetAll<UserModel>(" WHERE Email = @Email " + whereClause, new { Email = model.Email }).FirstOrDefault();//(model.Email, "Email");

            if (user != null)
            {
                LogApp.Log4Net.WriteLog("User found: " + model.Email, LogApp.LogType.GENERALLOG);
                var password = model.Password.EncryptPassword(user.Salt);
                LogApp.Log4Net.WriteLog("password : " + password, LogApp.LogType.GENERALLOG);

                //if (password == user.Password)
                //{

                ProcessLogin(user);
                LogApp.Log4Net.WriteLog("process login: ", LogApp.LogType.GENERALLOG);

                return user;
                //}
                
            }
            LogApp.Log4Net.WriteLog("User not found. " + model.Email, LogApp.LogType.GENERALLOG);
            return null;
        }

        private void ProcessLogin(UserModel model)
        {
            LogApp.Log4Net.WriteLog("process login enter: ", LogApp.LogType.GENERALLOG);

            var prevUser = GetLoggedInUserModel();
            if (prevUser != null && prevUser.ID == model.ID)
            {
                //Prevent logging into the account if you are already logged in
            }
            else
            {
                LogApp.Log4Net.WriteLog("else statment: ", LogApp.LogType.GENERALLOG);

                if (prevUser != null)
                {
                    LogApp.Log4Net.WriteLog("if user null statment: ", LogApp.LogType.GENERALLOG);
                    Logout();
                }
                
                var auth = new AuthModel(model.ID, model.Email, model.Roles.Select(x => x.Name).ToList());
                var ticket = new FormsAuthenticationTicket(1, model.Email, DateTime.Now, DateTime.Now.AddDays(2), true,
                                                           auth.ToString(), FormsAuthentication.FormsCookiePath);
                var encypted = FormsAuthentication.Encrypt(ticket);
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encypted));
                LogApp.Log4Net.WriteLog("loglogin: ", LogApp.LogType.GENERALLOG);

                LogLogin(model);
            }
        }

        private void LogLogin(UserModel model)
        {
            //Log the login!
            Database.ExecuteInsert("UserLogins", new[] { "UserID", "LoginOn" }, new { UserID = model.ID, LoginOn = DateTime.Now });
        }

        private AuthModel GetLoggedInUser()
        {
            try
            {
                
                var identity = (FormsIdentity)_principal.Identity;
                var data = identity.Ticket.UserData;
                var model = new AuthModel(data);
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public UserModel GetLoggedInUserModel() 
        {
            var authUser = this.GetLoggedInUser();
            if (authUser == null)
            {
                return null;
            }
            return Database.GetSingle<UserModel>(authUser.ID);
        }

        public List<int> GetAssignedCourses(int UserID) 
        {
            return Database.Query<int>("SELECT CourseID FROM AssignedCourses WHERE UserID = " + UserID).ToList();
        }
    }
}