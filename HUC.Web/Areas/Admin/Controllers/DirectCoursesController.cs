using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HUC.Web.App.Companies;
using HUC.Web.App.Companies.Users;
using HUC.Web.App.Companies.Users.Courses;
using HUC.Web.App.PageModels;
using HUC.Web.App.Shared;
using HUC.Web.App.Users;
using HUC.Web.App.Users.Courses;
using HUC.Web.Controllers;

namespace HUC.Web.Areas.Admin.Controllers
{
    public class DirectCoursesController : AdminBaseController
    {
        private const string NoteName = "Direct Course";
        private UsersService _users = new UsersService();

        public ActionResult Index(int id)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(id);

            return View(companyUser);
        }

        public ActionResult Create(int companyUserID)
        {
            var model = new DirectCourseAddModel
            {
                CompanyUserID = companyUserID
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DirectCourseAddModel model)
        {
            var companyUser = Database.GetSingle<CompanyUserModel>(model.CompanyUserID);

            if (companyUser.Company.Courses.Any(x => x.CourseID == model.CourseID))
            {
                ModelState.AddModelError("CourseID", "This course is already attached to the company and cannot be directly attached to the user.");
            }
            else
            {
                if (companyUser.User.UserCourses.Any(x => x.CourseID == model.CourseID))
                {
                    ModelState.AddModelError("CourseID", "This course is already directly attached to the user.");
                }
            }


            if (ModelState.IsValid)
            {
                var newUserCourse = new UserCourseModel
                {
                    UserID = companyUser.UserID,
                    CourseID = model.CourseID,
                    ComplianceScoreMinimum = model.ComplianceScoreMinimum,
                    IsComplete = false
                };

                Database.ExecuteInsert(newUserCourse);

                AddSuccessCreate(NoteName);
                return RedirectToAction("Index", new { id = model.CompanyUserID });
            }

            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var userCourse = Database.GetSingle<UserCourseModel>(id);

            if (userCourse.StartedOn.HasValue)
            {
                AddError("You cannot remove a course which has been started.");
                return RedirectToAction("Index", new { id = userCourse.CompanyUser.ID });
            }

            Database.HardDelete("UserCourses", userCourse.ID);

            return RedirectToAction("Index", new { id = userCourse.CompanyUser.ID });
        }
    }
}