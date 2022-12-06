using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.Areas.Company.Controllers;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class CompaniesController : AdminBaseController
    {
        private const string NoteName = "Company";
        private UsersService _users = new UsersService();

        public ActionResult Index(int? parentID = null)
        {
            ViewBag.ParentCompany = Database.GetSingle<CompanyModel>(parentID);

            IEnumerable<CompanyModel> model;
            if (parentID == null)
            {
                model = Database.GetAll<CompanyModel>("WHERE ParentCompanyID IS NULL");
            }
            else
            {
                model = Database.GetAll<CompanyModel>("WHERE ParentCompanyID = @ParentID", new { ParentID = parentID });
            }

            return View(model);
        }

        public ActionResult Create(int? id)
        {
            var model = new CompanyAddModel
            {
                ParentCompanyID = id
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CompanyAddModel model)
        {
            //User Specifics Creation
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
            //--

            if (!String.IsNullOrWhiteSpace(model.UserAdd.Password) && !String.IsNullOrWhiteSpace(model.UserAdd.ConfirmPassword) && model.UserAdd.Password != model.UserAdd.ConfirmPassword)
            {
                ModelState.AddModelError("UserAdd.ConfirmPassword", "The 'Password' field and 'Confirm Password' field must match.");
            }

            if (model.ParentCompanyID.HasValue)
            {
                model.IsDemonstration = model.ParentCompany.IsDemonstration;
            }

            if (ModelState.IsValid)
            {
                var companyID = Database.ExecuteInsert(model, true);
                UpdateEnumerables(companyID, model.CourseIDs);

                model.UserAdd.RoleIDs = new List<int>();
                model.UserAdd.IsActive = true;
                var userID = _users.Create(model.UserAdd);
                Database.ExecuteInsert(new CompanyUserAddModel
                {
                    CompanyID = companyID,
                    UserID = userID,
                    IsAdmin = true
                });

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index", new { parentID = model.ParentCompanyID });
            }

            AddErrorModel();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = Database.GetSingle<CompanyEditModel>(id);
            model.CourseIDs = model.Courses.Select(x => x.CourseID);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CompanyEditModel model)
        {
            if (ModelState.IsValid)
            {
                Database.ExecuteUpdate(model);
                UpdateEnumerables(model.ID, model.CourseIDs);

                if (model.IsDemonstration)
                {
                    Database.Execute("UPDATE Users SET IsActive = 1 WHERE ID IN (SELECT UserID FROM CompanyUsers WHERE CompanyID = @ID)", new { ID = model.ID });

                    Database.Execute("UPDATE Companies SET IsDemonstration = 1 WHERE ID IN (" + String.Join(", ", model.DescendantIDsIncludingSelf) + ")");
                }
                else
                {
                    Database.Execute("UPDATE Companies SET IsDemonstration = 0 WHERE ID IN (" + String.Join(", ", model.DescendantIDsIncludingSelf) + ")");
                }

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index", new { parentID = model.ParentCompanyID });
            }

            AddErrorModel();
            return View(model);
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

            if (!String.IsNullOrWhiteSpace(sql))
            {
                Database.Execute(sql, sqlParams);
            }
        }
    }
}
