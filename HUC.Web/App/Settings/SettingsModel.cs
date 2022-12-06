using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.Shared;
using System.ComponentModel.DataAnnotations;
using AtlasDB;


namespace HUC.Web.App.Settings
{
    public class SettingsModel:BaseModel
    {
        // [Required(ErrorMessage = "Tranining Officer Name is required.")]

      //  public int Id { get; set; }

        public int CompanyId { get; set; }
        [Required, StringLength(100), Display(Name = "Training Officer Name")]

        // [Required(ErrorMessage = "Tranining Officer Name is required.")]
        public string TraniningOfficerName { get; set; }
        [Required, StringLength(100), Display(Name = "Training Officer Email")]
        //  [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email Address not correct")]
        // [EmailAddress]

        //[Required(ErrorMessage = "Email is required.")]
        //[RegularExpression(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Invalid Email Address.")]
        public string TrainingOfficerEmail { get; set; }
        [Display(Name = "Course lead time")]

        // [Required(ErrorMessage = "Tranining Course Weeks is required.")]
        //  [MaxLength(2)]
       // [Range(1, 8, ErrorMessage = "Age must be between 25 and 60")]

        
        public int TrainingCoursesWeeks { get; set; }

        [Required(ErrorMessage = "Days Reminder 1 is required.")]

        [Display(Name = "1st reminder after lead time")]
        public int DaysReminder1 { get; set; }

        [Required(ErrorMessage = "Days Reminder 2 is required.")]
        [Display(Name = "2nd reminder after first")]
        public int DaysReminder2 { get; set; }
        [Required(ErrorMessage = "Days Reminder 3 is required.")]
        [Display(Name = "Final reminder after second")]
        public int DaysReminder3 { get; set; }

        [Display(Name = "Reminders")]
        public bool EmailReminder { get; set; }
        [Display(Name = "Course completion")]
        public bool EmailCourseComplete { get; set; }
        [Display(Name = "Course assignment")]
        public bool EmailAssignCourse { get; set; }
        [Display(Name = "Compliance course expiry")]
        public bool EmailCompliance { get; set; }


        private IEnumerable<EmailConfigurationModel> _emailConfiguration;
        [DBIgnore]
        public IEnumerable<EmailConfigurationModel> EmailConfiguration
        {
            get
            {
                if (_emailConfiguration == null)
                {
                    _emailConfiguration = Database.GetAll<EmailConfigurationModel>("where companyid=@companyid", new {companyId=CompanyId});
                }
                return _emailConfiguration;
            }
            set { _emailConfiguration = value; }
        }


    }
}