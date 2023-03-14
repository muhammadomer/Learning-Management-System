using AtlasDB;
using Dapper;
using dotless.Core.Abstractions;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.MediaItems;
using HUC.Web.App.Resources;
using HUC.Web.App.Resources.Chapters;
using HUC.Web.App.Resources.Chapters.Contents;
using HUC.Web.App.Resources.Questions;
using HUC.Web.App.Resources.Questions.Answers;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;


namespace HUC.Web.Services.Controllers
{

    public class UserServiceController : ApiController
    {
        private UsersService _usersService;
        private AtlasDatabase Database = new AtlasDatabase();
        private FileManipulation _files = new FileManipulation();

        public UserServiceController()
        {
            _usersService = new UsersService();
            
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public HttpResponseMessage Users()
        {
            List<string> userlist = new List<string>();
            userlist.Add("asd");
            return Request.CreateResponse(HttpStatusCode.OK, userlist);
        }

        [HttpPost]
        public string Login(UserLoginModel model)
        {
            UserModel authUser = null;
            if (!String.IsNullOrWhiteSpace(model.Email) && !String.IsNullOrWhiteSpace(model.Password))
            {
                authUser = _usersService.Authenticate(model);

                if (authUser == null)
                {
                    return "Invalid email/password combination";
                }
                else
                {
                    if (!authUser.IsActive)
                    {
                        return "This account is not activated.";
                    }
                    if (authUser.IsDeleted)
                    {
                        return "This account has been removed.";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _usersService.LogIn(model);

                //if (String.IsNullOrWhiteSpace(ReturnUrl))
                //{
                if (authUser.Company != null)
                {
                    if (authUser.Company.AllUsers.Any(x => x.UserID == authUser.ID && (x.IsAdmin || x.IsBackupAdmin)))
                    {
                        //This is an admin or backup admin account

                        return "Company";
                    }
                    else
                    {
                        //This is a normal user

                        return "Users";
                    }
                }
                else
                {
                    if (authUser.HasRole("admin"))
                    {
                        return "Admin";
                    }
                }
            }
            //}

            return "Error";
        }
        public HttpResponseMessage Logout()
        {
            _usersService.Logout();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        // CRUD For Admin User

        #region Admin User and company Registration
        //[Authorize(Roles = "Admin")]
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage CreateAdmin(UserAddModel model)
        {
            string response = "";
            string[] splittedEmails = model.Email.Split(new string[] { "$$$" }, StringSplitOptions.None);
            bool userExist = true;
            var checkModel = new UserModel();
            if (splittedEmails.Count() > 1)
            {
                string currentEmail = splittedEmails[0];
                string previousEmail = splittedEmails[1];
                model.Email = currentEmail;
                checkModel = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM UserRoles) ", new { Email = previousEmail }).FirstOrDefault(); //(model.Email, "Email")
                if (checkModel == null)
                {
                    userExist = false;
                }
            }
            if (ModelState.IsValid && String.IsNullOrWhiteSpace(response))
            {
                if (!userExist)
                {
                    model.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                    model.IsActive = true;
                    var newID = _usersService.Create(model);
                    LogApp.Log4Net.WriteLog("Message: This is from when inserting Admin. " + "UserID:" + newID, LogApp.LogType.GENERALLOG);
                    return Request.CreateResponse(HttpStatusCode.OK, "1");
                }
                else
                {
                    UserEditModel userEditModel = new UserEditModel();
                    userEditModel.Email = model.Email;
                    userEditModel.Password = model.Password;
                    userEditModel.ConfirmPassword = model.ConfirmPassword;
                    userEditModel.FirstName = model.FirstName;
                    userEditModel.LastName = model.LastName;
                    userEditModel.ResetPasswordKey = checkModel.ResetPasswordKey;
                    userEditModel.ActivateKey = checkModel.ActivateKey;
                    userEditModel.IsDeleted = checkModel.IsDeleted;
                    userEditModel.IsActive = checkModel.IsActive;
                    userEditModel.ID = checkModel.ID;
                    userEditModel.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                    _usersService.Edit(userEditModel);
                    LogApp.Log4Net.WriteLog("Message: This is from when Updating Admin. " + "UserID:" + userEditModel.ID + " Deleted:" + userEditModel.IsDeleted + " Email : " + userEditModel.Email + " First Name: " + userEditModel.FirstName + " Last Name: " + userEditModel.LastName, LogApp.LogType.GENERALLOG);
                    return Request.CreateResponse(HttpStatusCode.OK, "1");
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }
        //[Authorize(Roles = "Admin")]
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage EditAdmin(UserEditModel model)
        {
            string currentEmail = model.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
            string previousEmail = model.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];
            model.Email = currentEmail;
            var checkModel = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM UserRoles) ", new { Email = previousEmail }).FirstOrDefault(); //(model.Email, "Email");
            string response = "";
            if (ModelState.IsValid)
            {
                model.ResetPasswordKey = checkModel.ResetPasswordKey;
                model.ActivateKey = checkModel.ActivateKey;
                model.IsDeleted = checkModel.IsDeleted;
                model.IsActive = checkModel.IsActive;
                model.ID = checkModel.ID;
                model.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                _usersService.Edit(model);
                return Request.CreateResponse(HttpStatusCode.OK, "1");
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }
        //[Authorize(Roles = "Admin")]
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage DeleteAdmin(string Email)
        {
            var checkModel = Database.GetSingle<UserEditModel>(Email, "Email");
            if (checkModel != null)
            {
                checkModel.Email = checkModel.ID.ToString() + "@" + checkModel.ID.ToString() + ".com";
                checkModel.IsDeleted = true;
                checkModel.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                _usersService.Edit(checkModel);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, false);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetCoursesList(int CompanyId = 0)
        {
            var coursesList = new CompanyModel().CourseadminOptions(CompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, coursesList);
        }
        [HttpGet]
        public HttpResponseMessage GetUserLimit(int CompanyId)
        {
            var companyDetail = Database.GetSingle<CompanyModel>(CompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, companyDetail.UserLimit);
        }

        [HttpPost]
        public HttpResponseMessage GetCompanyUserRoles(int CompanyId , int userid)
        {
            List<CompanyUserRoleModel> model = Database.Query<CompanyUserRoleModel>("select * from CompanyUsers where CompanyID = @companyId and UserID = @TCuserId" , new { companyId = CompanyId , TCuserId = userid }).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [HttpPost]
        public HttpResponseMessage GetCompanyEmailConfig(int CompanyId,string Title)
        {
            CompanyEmailConfig model = Database.Query<CompanyEmailConfig>("select s.TraniningOfficerName,* from EmailConfigurations ec inner join settings s on ec.companyid=s.companyid where ec.CompanyID = @companyId and trim(title)=@title", new { companyId = CompanyId,title=Title }).FirstOrDefault();
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }




        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage CreateCompany(CompanyAddModel model)
        {
           
            string[] companyDetail = model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None);
            bool IfExistCompany = false;
            if (companyDetail.Count() > 1)
            {
                int CompanyId = Convert.ToInt32(model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[1]);
                string CompanyName = model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
                var detailModel = Database.GetSingle<CompanyEditModel>(CompanyId);
                if (detailModel != null)
                {
                    IfExistCompany = true;
                    model.Name = CompanyName;
                }
                else
                {
                    detailModel.Name = CompanyName;
                    detailModel.UserLimit = model.UserLimit;
                    detailModel.CourseIDs = model.CourseIDs;

                    Database.ExecuteUpdate(detailModel);
                    UpdateEnumerables(detailModel.ID, detailModel.CourseIDs);

                    if (detailModel.IsDemonstration)
                    {
                        Database.Execute("UPDATE Users SET IsActive = 1 WHERE ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @ID)", new { ID = detailModel.ID });

                        Database.Execute("UPDATE Companies SET IsDemonstration = 1 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
                    }
                    else
                    {
                        Database.Execute("UPDATE Companies SET IsDemonstration = 0 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
                    }
                    NewCompanyValues values = new NewCompanyValues();
                    values.CompanyId = CompanyId;
                    return Request.CreateResponse(HttpStatusCode.OK, values);
                }
            }
            if (!IfExistCompany)
            {
                var companyID = Database.ExecuteInsert(model, true);
                UpdateEnumerables(companyID, model.CourseIDs);

                model.UserAdd.RoleIDs = new List<int>();
                model.UserAdd.IsActive = true;
                var userID = _usersService.Create(model.UserAdd);
                Database.ExecuteInsert(new CompanyUserAddModel
                {
                    CompanyID = companyID,
                    UserID = userID,
                    IsAdmin = true
                });
                NewCompanyValues values = new NewCompanyValues();
                values.CompanyId = companyID;
                values.UserId = userID;
                return Request.CreateResponse(HttpStatusCode.OK, values);
            }
            else
            {
                int CompanyId = Convert.ToInt32(companyDetail[1]);// Convert.ToInt32(model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[1]);
                string CompanyName = companyDetail[0];// model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
                var detailModel = Database.GetSingle<CompanyEditModel>(CompanyId);
                detailModel.Name = CompanyName;
                List<int> courseid = new List<int>();
                if(model.CourseIDs != null)
                {
                    foreach (var item in detailModel.Courses)
                    {
                        if(item.Course.IsCreatedBy == true)
                        {
                            courseid.Add(item.CourseID);
                        }
                        //else
                        //{
                        //    courseid.Add(item.CourseID);
                        //}
                    }
                    foreach (var item in model.CourseIDs)
                    {
                        courseid.Add(item);
                    }
                    detailModel.CourseIDs = courseid;
                }
                detailModel.UserLimit = model.UserLimit;
               // detailModel.CourseIDs = model.CourseIDs;
                Database.ExecuteUpdate(detailModel);
                UpdateEnumerables(detailModel.ID, detailModel.CourseIDs);

                if (detailModel.IsDemonstration)
                {
                    Database.Execute("UPDATE Users SET IsActive = 1 WHERE ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @ID)", new { ID = detailModel.ID });

                    Database.Execute("UPDATE Companies SET IsDemonstration = 1 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
                }
                else
                {
                    Database.Execute("UPDATE Companies SET IsDemonstration = 0 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
                }
                NewCompanyValues values = new NewCompanyValues();
                values.CompanyId = CompanyId;
                return Request.CreateResponse(HttpStatusCode.OK, values);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetCompanyDetail(int id)
        {
            var model = Database.GetSingle<CompanyEditModel>(id);
            model.CourseIDs = model.Courses.Select(x => x.CourseID);
            return Request.CreateResponse(HttpStatusCode.OK, model.Courses);
        }
        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage EditCompany(CompanyEditModel model)
        {
            int CompanyId = Convert.ToInt32(model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[1]);
            string CompanyName = model.Name.Split(new string[] { "$$$" }, StringSplitOptions.None)[0];
            var detailModel = Database.GetSingle<CompanyEditModel>(CompanyId);
            detailModel.Name = CompanyName;
            detailModel.UserLimit = model.UserLimit;
            detailModel.CourseIDs = model.CourseIDs;

            Database.ExecuteUpdate(detailModel);
            UpdateEnumerables(detailModel.ID, detailModel.CourseIDs);

            if (detailModel.IsDemonstration)
            {
                Database.Execute("UPDATE Users SET IsActive = 1 WHERE ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @ID)", new { ID = detailModel.ID });

                Database.Execute("UPDATE Companies SET IsDemonstration = 1 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
            }
            else
            {
                Database.Execute("UPDATE Companies SET IsDemonstration = 0 WHERE ID IN (" + String.Join(", ", detailModel.DescendantIDsIncludingSelf) + ")");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "1");
        }

        private void UpdateEnumerables(int id, IEnumerable<int> CourseIDs)
        {
            var prevCompany = Database.GetSingle<CompanyModel>(id);

            var sql = "";
            var sqlParams = new DynamicParameters();
            sqlParams.Add("ID", id);

            if (CourseIDs != null && CourseIDs.Any())
            {
                var prevCourses = prevCompany.Courses;

                //First we compare the prev companies with our data
                foreach (var curPrevCourse in prevCourses)
                {
                    if (CourseIDs.Any(x => x == curPrevCourse.CourseID))
                    {
                        //We have a match, do nothing to preserve company customisations.

                    }
                    else
                    {
                        //Item only exists in old company, doesn't exist in new model. Remove occurance
                        sql += "DELETE FROM CompanyCourses WHERE ID = " + curPrevCourse.ID + ";";
                    }
                }

                //Now we iterate all items in the Course list that doesn't exist in the prevCourses list
                foreach (var curNewCourse in CourseIDs.Where(x => !prevCourses.Any(y => y.CourseID == x)))
                {
                    //This item is new, we insert!
                    sql += "INSERT INTO CompanyCourses(CompanyID, CourseID) VALUES(@ID, " + curNewCourse + ");";
                }
            }
            else
            {
                var prevCourses = prevCompany.Courses;
                foreach (var curPrevCourse in prevCourses)
                {
                    if (CourseIDs.Any(x => x == curPrevCourse.CourseID))
                    {
                        //We have a match, do nothing to preserve company customisations.

                    }
                    else
                    {
                        //Item only exists in old company, doesn't exist in new model. Remove occurance
                        sql += "DELETE FROM CompanyCourses WHERE ID = " + curPrevCourse.ID + ";";
                    }
                }

            //    sql += "DELETE FROM CompanyCourses WHERE ID = " + curPrevCourse.ID + ";";

            }






            if (!String.IsNullOrWhiteSpace(sql))
            {
                Database.Execute(sql, sqlParams);
            }
        }
        #endregion

        #region simple user registration

        [AcceptVerbs("GET", "POST")]

        public HttpResponseMessage EmailAlreadyExist(CompanyUserAddModel model)
        {
            string email = model.UserAdd.Email.Split('+')[0].Trim();
            int tcuserid = model.UserID;
            int emailcount = Database.Query<int>("select count(*)  from users where email='" + email + "'").FirstOrDefault();
            LogApp.Log4Net.WriteLog("email count: " +emailcount, LogApp.LogType.GENERALLOG);

            if (emailcount > 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Email already exist");

            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "1");
            }
        }
       
        [AcceptVerbs("GET", "POST")]


        public HttpResponseMessage updateEmailAlreadyExist(useremail model)
        {
            string email = model.Email;
            int tcuserid = model.UserID;
            int emailcount = Database.Query<int>("select count(*)  from users where email='" + email + "' and id!="+tcuserid  ).FirstOrDefault();
            LogApp.Log4Net.WriteLog("email count: " + emailcount, LogApp.LogType.GENERALLOG);

            if (emailcount > 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Email already exist");

            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "1");
            }
        }

        [AcceptVerbs("GET", "POST")]


        public HttpResponseMessage CreateUser(CompanyUserAddModel model)
        {
            string[] splittedEmails = model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None);
            bool IfExistUser = false;


           // string email1 = model.UserAdd.Email.Split('+')[0].Trim();
           // int emailcount = Database.Query<int>("select id  from users where email='" + email1 + "'").FirstOrDefault();


            if (splittedEmails.Count() > 1)
            {
                var response = "";
                string Email = model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[0].Trim();
                string CompanyId = model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[1].Trim();
                int PrevUserLevel = Convert.ToInt32(model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[2].Trim());
                int CurrentUserLevel = Convert.ToInt32(model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[3].Trim());
                string PrevEmail = model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[4].Trim();
                int userid = Convert.ToInt32(model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[5].Trim());
                bool isDeleted = Convert.ToBoolean(model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[6].Trim());
                string roleuser = model.UserAdd.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[7].Trim();
                List<int> val = roleuser.Split(',').Select(int.Parse).ToList();
                
                //role set in company user multiple


                //code added by omer

               List<int> compUserID = Database.Query<int>("select id  from CompanyUsers where CompanyId=" + CompanyId + " and UserId=" + userid  ).ToList();

                foreach (var id in compUserID)
                {
                    Database.HardDelete("CompanyUsers",id);
                }

                foreach (var item in val)
                {
                    if(item == 1)
                    {
                        int count = Database.Query<int>("select COUNT(*)  from CompanyUsers where CompanyId=" + CompanyId + " and UserId=" + userid + " and IsAdmin = 0 and IsBackupAdmin = 1" ).FirstOrDefault();
                        if(count == 0)
                        {
                            if (userid > 0)
                            {
                                CompanyUserModel rolemodel = new CompanyUserModel();
                                rolemodel.CompanyID = Convert.ToInt32(CompanyId);
                                rolemodel.UserID = Convert.ToInt32(userid);
                                rolemodel.IsAdmin = false;
                                rolemodel.IsBackupAdmin = true;
                                rolemodel.IsBackupAdminCourseUsable = false;
                                Database.ExecuteInsert(rolemodel);
                            }
                        }
                    }
                    if (item == 2)
                    {
                        int count = Database.Query<int>("select COUNT(*)  from CompanyUsers where CompanyId=" + CompanyId + " and UserId=" + userid + " and IsAdmin = 0 and IsBackupAdmin = 0").FirstOrDefault();
                        if (count == 0)
                        {
                            if(userid > 0)
                            {
                                CompanyUserModel rolemodel = new CompanyUserModel();
                                rolemodel.CompanyID = Convert.ToInt32(CompanyId);
                                rolemodel.UserID = Convert.ToInt32(userid);
                                rolemodel.IsAdmin = false;
                                rolemodel.IsBackupAdmin = false;
                                rolemodel.IsBackupAdminCourseUsable = false;
                                Database.ExecuteInsert(rolemodel);
                            }
                        }
                    }
                    if (item == 3)
                    {
                        int count = Database.Query<int>("select COUNT(*)  from CompanyUsers where CompanyId=" + CompanyId + " and UserId=" + userid + " and IsAdmin = 1 and IsBackupAdmin = 0").FirstOrDefault();
                        if (count == 0)
                        {
                            if (userid > 0)
                            {
                                CompanyUserModel rolemodel = new CompanyUserModel();
                                rolemodel.CompanyID = Convert.ToInt32(CompanyId);
                                rolemodel.UserID = Convert.ToInt32(userid);
                                rolemodel.IsAdmin = true;
                                rolemodel.IsBackupAdmin = false;
                                rolemodel.IsBackupAdminCourseUsable = false;
                                Database.ExecuteInsert(rolemodel);
                            }
                        }
                    }
                }
                var user = new UserModel();
                if (userid > 0)
                {
                    user = Database.GetSingle<UserModel>(userid);
                }
                else
                {
                    user = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 1 OR IsBackupAdmin = 1)) ", new { Email = PrevEmail }).FirstOrDefault();//(model.Email, "Email");
                    if (PrevUserLevel == 2 || PrevUserLevel == 0)
                    {
                        user = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 0 AND IsBackupAdmin = 0)) ", new { Email = PrevEmail }).FirstOrDefault();//(model.Email, "Email");
                    }
                }
                //model.UserAdd.Email = Email;


                if (user != null)
                {
                    IfExistUser = true;
                    if (ModelState.IsValid)
                    {
                        var editModel = new CompanyUserEditModel();
                        editModel.UserEdit = new UserEditModel();
                        var checkItem = Database.GetSingle<CompanyUserModel>(user.ID, "UserID");
                        editModel.UserID = checkItem.UserID;
                        editModel.CompanyID = checkItem.CompanyID;

                        editModel.UserEdit.ID = checkItem.User.ID;
                        editModel.UserEdit.Email = Email;
                        editModel.UserEdit.FirstName = model.UserAdd.FirstName;
                        editModel.UserEdit.LastName = model.UserAdd.LastName;
                        editModel.UserEdit.IsActive = checkItem.User.IsActive;
                        editModel.UserEdit.IsDeleted = isDeleted;
                        editModel.UserEdit.ActivateKey = checkItem.User.ActivateKey;
                        editModel.UserEdit.ResetPasswordKey = checkItem.User.ResetPasswordKey;
                        editModel.UserEdit.RoleIDs = new List<int>();
                        _usersService.Edit(editModel.UserEdit);

                        model.IsAdmin = checkItem.IsAdmin;
                        Database.ExecuteUpdate(model);
                        //if (CurrentUserLevel == 1 && !checkItem.IsAdmin)
                        //{
                        //    Database.Execute("UPDATE CompanyUsers SET IsBackupAdmin = 1 WHERE ID = @UserID;",
                        //    new
                        //    {
                        //        UserID = checkItem.ID
                        //    });
                        //}
                        //else if (CurrentUserLevel == 2)
                        //{
                        //    Database.Execute(
                        //       "UPDATE CompanyUsers SET IsBackupAdmin = 0 WHERE ID = @UserID;",
                        //       new
                        //       {
                        //           UserID = checkItem.ID
                        //       });
                        //}
                        LogApp.Log4Net.WriteLog("Message: This is from when Updating User. " + "UserID:" + user.ID + " Deleted:" + isDeleted + "CompanyID:" + CompanyId + " Email : " + Email + " First Name: " + model.UserAdd.FirstName + " Last Name: " + model.UserAdd.LastName + " CurrentUserLevel : " + CurrentUserLevel + " CurrentUserLevel : " + PrevUserLevel, LogApp.LogType.GENERALLOG);
                        return Request.CreateResponse(HttpStatusCode.OK, user.ID.ToString());
                    }
                }
                else
                {
                    model.UserAdd.Email = Email;
                    model.CompanyID = Convert.ToInt32(CompanyId);
                }

            }
            else
            {
                string Email = model.UserAdd.Email.Split('+')[0].Trim();
                string CompanyId = model.UserAdd.Email.Split('+')[1].Trim();
                model.UserAdd.Email = Email;
                model.CompanyID = Convert.ToInt32(CompanyId);
            }
            if (!IfExistUser)
            {
                if (ModelState.IsValid)
                {
                    //string Email = model.UserAdd.Email.Split('+')[0].Trim();
                    //string CompanyId = model.UserAdd.Email.Split('+')[1].Trim();
                    model.UserAdd.RoleIDs = new List<int>();
                    model.UserAdd.ActivateKey = Guid.NewGuid().ToString();
                    model.UserAdd.ResetPasswordKey = null;
                    model.UserAdd.IsActive = true;//company.IsDemonstration;
                    var newID = _usersService.Create(model.UserAdd);
                    model.UserID = newID;
                    Database.ExecuteInsert(model);
                    LogApp.Log4Net.WriteLog("Message: This is from when Inserting User. " + "UserID:" + newID + " Email : " + model.UserAdd.Email + " First Name: " + model.UserAdd.FirstName + " Last Name: " + model.UserAdd.LastName, LogApp.LogType.GENERALLOG);
                    return Request.CreateResponse(HttpStatusCode.OK, newID.ToString());
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user data for Training Courses");
            
        }

        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage EditUser(CompanyUserEditModel model)
        {
            var response = "";
            string Email = model.UserEdit.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[0].Trim();
            string CompanyId = model.UserEdit.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[1].Trim();
            int PrevUserLevel = Convert.ToInt32(model.UserEdit.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[2].Trim());
            int CurrentUserLevel = Convert.ToInt32(model.UserEdit.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[3].Trim());
            string PrevEmail = model.UserEdit.Email.Split(new string[] { "$$$" }, StringSplitOptions.None)[4].Trim();
            model.UserEdit.Email = Email;
            UserModel user = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 1 OR IsBackupAdmin = 1)) ", new { Email = PrevEmail }).FirstOrDefault();//(model.Email, "Email");
            if (PrevUserLevel == 2 || PrevUserLevel == 0)
            {
                user = Database.GetAll<UserModel>(" WHERE Email = @Email AND Users.ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = " + CompanyId + " AND (IsAdmin = 0 AND IsBackupAdmin = 0)) ", new { Email = PrevEmail }).FirstOrDefault();//(model.Email, "Email");
            }
            var checkItem = Database.GetSingle<CompanyUserModel>(user.ID, "UserID");

            if (ModelState.IsValid)
            {
                model.UserID = checkItem.UserID;
                model.CompanyID = checkItem.CompanyID;

                model.UserEdit.ID = checkItem.User.ID;
                model.UserEdit.IsActive = checkItem.User.IsActive;
                model.UserEdit.IsDeleted = checkItem.User.IsDeleted;
                model.UserEdit.ActivateKey = checkItem.User.ActivateKey;
                model.UserEdit.ResetPasswordKey = checkItem.User.ResetPasswordKey;
                model.UserEdit.RoleIDs = new List<int>();
                _usersService.Edit(model.UserEdit);

                model.IsAdmin = checkItem.IsAdmin;
                Database.ExecuteUpdate(model);
                //if (CurrentUserLevel == 1 && !checkItem.IsAdmin)
                //{
                //    Database.Execute("UPDATE CompanyUsers SET IsBackupAdmin = 1 WHERE ID = @UserID;",
                //    new
                //    {
                //        UserID = checkItem.ID
                //    });
                //}
                //else if (CurrentUserLevel == 2)
                //{
                //    Database.Execute(
                //       "UPDATE CompanyUsers SET IsBackupAdmin = 0 WHERE ID = @UserID;",
                //       new
                //       {
                //           UserID = checkItem.ID
                //       });
                //}
                return Request.CreateResponse(HttpStatusCode.OK, "1");
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, response);
        }

        public HttpResponseMessage DeleteUser(string email)
        {
            var company = _usersService.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(email, "Email");
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No user exists");
            }
            if (companyUser.IsAdmin)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete the company admin");
            }

            Database.SoftDelete("CompanyUsers", companyUser.ID);
            Database.SoftDelete("Users", companyUser.UserID);

            //Set Email has deleted
            if (!string.IsNullOrWhiteSpace(companyUser.User.Email))
            {
                Database.Execute("UPDATE Users SET Email = @email WHERE ID = @id", new { email = companyUser.User.Email + "(deleted)", id = companyUser.UserID });
            }

            return Request.CreateResponse(HttpStatusCode.OK, "1");
        }


        [AcceptVerbs("GET", "POST")]
        public HttpResponseMessage ChangeDeletedAccountUserEmailAddress(int companyid)
        {
            var users = Database.GetAll<CompanyUserModel>().Where(x => x.CompanyID == companyid).ToList();

          

            foreach(var user in users)
            {

               var _users = Database.GetSingle<UserModel>(user.UserID);

                _users.Email = user.UserID + "-@compendiumframework.com";

                _users.IsDeleted = true;
               
                Database.ExecuteUpdate(_users);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "1");

        }



        #endregion










       


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

    public class NewCompanyValues
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }

    }
    public class useremail
    {
        public int UserID { get; set; }
        public string Email { get; set; }
    }

    public class CompanyEmailConfig
    {

        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string TraniningOfficerName { get; set; }
    }
}
