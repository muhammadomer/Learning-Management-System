using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HUC.Web.App.Shared;

namespace HUC.Web.App.Resources.Chapters.Contents
{
    public enum ContentType
    {
        //These values are in the hundreds so you can add multiple of the same type whilst keeping it in an easy-to-read numerical value in the database.
        //Example - 100's = text (101,102,103 etc will all be text input and output, however the method of input/output may differ.
        Text = 100,

        Audio = 200,

        [StringValue("Video (YouTube)")]
        VideoYoutube = 300,
        [StringValue("Video (Vimeo)")]
        VideoVimeo = 301,
        [StringValue("Video (Wistia)")]
        VideoWistia = 302,
        Accordion = 400,
        Flip = 500,
        Interview = 600,
        [StringValue("Document")]
        PDF = 700,
        //Radio = 700,
        //Slider = 800
    }
}