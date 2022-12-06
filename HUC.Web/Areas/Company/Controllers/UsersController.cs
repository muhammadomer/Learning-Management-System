using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Courses;
using HUC.Web.App.PageModels;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using HUC.Web.Controllers;
using HUC.Web.Models.SinglePoint;

namespace HUC.Web.Areas.Company.Controllers
{
    public class UsersController : CompanyBaseController
    {
        private const string NoteName = "User";
        private UsersService _users = new UsersService();
        public ActionResult Index()
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            company.AllUsers.GroupBy(x => x.UserID).Select(x => x.First());
            return View(company) ;
        }

        public ActionResult Create()
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            if (company.UsersAvailable <= 0)
            {
                AddError("You have reached your maximum users. Please contact us if you would like to increase your limit.");
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(CompanyUserAddModel model)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            if (company.UsersAvailable <= 0)
            {
                AddError("You have reached your maximum users. Please contact us if you would like to increase your limit.");
                return RedirectToAction("Index");
            }

            if (!String.IsNullOrWhiteSpace(model.UserAdd.Email))
            {
                if (!model.UserAdd.Email.IsValidEmail())
                {
                    ModelState.AddModelError("UserAdd.Email", "The email address provided is in an incorrect format.");
                }
                else
                {
                    var prevUser = Database.GetSingle<UserModel>(model.UserAdd.Email, "Email");
                    if (prevUser != null)
                    {
                        ModelState.AddModelError("UserAdd.Email", "The email address provided is already in use");
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(model.UserAdd.Password) && !String.IsNullOrWhiteSpace(model.UserAdd.ConfirmPassword) && model.UserAdd.Password != model.UserAdd.ConfirmPassword)
            {
                ModelState.AddModelError("UserAdd.ConfirmPassword", "The 'Password' field and 'Confirm Password' field must match.");
            }

            if (ModelState.IsValid)
            {
                model.UserAdd.RoleIDs = new List<int>();
                model.UserAdd.ActivateKey = Guid.NewGuid().ToString();
                model.UserAdd.ResetPasswordKey = null;
                model.UserAdd.IsActive = company.IsDemonstration;
                var newID = _users.Create(model.UserAdd);
                if (!company.IsDemonstration)
                {
                    new MailController().Activation(model.UserAdd).Deliver();
                }

                model.CompanyID = company.ID;
                model.UserID = newID;
                Database.ExecuteInsert(model);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            if (!company.AllDescendantUsersIncludingSelf.Any(x => x.ID == id))
            {
                AddError("No user exists");
                return RedirectToAction("Index");
            }

            var model = Database.GetSingle<CompanyUserEditModel>(id);
            model.UserEdit = Database.GetSingle<UserEditModel>(model.UserID);
            model.UserEdit.Password = null;
            model.UserEdit.Salt = null;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CompanyUserEditModel model)
        {
            var checkItem = Database.GetSingle<CompanyUserModel>(model.ID);
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            if (!company.AllDescendantUsersIncludingSelf.Any(x => x.ID == checkItem.ID))
            {
                AddError("No user exists");
                return RedirectToAction("Index");
            }

            if (!String.IsNullOrWhiteSpace(model.UserEdit.Email))
            {
                if (!model.UserEdit.Email.IsValidEmail())
                {
                    ModelState.AddModelError("UserEdit.Email", "The email address provided is in an incorrect format.");
                }
                else
                {
                    var prevUser = Database.GetSingle<UserModel>(model.UserEdit.Email, "Email");
                    if (prevUser != null && prevUser.ID != checkItem.UserID)
                    {
                        ModelState.AddModelError("UserEdit.Email", "The email address provided is already in use");
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(model.UserEdit.Password) && !String.IsNullOrWhiteSpace(model.UserEdit.ConfirmPassword) && model.UserEdit.Password != model.UserEdit.ConfirmPassword)
            {
                ModelState.AddModelError("UserEdit.ConfirmPassword", "The 'Password' field and 'Confirm Password' field must match.");
            }

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
                _users.Edit(model.UserEdit);

                model.IsAdmin = checkItem.IsAdmin;
                Database.ExecuteUpdate(model);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index");
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                AddError("No user exists");
                return RedirectToAction("Index");
            }
            if (companyUser.IsAdmin)
            {
                AddError("You cannot delete the company admin");
                return RedirectToAction("Index");
            }

            if (undo)
            {
                //Check that undoing this action will not set the users over the limit
                if (company.UsersAvailable <= 0)
                {
                    AddError("You have reached your maximum users. Undoing this deletion will put you over your limit. Please contact us if you would like to increase your limit.");
                }
                else
                {
                    Database.SoftDelete("CompanyUsers", companyUser.ID, true);
                    Database.SoftDelete("Users", companyUser.UserID, true);

                    if (!string.IsNullOrWhiteSpace(companyUser.User.Email))
                    {
                        var email = companyUser.User.Email.Replace("(deleted)", "");
                        Database.Execute("UPDATE Users SET Email = @email WHERE ID = @id", new { email = email, id = companyUser.UserID });
                    }

                    AddDeleteUndone(NoteName);
                }
            }
            else
            {
                Database.SoftDelete("CompanyUsers", companyUser.ID);
                Database.SoftDelete("Users", companyUser.UserID);

                //Set Email has deleted
                if (!string.IsNullOrWhiteSpace(companyUser.User.Email))
                {
                    Database.Execute("UPDATE Users SET Email = @email WHERE ID = @id", new { email = companyUser.User.Email + "(deleted)", id = companyUser.UserID });
                }
                
                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }

        public ActionResult Statistics(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            //employeesEntities.Database.Connection.ConnectionString = employeesEntities.Database.Connection.ConnectionString.Replace("DentonsEmployeesForRiskManager", "HUC");
            ViewBag.CourseList = _users.GetAssignedCourses(companyUser.UserID);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                AddError("No user exists");
                return RedirectToAction("Index");
            }
            return View(companyUser);
        }

        public JsonResult StatisticsData(int id,int next = 0)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                return null;
            }

            var courseTicks = new List<object[]>();
            var timePerCourseData = new List<decimal[]>();
            var scorePerCourseData = new List<int[]>();
            var timePerCourseHasData = false;
            var scorePerCourseHasData = false;
            var courseCount = 1;
            foreach (var curCourse in companyUser.User.AllCourses().Skip(next*10).Take(10))
            {
                var userCourse = companyUser.User.UserCourses.FirstOrDefault(x => x.CourseID == curCourse.ID);

                var courseNameSmall = curCourse.Name;
                if (courseNameSmall.Length > 15)
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

                    timePerCourseData.Add(new[] { courseCount, Math.Round((decimal)userCourse.TimeTaken.TotalMinutes) });
                    scorePerCourseData.Add(new[] { courseCount, ((userCourse.TotalScore * 100) / curCourse.MaxScore) });
                }
                courseCount++;
            }
         bool   isdata = timePerCourseData.Count() > 0 ? true : false;
            var timePerCourse = new
            {
                barData = timePerCourseData.ToArray(),
                ticks = courseTicks,
                noData = !timePerCourseHasData,
                existdata = isdata
            };

            var scorePerCourse = new
            {
                barData = scorePerCourseData.ToArray(),
                ticks = courseTicks,
                noData = !scorePerCourseHasData,
                existdata = isdata
            };


            return Json(new
            {
                TimePerCourse = timePerCourse,
                ScorePerCourse = scorePerCourse
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Results(int id, int courseID)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;

            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                AddError("No user exists");
                return RedirectToAction("Index");
            }
            
            var userCourse = Database.GetAll<UserCourseModel>("WHERE UserID = @UserID AND CourseID = @CourseID", new { UserID = companyUser.UserID, CourseID = courseID}).FirstOrDefault();
            if (userCourse == null)
            {
                AddInfo("User hasn't started this course yet.");
                return RedirectToAction("Statistics", new { id = id });
            }

            var model = new UserTestResultsPageModel
            {
                User = companyUser.User,
                UserCourse = userCourse
            };

            return View(model);
        }

        public ActionResult SetBackup(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                //Selected ID is not a valid user

                AddError("Invalid user selected");
                return RedirectToAction("Index");
            }


            Database.Execute("UPDATE CompanyUsers SET IsBackupAdmin = 1 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index");
        }

        public ActionResult RevokeBackup(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                //Selected ID is not a valid user

                AddError("Invalid user selected");
                return RedirectToAction("Index");
            }
            if (company.UsersAvailable <= 0)
            {
                //Action will cause user limit to be breached

                AddError("Revoking this user's backup admin status will send you over your user limit.");
                return RedirectToAction("Index");
            }

            Database.Execute(
                "UPDATE CompanyUsers SET IsBackupAdmin = 0 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index");
        }

        public ActionResult SetCourseUsable(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                //Selected ID is not a valid user

                AddError("Invalid user selected");
                return RedirectToAction("Index");
            }
            if (company.UsersAvailable <= 0)
            {
                //Action will cause user limit to be breached

                AddError("Revoking this user's backup admin status will send you over your user limit.");
                return RedirectToAction("Index");
            }


            Database.Execute("UPDATE CompanyUsers SET IsBackupAdminCourseUsable = 1 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index");
        }

        public ActionResult RevokeCourseUsable(int id)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            if (companyUser == null || !company.AllDescendantUsersIncludingSelf.Any(x => x.ID == companyUser.ID))
            {
                //Selected ID is not a valid user

                AddError("Invalid user selected");
                return RedirectToAction("Index");
            }


            Database.Execute("UPDATE CompanyUsers SET IsBackupAdminCourseUsable = 0 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index");
        }
    }
}