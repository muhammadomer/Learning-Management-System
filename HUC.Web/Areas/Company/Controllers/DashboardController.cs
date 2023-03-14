using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HUC.Web.App.Courses;
using HUC.Web.App.PageModels;
using HUC.Web.App.Users;
using HUC.Web.Models;

namespace HUC.Web.Areas.Company.Controllers
{
    public class DashboardController : CompanyBaseController
    {
        private UsersService _users = new UsersService();

        public ActionResult Index()
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
          
            var model = new CompanyDashboardPageModel
            {
                Company = company,
                Year = 2012
            };

            return View(model);
        }

        public ActionResult Graphs(int? year = null)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;
           
            var model = new CompanyDashboardPageModel
            {
                Company = company,
                Year = year
            };

            var FreeTexReviewCourses = GetFreeTextReviewCourses(company.ID);

            if (FreeTexReviewCourses != null && FreeTexReviewCourses.Count > 0)
            {
                foreach (var item in model.Company.Courses.ToList())
                {
                    item.HasFreeTextCheck =  FreeTexReviewCourses.Where(x => x.CourseId == item.CourseID).Count() > 0 ? 1 : 0;
                }
            }

            //foreach (var item in model.Company.Courses.ToList())
            //{

            //    int hassfreetext = GetReviews(item.CourseID);
            //    item.HasFreeTextCheck = hassfreetext;
            //}


