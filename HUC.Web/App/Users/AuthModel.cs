using System;
using System.Collections.Generic;
using System.Linq;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Users
{
    public class AuthModel : BaseModel
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }

        public AuthModel(int id, string email, IList<string> roles)
        {
            ID = id;
            Email = email;
            Roles = roles;
        }

        public AuthModel(string data)
        {
            var items = data.Split(Convert.ToChar("\x01"));
            ID = Convert.ToInt32(items[0]);
            Email = items[1];
            Roles = items[2].Split(';');
        }

        public override string ToString()
        {
            var rolesStr = string.Join(";", Roles.ToArray());

            if (String.IsNullOrWhiteSpace(rolesStr))
            {
                rolesStr = "";
            }

            return string.Format("{0}\x01{1}\x01{2}", ID, Email, rolesStr);
        }
    }
}