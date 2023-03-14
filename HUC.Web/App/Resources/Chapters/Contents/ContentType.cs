using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.Shared;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HUC.Web.App.Resources.Chapters.Contents
{
    public enum ContentType
    {
        //These values are in the hundreds so you can add multiple of the same type whilst keeping it in an easy-to-read numerical value in the database.
        //Example - 100's = text (101,102,103 etc will all be text input and output, however the method of input/output may differ.

        [StringValue("Document")]
        // [Display(Order = 0)]
        [Order(0)]
        PDF = 700,

        [StringValue("Type Text")]
        // [Display(Order = 1)]
        [Order(1)]

        Text = 100,

        [StringValue("Video (YouTube)")]
        // [Display(Order = 2)]
        [Order(2)]

        VideoYoutube = 300,
        [StringValue("Video (Vimeo)")]
        // [Display(Order = 3)]
        [Order(3)]

        VideoVimeo = 301,
        [StringValue("Video (Wistia)")]
        // [Display(Order = 4)]
        [Order(4)]

        VideoWistia = 302,
        [StringValue("Audio (MP3)")]
        // [Display(Order = 5)]
        [Order(5)]

        Audio = 200,

        // [Display(Order = 6)]
        
        [StringValue("Flip Card")]
        // [Display(Order = 7)]
        [Order(6)]

        Flip = 500,
        [StringValue("2-way interview")]
        // [Display(Order = 8)]
        [Order(7)]

        Interview = 600,
        [Order(8)]

        Accordion = 400,


        //Radio = 700,
        //Slider = 800
    }

    public class OrderAttribute : Attribute
    {
        public int priority;

        public OrderAttribute(int priority)
        {
            this.priority = priority;
        }
    }

    public class EnumContentType{
    
       public IEnumerable<ContentType> getByOrder()
        {

            Dictionary<string, int> priorityTable = new Dictionary<string, int>();

            var values = Enum.GetValues(typeof(ContentType)).Cast<ContentType>();
            MemberInfo[] members = typeof(ContentType).GetMembers();
            foreach (MemberInfo member in members)
            {
                object[] attrs = member.GetCustomAttributes(typeof(OrderAttribute), false);
                foreach (object attr in attrs)
                {
                    OrderAttribute orderAttr = attr as OrderAttribute;

                    if (orderAttr != null)
                    {
                        string propName = member.Name;
                        int priority = orderAttr.priority;

                        priorityTable.Add(propName, priority);
                    }
                }
            }

            values = values.OrderBy(n => priorityTable[n.ToString("G")]);

            return values;
               
        }
        

    }

}