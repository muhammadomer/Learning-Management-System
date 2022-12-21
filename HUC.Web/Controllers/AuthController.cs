using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App;
using HUC.Web.App.Users;

namespace HUC.Web.Controllers
{
    public class AuthController : BaseController
    {
        private UsersService _usersService;
        string ConString = ConfigurationManager.ConnectionStrings["SinglePoint_Entities"].ConnectionString;
        int CompanyId = 0;

        public AuthController()
        {
            _usersService = new UsersService();

           




        }

        public ActionResult Login(string ReturnUrl = null)
        {

            //string dbName_SinglePointName = (string)Session["SinglePointDBName"];
            //LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePointName.ToString(), LogApp.LogType.GENERALLOG);
            //employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePointName.ToString());
            //var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();
            //var url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            //return Redirect(model.DentonsEmployeesURL);

            //return Redirect("http://localhost:33366/");
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserLoginModel model, string ReturnUrl = null)
        {
            UserModel authUser = null;
            if (!String.IsNullOrWhiteSpace(model.Email) && !String.IsNullOrWhiteSpace(model.Password))
            {
                authUser = _usersService.Authenticate(model);

                if (authUser == null)
                {
                    ModelState.AddModelError("Email", "Invalid email/password combination");
                    ModelState.AddModelError("Password", "Invalid email/password combination");
                }
                else
                {
                    if (!authUser.IsActive)
                    {
                        ModelState.AddModelError("Email", "This account is not activated.");
                    }
                    if (authUser.IsDeleted)
                    {
                        ModelState.AddModelError("Email", "This account has been removed.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _usersService.LogIn(model);

                if (String.IsNullOrWhiteSpace(ReturnUrl))
                {
                    if (authUser.Company != null)
                    {
                        if (authUser.Company.AllUsers.Any(x => x.UserID == authUser.ID && (x.IsAdmin || x.IsBackupAdmin)))
                        {
                            //This is an admin or backup admin account

                            return RedirectToAction("Index", "Dashboard", new { area = "Company" });
                        }
                        else
                        {
                            //This is a normal user

                            return RedirectToAction("Index", "Dashboard", new { area = "Users" });
                        }
                    }
                    else
                    {
                        if (authUser.HasRole("admin"))
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                        else
                        {
                            return Redirect("/");
                        }
                    }
                }
                else
                {
                    return Redirect(ReturnUrl);
                }
            }

            return View(model);
        }
        [AllowAnonymous]
        public ActionResult LoginForSinglePoint()
        {
            try
            {
                UserModel authUser = null;
                UserLoginModel model = new UserLoginModel();
                NameValueCollection nvc = Request.Form;
                string AccountId2 = Request.QueryString["AccountId"];
                
                LogApp.Log4Net.WriteLog("AccountId2: " + AccountId2, LogApp.LogType.GENERALLOG);
                if (!string.IsNullOrEmpty(AccountId2))
                {
                    LogApp.Log4Net.WriteLog("Account id is not null", LogApp.LogType.GENERALLOG);
                    AccountId2 = AccountId2.Replace("%2B", "+");
                    AccountId2 = AccountId2.Replace("%2C%20", ",");
                    AccountId2 = AccountId2.Replace("%2F", "/");
                    string accountDetailEncrypted = AccountId2;
                    string accountDetailDecrypted = Cryptography.Decrypt(accountDetailEncrypted);
                    string Email = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
                    string Password = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];
                    CompanyId = Convert.ToInt32(accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[2]);
                    bool IsAdmin = Convert.ToBoolean(accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[3]);
                    string SinglePointDBName = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[4];
                    HttpContext.Session["SinglePointDBName"] = SinglePointDBName;
                    HttpContext.Session.Timeout = 400;
                    LogApp.Log4Net.WriteLog("SinglePointDBName: " + SinglePointDBName + " and CompanyId:" + CompanyId, LogApp.LogType.GENERALLOG);
                   
                    // Receive Company ID
                    model.Email = Email;
                    model.Password = Password;
                    if (CompanyId == 0) // Super Admin
                    {
                        authUser = _usersService.Authenticate(model, " AND Users.ID IN (SELECT UserID FROM UserRoles) ");
                    }
                    else
                    {
                        string whereClause = " AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 1 OR IsBackupAdmin = 1)) ";
                        if (!IsAdmin)
                        {
                            whereClause = " AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 0 AND IsBackupAdmin = 0)) ";
                        }
                        authUser = _usersService.Authenticate(model, whereClause);
                    }
                }

                if (authUser != null && authUser.Company != null)
                {
                    if (authUser.Company.AllUsers.Any(x => x.UserID == authUser.ID && (x.IsAdmin || x.IsBackupAdmin)))
                    {
                            using (SqlConnection connection = new SqlConnection(ConString))
                            {
                                SqlDataAdapter da = new SqlDataAdapter("select * from Accounts where TrainingCoursesCompanyId =" + CompanyId, connection);
                                DataTable dt = new DataTable();
                                da.Fill(dt);
                                if (dt.Rows.Count > 0)
                                {
                                    DataRow row = dt.Rows[0];
                                    bool IsCourseAllow = Convert.ToBoolean(row["allowadmincourseTrainingCourses"]);
                                    //string companylogo = row["CompanyLogo"].ToString();
                                    HttpContext.Session["CourseAllowAdmin"] = IsCourseAllow;
                                    HttpContext.Session["CompanyId"] = CompanyId;
                                    //HttpContext.Session["CompanyLogo"] = "https://compendium-cloud-dev.ghost-digital.com" + companylogo.Replace("~/","/");
                                }
                            }
                        return RedirectToAction("Graphs", "Dashboard", new { area = "Company" });
                    }
                    else
                    {
                        //This is a normal user

                        return RedirectToAction("Index", "Dashboard", new { area = "Users" });
                    }
                }
                else
                {
                    if (authUser != null && authUser.HasRole("admin"))
                    {
                        return RedirectToAction("Index", "Courses", new { area = "Admin" });
                    }
                    else
                    {
                        return Redirect("/");
                    }
                }
            }
            catch (Exception ex)
            {
                LogApp.Log4Net.WriteException(ex);
                return Redirect("/");
            }
        }
        [AllowAnonymous]
        public ActionResult LoginUser()
        {
            UserLoginModel model = new UserLoginModel();
            NameValueCollection nvc = Request.Form;
            string AccountId2 = Request.QueryString["AccountId"];
            if (!string.IsNullOrEmpty(AccountId2))
            {
                AccountId2 = AccountId2.Replace("%2B", "+");
                AccountId2 = AccountId2.Replace("%2C%20", ",");
                AccountId2 = AccountId2.Replace("%2F", "/");
                string accountDetailEncrypted = AccountId2;
                string accountDetailDecrypted = Cryptography.Decrypt(accountDetailEncrypted);
                string Email = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
                string Password = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];

                string IsUser = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[2];
                string Compnayuserid = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[3];

                try
                {
                    string SinglePointDBName = accountDetailDecrypted.Split(new string[] { "$$$" }, StringSplitOptions.None)[4];
                    HttpContext.Session["SinglePointDBName"] = SinglePointDBName;


                    //employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", SinglePointDBName);

                    //var _model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();
                    //Redirect(_model.DentonsEmployeesURL);

                    HttpContext.Session.Timeout = 400;
                    LogApp.Log4Net.WriteLog("SinglePointDBName: " + SinglePointDBName + " and CompanyId:" + CompanyId, LogApp.LogType.GENERALLOG);
                }
                catch (Exception ex) {  }

                // Receive Company ID
                // int CompanyId = 104;
                bool IsAdmin = false; // Simple user
                model.Email = Email;
                model.Password = Password;
                UserModel authUser = null;
                if (CompanyId == 0) // Super Admin
                {

                }
                else
                {

                }
                //if (!String.IsNullOrWhiteSpace(model.Email) && !String.IsNullOrWhiteSpace(model.Password))
                //{
                //    authUser = _usersService.Authenticate(model, " INNER JOIN CompanyUsers ON Users.ID = CompanyUsers.UserID ");
                //}

                if (ModelState.IsValid)
                {
                    _usersService.LogIn(model);
                    if (IsUser == "0")
                    {
                
                        return RedirectToAction("Index", "Dashboard", new { area = "Users" });

                    }
                    //if (authUser.Company != null)
                    //{
                    //    //This is a normal user
                        
                    //}
                }
            }
            return View(model);
        }

        //public ActionResult Logout()
        //{
        //    _usersService.Logout();

        //    return Redirect("/");
        //} 
        public void Logout()
        {
            _usersService.Logout();

            Response.Write("@RenderSection('scripts', required: false)@{ <script language='javascript'> {window.close(); }</script>}");

        }
        public ActionResult LogoutGDD()
        {
            _usersService.Logout();

            string dbName_SinglePointName = (string)Session["SinglePointDBName"];
            LogApp.Log4Net.WriteLog("sending email for database : " + dbName_SinglePointName.ToString(), LogApp.LogType.GENERALLOG);
            employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", dbName_SinglePointName.ToString());
            var model = employeesEntities.DentonsEmployeesSettings.FirstOrDefault();


             return Redirect(model.DentonsEmployeesURL+"logOut.aspx");



        }




        public ActionResult LoginPod()
        {
            return View();
        }

        public ActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reset(UserResetModel model)
        {
            var user = Database.GetSingle<UserModel>(model.Email, "Email");
            if (user == null)
            {
                ModelState.AddModelError("Email", "No account exists for this Email address.");
            }

            if (ModelState.IsValid)
            {
                var guid = Guid.NewGuid().ToString();

                Database.ExecuteUpdate("Users", new[] { "ResetPasswordKey" }, new { ID = user.ID, ResetPasswordKey = guid });

                user.ResetPasswordKey = guid;
                new MailController().ResetPassword(user).Deliver();

                AddInfo("An email has been sent to continue with the password reset.");
                return Redirect("/");
            }

            return View(model);
        }

        public ActionResult ResetPassword(string code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                AddError("Invalid code provided.");
            }
            else
            {
                var resettingUser = Database.GetSingle<UserModel>(code, "ResetPasswordKey");

                if (resettingUser == null)
                {
                    AddError("Invalid code provided.");
                }
                else
                {
                    var model = new UserResetPasswordModel
                    {
                        Code = code
                    };

                    return View(model);
                }
            }

            return RedirectToAction("Index", "Site");
        }