            return View(model);
        }
        private List<CountRecords> GetFreeTextReviewCourses(int companyID)
        {
            try
            {
                var company = _users.GetLoggedInUserModel().RepresentingCompany;
                ViewBag._Company = company;
                var courseList = Database.Query<CountRecords>(
                                    "select R.id, r.courseid,count(R.courseid) TotalCount" +
                                    " from AssignedCourses AC" +
                                    " inner join Resources R" +
                                    " on R.CourseID = AC.CourseID and AC.CompanyID = " + companyID +
                                    " inner join TestQuestions TQ" +
                                    " on R.Id = TQ.ResourceID and QuestionType = 7" +
                                    " left outer join TestQuestionAnswers TQA" +
                                    " on TQA.TestQuestionId = TQ.Id" +

                                    " where IsResult = 0 and CourseUserId!=0" +
                                    " group by R.id,r.courseid").ToList();

                return courseList;

                //var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id);

                //if (curUserCourse != null)
                //{
                //    if (!curUserCourse.IsComplete)
                //    {
                //        //Next resource being null means this is the last resource. Now we mark the course as complete
                //        Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
                //    }
                //}

            }
            catch (Exception ex) { LogApp.Log4Net.WriteException(ex); }
            return null;
        }

        private int GetReviews(int id)
        {

            int freetextbool = 0;

            try
            {

                var company = _users.GetLoggedInUserModel().RepresentingCompany;
                var course = CourseModel.GetIfInCompany(id, company.ID);

                var userList = Database.Query<string>("SELECT [UserID] FROM [AssignedCourses] WHERE CourseID = " + id).ToList();
                if (userList != null)
                {
                    ViewBag.UserList = userList;
                    ViewBag._Company = company;
                    var intUsers = userList.Select(int.Parse).ToArray();
                    var resources = course.Resources.ToList();
                    foreach (var item in resources)
                    {
                        foreach (var userid in intUsers)
                        {
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
                                        freetextbool = 1;


                                    }
                                }
                            }
                        }
                    }
                }

                var curUser = _users.GetLoggedInUserModel();


                var curUserCourse = curUser.UserCourses.FirstOrDefault(x => x.CourseID == id);

                //if (curUserCourse != null)
                //{
                //    if (!curUserCourse.IsComplete)
                //    {
                //        //Next resource being null means this is the last resource. Now we mark the course as complete
                //        Database.ExecuteUpdate("UserCourses", new[] { "IsComplete", "CompleteOn" }, new { ID = curUserCourse.ID, IsComplete = true, CompleteOn = DateTime.Now });
                //    }
                //}

            }
            catch (Exception ex) { LogApp.Log4Net.WriteException(ex); }
            return freetextbool;
        }


        public JsonResult PageData(int ? year, int next = 0)
        {
            var company = _users.GetLoggedInUserModel().RepresentingCompany;

            var courseTicks = new List<object[]>();         
            var targetCompliancePerCourseData = new List<int[]>();
            var userAverageScorePerCourseData = new List<decimal[]>();
            var userAverageTimeSpentPerCourseData = new List<decimal[]>();
            var userAverageScorePerCourseHasData = false;
            var userAverageTimeSpentPerCourseHasData = false;
            var courseCount = 1;  
            
            if (year == null)
            {
                foreach (var curCourse in company.Courses.OrderBy(w => w.Course.Name)/*.Skip(next * 10).Take(10)*/)
                {
                    var courseNameSmall = curCourse.Course.Name;
                    if (courseNameSmall.Length > 15)
                    {
                        courseNameSmall = courseNameSmall.Substring(0, 10) + "...";
                    }

                    //courseTicks.Add(new object[] { courseCount, courseNameSmall });

                    var userCourses = year.HasValue
                        ? curCourse.UserCourses.Where(x => x.StartedOn.HasValue).ToList()
                        : curCourse.UserCourses;

                    if (userCourses.Any(/*x => x.IsComplete*/ ))
                    {
                        courseTicks.Add(new object[] { courseCount, courseNameSmall });
                        userAverageScorePerCourseHasData = true;
                        int scoremin = (curCourse.ComplianceScoreMinimum * 100) / curCourse.Course.MaxScore;

                        targetCompliancePerCourseData.Add(new[] { courseCount, /*((curCourse.ComplianceScoreMinimum * 100) / curCourse.Course.MaxScore)*/scoremin });
                        userAverageScorePerCourseData.Add(new[] { courseCount, Math.Round(((curCourse.ComplianceScoreAverage * 100) / curCourse.Course.MaxScore)) });
                    }
                    if (userCourses.Any())
                    {
                        userAverageTimeSpentPerCourseHasData = true;
                        userAverageTimeSpentPerCourseData.Add(new[] { courseCount, Math.Round((decimal)curCourse.AverageTime.TotalMinutes) });
                        courseCount++;
                        //if (courseCount.Equals(11))
                        //{
                        //    break;
                        //}
                    }

                }
            }
            else
            {
                foreach (var curCourse in company.Courses.Where(w => w.Course.CreatedDate.Value.Year == year).OrderBy(w => w.Course.Name)/*.Skip(next * 10).Take(10)*/)
                {
                    var courseNameSmall = curCourse.Course.Name;
                    if (courseNameSmall.Length > 15)
                    {
                        courseNameSmall = courseNameSmall.Substring(0, 10) + "...";
                    }

                    //courseTicks.Add(new object[] { courseCount, courseNameSmall });

                    var userCourses = year.HasValue
                        ? curCourse.UserCourses.Where(x => x.StartedOn.HasValue && x.Course.CreatedDate.Value.Year == year).ToList()
                        : curCourse.UserCourses;

                    if (userCourses.Any(/*x => x.IsComplete*/ ))
                    {
                        courseTicks.Add(new object[] { courseCount, courseNameSmall });
                        userAverageScorePerCourseHasData = true;
                        int scoremin = (curCourse.ComplianceScoreMinimum * 100) / curCourse.Course.MaxScore;

                        targetCompliancePerCourseData.Add(new[] { courseCount, /*((curCourse.ComplianceScoreMinimum * 100) / curCourse.Course.MaxScore)*/scoremin });
                        userAverageScorePerCourseData.Add(new[] { courseCount, Math.Round(((curCourse.ComplianceScoreAverage * 100) / curCourse.Course.MaxScore)) });
                    }
                    if (userCourses.Any())
                    {
                        userAverageTimeSpentPerCourseHasData = true;
                        userAverageTimeSpentPerCourseData.Add(new[] { courseCount, Math.Round((decimal)curCourse.AverageTime.TotalMinutes) });
                        courseCount++;
                        //if (courseCount.Equals(11))
                        //{
                        //    break;
                        //}
                    }

                }

            }

            bool isdata = userAverageScorePerCourseData.Skip(next * 10).Take(10).Count() > 0 ? true : false;
            userAverageScorePerCourseHasData = isdata;
            userAverageTimeSpentPerCourseHasData = isdata;
            var userAverageScorePerCourse = new
            {
                barDataCompliance = targetCompliancePerCourseData.Skip(next * 10).Take(10).ToArray(),
                barDataAverage = userAverageScorePerCourseData.Skip(next * 10).Take(10).ToArray(),
                ticks = courseTicks.Skip(next * 10).Take(10),
                noData = !userAverageScorePerCourseHasData,
                existdata = isdata
            };
            isdata = userAverageTimeSpentPerCourseData.Count() > 0 ? true : false;
           
            var userAverageTimeSpentPerCourse = new
            {
                barData = userAverageTimeSpentPerCourseData.Skip(next * 10).Take(10).ToArray(),
                ticks = courseTicks.Skip(next * 10).Take(10),
                noData = !userAverageTimeSpentPerCourseHasData,
                existdata = isdata
            };


            return Json(new
            {
                UserAverageScorePerCourse = userAverageScorePerCourse,
                UserAverageTimeSpentPerCourse = userAverageTimeSpentPerCourse,
                TotalRecordScoreInGraph = userAverageScorePerCourseData.Count(),
                TotalRecordTimeInGraph = userAverageTimeSpentPerCourseData.Count()
            }, JsonRequestBehavior.AllowGet);
        }
      
    }


}