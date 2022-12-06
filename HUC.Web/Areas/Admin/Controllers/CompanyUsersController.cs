using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.PageModels;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.Controllers;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class CompanyUsersController : AdminBaseController
    {
        private const string NoteName = "User";
        private UsersService _users = new UsersService();

        public ActionResult Index(int id)
        {
            var company = Database.GetSingle<CompanyModel>(id);

            return View(company);
        }

        public ActionResult Create(int id)
        {
            var company = Database.GetSingle<CompanyModel>(id);
            if (company.UsersAvailable <= 0)
            {
                AddError("Company has reached maximum users.");
                return RedirectToAction("Index", new { id = company.ID });
            }

            var model = new CompanyUserAddModel
            {
                CompanyID = company.ID
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CompanyUserAddModel model)
        {
            var company = Database.GetSingle<CompanyModel>(model.CompanyID);
            if (company.UsersAvailable <= 0)
            {
                AddError("Company has reached maximum users.");
                return RedirectToAction("Index", new { id = company.ID });
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
                if (!company.IsDemonstration && company.IsInitialVerificationEnabled)
                {
                    new MailController().Activation(model.UserAdd).Deliver();
                }

                model.CompanyID = company.ID;
                model.UserID = newID;
                Database.ExecuteInsert(model);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index", new { id = company.ID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<CompanyUserEditModel>(id);

            var company = model.Company;
            if (!company.AllUsers.Any(x => x.ID == id))
            {
                AddError("No user exists");
                return RedirectToAction("Index", new { id = company.ID });
            }

            model.UserEdit = Database.GetSingle<UserEditModel>(model.UserID);
            model.UserEdit.Password = null;
            model.UserEdit.Salt = null;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CompanyUserEditModel model)
        {
            var checkItem = Database.GetSingle<CompanyUserModel>(model.ID);
            var company = model.Company;
            if (!company.AllUsers.Any(x => x.ID == checkItem.ID))
            {
                AddError("No user exists");
                return RedirectToAction("Index", new { id = company.ID });
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
                return RedirectToAction("Index", new { id = company.ID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;
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
                    AddError("Company has reached maximum users. Undoing this deletion will put you over your limit. Please contact us if you would like to increase your limit.");
                }
                else
                {
                    Database.SoftDelete("CompanyUsers", companyUser.ID, true);
                    Database.SoftDelete("Users", companyUser.UserID, true);

                    AddDeleteUndone(NoteName);
                }
            }
            else
            {
                Database.SoftDelete("CompanyUsers", companyUser.ID);
                Database.SoftDelete("Users", companyUser.UserID);

                AddDeleted(NoteName, Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult SetBackup(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;

            Database.Execute("UPDATE CompanyUsers SET IsBackupAdmin = 1 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult RevokeBackup(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;

            if (company.UsersAvailable <= 0)
            {
                //Action will cause user limit to be breached

                AddError("Revoking this user's backup admin status will send you over your user limit.");
                return RedirectToAction("Index", new { id = company.ID });
            }

            Database.Execute(
                "UPDATE CompanyUsers SET IsBackupAdmin = 0 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult SetCourseUsable(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;

            if (company.UsersAvailable <= 0)
            {
                //Action will cause user limit to be breached

                AddError("Revoking this user's backup admin status will send you over your user limit.");
                return RedirectToAction("Index", new { id = company.ID });
            }


            Database.Execute("UPDATE CompanyUsers SET IsBackupAdminCourseUsable = 1 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult RevokeCourseUsable(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;

            Database.Execute("UPDATE CompanyUsers SET IsBackupAdminCourseUsable = 0 WHERE ID = @UserID;",
                new
                {
                    UserID = id
                });

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult Activate(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);
            var company = companyUser.Company;

            Database.Execute(
                "UPDATE Users SET IsActive = 1 WHERE ID = @UserID;",
                new
                {
                    UserID = companyUser.UserID
                });

            return RedirectToAction("Index", new { id = company.ID });
        }

        public ActionResult Move(int id)
        {
            var model = new CompanyUserMoveModel
            {
                ID = id
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Move(CompanyUserMoveModel model)
        {
            var validOptions = model.CompanyOptions().Select(x => x.Value);
            if (!validOptions.Contains(model.CompanyID.ToString()))
            {
                ModelState.AddModelError("CompanyID", "You must select a company from the list.");
            }

            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);

                AddSuccess("This user has now been moved.");
                return RedirectToAction("Index", new { id = model.CompanyUser.CompanyID });
            }

            return View(model);
        }

        public ActionResult SendActivationEmails(int id)
        {
            var inactiveUsers = Database.GetAll<UserModel>(
                "WHERE IsActive = 0 AND IsDeleted = 0 AND ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @CompanyID)",
                new {
                    CompanyID = id
                }
            );

            if (inactiveUsers.Any())
            {
                foreach (var curInactiveUser in inactiveUsers)
                {
                    new MailController().Activation(curInactiveUser).Deliver();
                }

                AddInfo(inactiveUsers.Count() + " activation emails sent");
            }
            else
            {
                AddInfo("All users are already activated!");
            }


            return RedirectToAction("Index", new { id = id });
        }

        //public ActionResult TestBulkMail(int id, int qty)
        //{
        //    var user = Database.GetSingle<UserModel>(id);

        //    for (int i = 0; i < qty; i++)
        //    {
        //        new MailController().Activation(user).Deliver();
        //    }

        //    return null;
        //}
    }
}