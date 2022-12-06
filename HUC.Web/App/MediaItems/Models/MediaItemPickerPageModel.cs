using System.Collections.Generic;
using System.Linq;
using HUC.Web.App.Shared;

namespace HUC.Web.App.MediaItems.Models
{
    public class MediaItemPickerPageModel : BaseModel
    {
        //Data filters
        public string TargetElement { get; set; }
        public List<FileType> FileTypes { get; set; }
        public bool IsMultiple { get; set; }
        public bool IsRemovable { get; set; }
        public int? UploadedID { get; set; }

        //Return List
        public IEnumerable<MediaItemModel> Items
        {
            get
            {
                var allMediaItems = Database.GetAll<MediaItemModel>("WHERE IsDeleted = 0");

                if (FileTypes == null || !FileTypes.Any() ||FileTypes.Contains(FileType.Any))
                {
                    return allMediaItems;
                }

                return allMediaItems.Where(x => FileTypes.Contains(x.Type));
            }
        }

        //Add Model
        public MediaItemAddModel Add { get; set; }
    }
}