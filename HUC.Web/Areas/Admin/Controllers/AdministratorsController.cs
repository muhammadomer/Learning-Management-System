using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class AdministratorsController : AdminBaseController
    {
        private UsersService _users = new UsersService();

        public ActionResult Index()
        {
            var model = Database.GetAll<UserModel>("WHERE IsDeleted = 0").Where(x => x.HasRole("admin"));

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserAddModel model)
        {
            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                if (!model.Email.IsValidEmail())
                {
                    ModelState.AddModelError("Email", "The email address provided is in an incorrect format.");
                }
                else
                {
                    var prevUser = Database.GetSingle<UserModel>(model.Email, "Email");
                    if (prevUser != null)
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
                model.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                model.IsActive = true;
                _users.Create(model);

                AddSuccessCreate("Administrator");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<UserEditModel>(id);
            model.Password = null;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(UserEditModel model)
        {
            var checkModel = Database.GetSingle<UserModel>(model.ID);

            if (!String.IsNullOrWhiteSpace(model.Email))
            {
                if (!model.Email.IsValidEmail())
                {
                    ModelState.AddModelError("Email", "The email address provided is in an incorrect format.");
                }
                else
                {
                    var prevUser = Database.GetSingle<UserModel>(model.Email, "Email");
                    if (prevUser != null && prevUser.ID != model.ID)
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
                model.ResetPasswordKey = checkModel.ResetPasswordKey;
                model.ActivateKey = checkModel.ActivateKey;
                model.IsDeleted = checkModel.IsDeleted;
                model.IsActive = checkModel.IsActive;
                model.RoleIDs = new[] { Database.GetSingle<RoleModel>("Admin", "Name").ID };
                _users.Edit(model);

                AddSuccessEdit("Administrator");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Delete(int id, bool undo = false)
        {
            Database.SoftDelete("Users", id, undo);

            if (undo)
            {
                AddDeleteUndone("Administrator");
            }
            else
            {
                AddDeleted("Administrator", Url.Action("Delete", new { id = id, undo = true }));
            }

            return RedirectToAction("Index");
        }
    }
}