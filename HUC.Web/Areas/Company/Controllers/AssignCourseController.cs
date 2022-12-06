using HUC.Web.App.Companies;
using HUC.Web.App.Courses;
using HUC.Web.App.Users;
using HUC.Web.Models;
using HUC.Web.Models.SinglePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace HUC.Web.Areas.Company.Controllers
{
    public class AssignCourseController : CompanyBaseController
    {
        UsersService _usersService;
        public AssignCourseController()
        {
            _usersService = new UsersService();
        }
        // GET: Company/AssignCourse
        public ActionResult Index()
        {
            string dbName_SinglePoint = (string)Session["SinglePointDBName"];
            if (String.IsNullOrWhiteSpace(dbName_SinglePoint))
            {
                _usersService.Logout();
            }
            else
            {
                employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePoint);
                ViewBag.UserList = employeesEntities.Users.Where(x => x.IsDeleted == false && x.RolePersmission.Contains("2")).ToList();
                ViewBag.DepartmentList = employeesEntities.Users.Where(x => x.TCLevelPermissionId == 2).Select(x => x.Department).Distinct().ToList();
                ViewBag.PracticeGroupList = employeesEntities.Users.Where(x => x.TCLevelPermissionId == 2).Select(x => x.PracticeGroup).Distinct().ToList();
                ViewBag.AllDepartments = employeesEntities.Departments.Where(x => x.Visible == true).ToList();
                var GetLoggedInUser = new UsersService().GetLoggedInUserModel();
                ViewBag.CoursesList = new CompanyModel().CourseOptions(GetLoggedInUser.Company.ID).OrderBy(x => x.Text, new StrCmpLogicalComparer());
            }
            return View();
        }
        public  JsonResult SendEmail(List<EmailSenders> senders)
        {
            string ok = "yes";
            if(senders.Count > 0)
            {

            foreach (var item in senders)
            {
                _usersService.SendEmail(item);
            }
            }
            else
            {
                ok = "no";
            }
           return Json(new { Feedback = ok}, JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult AssignCourse(List<string> UsersEmail, List<int> UserCourses)
        {
            try
            {



                EmailSenders mail = new EmailSenders();
                LogApp.Log4Net.WriteLog("user email found: " + UsersEmail.Count, LogApp.LogType.GENERALLOG);
                foreach (var email in UsersEmail)
                {
                    int companyID = new UsersService().GetLoggedInUserModel().Company.ID;

                    var user = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + companyID + " AND (IsAdmin = 0 AND IsBackupAdmin = 0)) ", new { Email = email }).FirstOrDefault();
                    LogApp.Log4Net.WriteLog("user courses: " + UserCourses.Count, LogApp.LogType.GENERALLOG);
                    if (UserCourses.Contains(-1))
                    {
                        LogApp.Log4Net.WriteLog("user course contains -1: ", LogApp.LogType.GENERALLOG);
                        Database.Execute("DELETE FROM [dbo].[AssignedCourses] WHERE CompanyID = @CompanyID AND UserID = @UserID", new { CompanyID = companyID, UserID = user.ID });
                        return Json(new { Feedback = true }, JsonRequestBehavior.AllowGet);
                    }

                    var assignedCourses = Database.GetAll<AssignedCoursesModel>().Where(x => Convert.ToInt32(x.UserID) == user.ID && companyID == x.CompanyID).Select(x => x.CourseID).ToList();
                    var newAssinged = UserCourses.Where(x => !assignedCourses.Contains(x)).ToList();
                    LogApp.Log4Net.WriteLog("Asigned courses: " + assignedCourses.Count + " and new courses:" + newAssinged.Count, LogApp.LogType.GENERALLOG);
                    _usersService.AssignCourse(user.ID, companyID, UserCourses);
                    mail.name = user.FirstName + " " + user.LastName;
                    mail.emails = email;
                    mail.courses = Database.GetAll<CourseModel>().Where(x => newAssinged.Contains(x.ID)).Select(x => x.Name).ToList();
                    if (newAssinged.Count > 0)
                    {
                        

                        string dbName_SinglePointName = (string)Session["SinglePointDBName"];
                        LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePointName, LogApp.LogType.GENERALLOG);
                        employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager",  dbName_SinglePointName);

                        var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();


                        bool assigncourse;
                        assigncourse = Database.Query<bool>("select EmailAssignCourse from settings where companyid=" + companyID).First();

                        LogApp.Log4Net.WriteLog("assign course : " + assigncourse, LogApp.LogType.GENERALLOG);

                        if (assigncourse)
                        {
                            _usersService.SendEmailByCompany(mail, model);
                        }


                    }
                }
            }
            catch(Exception ex) { LogApp.Log4Net.WriteLog("Error : " + ex.Message, LogApp.LogType.GENERALLOG); }
            return Json(new { Feedback = true }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUserCourses(string UsersEmail)
        {
           
                int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
            var userCourses = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + companyID + " AND (IsAdmin = 0 AND IsBackupAdmin = 0)) ", new { Email = UsersEmail }).FirstOrDefault().ID;
            var coursIDs = Database.GetAll<AssignedCoursesModel>("Where UserID = " + userCourses.ToString()).ToList();
            string coursids = "";
            if(coursIDs != null && coursIDs.Count() > 0)
            {

            foreach (var item in coursIDs)
            {
                coursids += item.CourseID + "{##}";
            }
            }
            else
            {
                coursids = "no";
            }
           
            return Json(coursids, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDepartmentCourses(int DeptID)
        {

            int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
            var coursIDs = Database.GetAll<DepartmentWithCourse>("Where DeptID = " + DeptID + " and CompanyID =" + companyID).ToList();
            string coursids = "";
            if (coursIDs != null && coursIDs.Count() > 0)
            {

                foreach (var item in coursIDs)
                {
                    coursids += item.CourseID + "{##}";
                }
            }
            else
            {
                coursids = "no";
            }

            return Json(coursids, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddCoursesByDept(int DepartmentID , List<int> UserCourses)
        {
            List<EmailSenders> senders = new List<EmailSenders>();
            
            string dbName_SinglePoint = (string)Session["SinglePointDBName"];
            if (String.IsNullOrWhiteSpace(dbName_SinglePoint))
            {
                _usersService.Logout();
            }
            employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePoint);
            int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
           List<Departments> departments = new List<Departments>();
              //new List<int>();
            var checkedList = Database.GetAll<DepartmentWithCourse>("Where DeptID = @DeptID", new { DeptID = DepartmentID }).Select(x=> x.CourseID).ToList();
            List<int> uncheckList = checkedList.Where(x=> !UserCourses.Contains(x)).ToList();
            var newAssignedCourses = UserCourses.Where(x => !checkedList.Contains(x)).ToList();
           
            if (DepartmentID.Equals(-1))
            {
                departments = employeesEntities.Departments.ToList();
            }
            else
            {
                departments.Add(employeesEntities.Departments.Where(x => x.ID == DepartmentID).FirstOrDefault());              
            }
            foreach (var item in departments)
            {
                var users = employeesEntities.Users.Where(x =>x.Department == item.Name && x.IsDeleted == false).ToList();
                LogApp.Log4Net.WriteLog("users: " +users.Count , LogApp.LogType.GENERALLOG);
                if (users != null && users.Count > 0)
                {
                  
                    foreach (var user in users)
                    {
                       
                        //  var RealUser = Database.GetAll<UserModel>().Where(x=> x.Email.Trim().ToLower() == user.Email.Trim().ToLower()).FirstOrDefault();   //.GetSingle<UserModel>($"Where Email ='{user.Email}'");

                        var RealUser = Database.GetAll<UserModel>().Where(x =>  x.Email.Trim().ToLower() == user.Email.Trim().ToLower() && x.Company.ID==companyID).FirstOrDefault();   //.GetSingle<UserModel>($"Where Email ='{user.Email}'");


                        if (RealUser != null)
                        {
                            LogApp.Log4Net.WriteLog("Real User: " + RealUser.Email , LogApp.LogType.GENERALLOG);
                            EmailSenders email = new EmailSenders();
                            email.courses = Database.GetAll<CourseModel>().Where(x => newAssignedCourses.Contains(x.ID)).Select(x => x.Name).ToList();
                            email.emails = RealUser.Email;
                            email.name = RealUser.FirstName+","+RealUser.LastName;
                            senders.Add(email);
                            var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();
                            bool assigncourse;
                            assigncourse = Database.Query<bool>("select EmailAssignCourse from settings where companyid=" + companyID).First();
                            if (assigncourse)
                            {
                                _usersService.SendEmailByCompany(email, model);
                            }

                            if (uncheckList != null && uncheckList.Count > 0)
                            {
                                _usersService.DeleteAssignCourse(RealUser.ID, uncheckList);
                            }
                            _usersService.UpdateAssignCourseByDepartment(RealUser.ID, companyID, UserCourses);

                          

                        }
                    }
                }
               
            }
            if (DepartmentID.Equals(-1))
            {
                foreach (var item in uncheckList)
                {
                    Database.Execute("DELETE FROM [dbo].[DepartmentWithCourses] WHERE CourseID = @CourseID", new { CourseID = item });
                }
            }
            bool blank = false;
            if(newAssignedCourses.Count() > 0)
            {
                blank = true;
            }
            _usersService.AddingDepartmentCourse(DepartmentID, UserCourses, companyID);
            return Json(new { Feedback = true,emails = senders, isempty = blank }, JsonRequestBehavior.AllowGet);
        }

       /* public JsonResult AddCoursesByDept(List<string> Departments, List<int> UserCourses)
        {

            string dbName_SinglePoint = (string)Session["SinglePointDBName"];
            if (String.IsNullOrWhiteSpace(dbName_SinglePoint))
            {
                _usersService.Logout();
            }
            employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePoint);
            int companyID = new UsersService().GetLoggedInUserModel().Company.ID;
            foreach (var item in Departments)
            {
                var users = employeesEntities.Users.Where(x => x.TCLevelPermissionId == 2 && x.Department == item).ToList();
                if (users != null && users.Count > 0)
                {
                    foreach (var user in users)
                    {

                        var RealUser = Database.GetAll<UserModel>().Where(x => x.Email.Trim().ToLower() == user.Email.Trim().ToLower()).FirstOrDefault();   //.GetSingle<UserModel>($"Where Email ='{user.Email}'");
                        if (RealUser != null)
                        {
                            _usersService.UpdateAssignCourseByDepartment(RealUser.ID, companyID, UserCourses);
                        }
                    }
                }

            }
            return Json(new { Feedback = true }, JsonRequestBehavior.AllowGet);
        }*/
    }
}