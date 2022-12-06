using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AtlasDB;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Companies.Users
{
    [DBTableName("CompanyUsers")]
    public class CompanyUserMoveModel : BaseModel
    {
        public int CompanyID { get; set; }

        public List<SelectListItem> CompanyOptions()
        {
            var sli = new List<SelectListItem>();
            sli.Add(new SelectListItem());

            var options = CompanyUser.Company.TopCompany.DescendantCompaniesIncSelf;
            foreach (var curOption in options.Where(x => x.ID != CompanyUser.CompanyID))
            {
                sli.Add(new SelectListItem
                {
                    Text = curOption.Name,
                    Value = curOption.ID.ToString()
                });
            }

            return sli;
        }

        private CompanyUserModel _companyUser;
        [DBIgnore]
        public CompanyUserModel CompanyUser
        {
            get
            {
                if (_companyUser == null)
                {
                    _companyUser = Database.GetSingle<CompanyUserModel>(ID);
                }
                return _companyUser;
            }
            set { _companyUser = value; }
        }
    }
}