        [HttpPost]
        public ActionResult ResetPassword(UserResetPasswordModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Code))
            {
                AddError("Invalid code provided.");
                return RedirectToAction("Index", "Site");
            }
            var resettingUser = Database.GetSingle<UserModel>(model.Code, "ResetPasswordKey");
            if (resettingUser == null)
            {
                AddError("Invalid code provided.");
                return RedirectToAction("Index", "Site");
            }

            if (!String.IsNullOrWhiteSpace(model.Password) && !String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("UserAdd.ConfirmPassword", "The 'Password' field and 'Confirm Password' field must match.");
            }

            if (ModelState.IsValid)
            {
                var editModel = Database.GetSingle<UserEditModel>(resettingUser.ID);
                editModel.Password = model.Password;
                editModel.ConfirmPassword = model.Password;
                editModel.ResetPasswordKey = null;
                editModel.RoleIDs = resettingUser.Roles.Select(x => x.ID);
                _usersService.Edit(editModel);

                AddInfo("Successfully reset password. Please login with your new password.");
                return RedirectToAction("Login");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Activate(string code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                AddError("Invalid code provided.");
            }
            else
            {
                var activatingUser = Database.GetSingle<UserModel>(code, "ActivateKey");

                if (activatingUser == null)
                {
                    AddError("Invalid code provided.");
                }
                else
                {
                    //Valid code, user attained. Activate!

                    Database.ExecuteUpdate("Users", new[] { "ActivateKey", "IsActive" }, new { ID = activatingUser.ID, IsActive = true, ActivateKey = (string)null });

                    AddSuccess("Your account has been successfully activated!");
                }
            }

            return RedirectToAction("Index", "Site");
        }

        public ActionResult ActiveCompany(int id)
        {
            var curUser = _usersService.GetLoggedInUserModel();

            if (curUser.RepresentableCompanies.Any(x => x.ID == id))
            {
                curUser.SetRepresentingCompany(id);
            }

            return Redirect(HttpContext.Request.UrlReferrer?.ToString() ?? "/");
        }

       
    }
}