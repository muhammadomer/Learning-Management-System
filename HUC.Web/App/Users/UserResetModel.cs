using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users
{
    public class UserResetModel : BaseModel
    {
        [Required, StringLength(500), DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}