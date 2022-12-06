using System.Web.Optimization;

namespace HUC.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/_Content/js/lib/jquery-1.8.2.js",
                "~/_Content/js/lib/jquery-ui-1.9.2.js",
                "~/_Content/js/lib/jquery.unobtrusive-ajax.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/signalR").Include(
              "~/_Content/js/lib/jquery.signalR-2.2.1.js"
              ));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/_Content/js/lib/modernizr-2.6.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/_Content/js/lib/jquery.placeholder.js",
                "~/_Content/js/lib/jquery.anyslider.js",
                "~/_Content/js/lib/jquery.colorbox.js",
                "~/_Content/js/main.js"));

            bundles.Add(new LessBundle("~/bundles/less").Include(
                "~/_Content/css/reset.css",
                "~/_Content/css/site.css",
                "~/_Content/css/width-1300.css",
                "~/_Content/css/width-1250.css",
                "~/_Content/css/width-1150.css",
                "~/_Content/css/width-1050.css",
                "~/_Content/css/width-640.css",
                "~/_Content/css/width-480.css",
                "~/_Content/css/width-400.css"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/_Content/css/lib/colorbox.css"));


            //SCALE BUNDLES
            bundles.Add(new ScriptBundle("~/bundles/scale/js").Include(
                "~/_Content/Scale/js/charts/easypiechart/jquery.ui.touch-punch.min.js",
                "~/_Content/Scale/js/charts/easypiechart/jquery.easy-pie-chart.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.grow.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.pie.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.resize.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.spline.js",
                "~/_Content/Scale/js/charts/flot/jquery.flot.tooltip.js",
                "~/_Content/js/lib/jquery.flot.time.js",
                "~/_Content/Scale/js/charts/flot/demo.js",
                "~/_Content/Scale/js/charts/sparkline/jquery.sparkline.min.js",
                "~/_Content/Scale/js/charts/sparkline/jquery.sparkline.min.js",
                "~/_Content/Scale/js/chosen/chosen.jquery.min.js",
                "~/_Content/Scale/js/datatables/jquery.csv-0.71.min.js",
                "~/_Content/Scale/js/datatables/jquery.dataTables.min.js",
                "~/_Content/Scale/js/calendar/bootstrap_calendar.js",
                "~/_Content/Scale/js/datepicker/bootstrap-datepicker.js",
                "~/_Content/Scale/js/file-input/bootstrap-filestyle.min.js",
                "~/_Content/Scale/js/file-input/bootstrap.file-input.js",
                "~/_Content/Scale/js/fullcalendar/fullcalendar.min.js",
                "~/_Content/Scale/js/intro/intro.min.js",
                "~/_Content/Scale/js/jvectormap/jquery-jvectormap-1.2.2.min.js",
                "~/_Content/Scale/js/jvectormap/jquery-jvectormap-us-aea-en.js",
                "~/_Content/Scale/js/jvectormap/jquery-jvectormap-world-mill-en.js",
                //"~/_Content/Scale/js/maps/gmaps.js",
                "~/_Content/Scale/js/markdown/epiceditor.min.js",
                "~/_Content/Scale/js/nestable/jquery.nestable.js",
                "~/_Content/Scale/js/parsley/parsley.extend.js",
                "~/_Content/Scale/js/parsley/parsley.min.js",
                "~/_Content/Scale/js/slider/bootstrap-slider.js",
                "~/_Content/Scale/js/slimscroll/jquery.slimscroll.min.js",
                "~/_Content/Scale/js/sortable/jquery.sortable.js",
                "~/_Content/Scale/js/wizard/jquery.bootstrap.wizard.js",
                "~/_Content/Scale/js/wysiwyg/bootstrap-wysiwyg.js",
                "~/_Content/Scale/js/wysiwyg/jquery.hotkeys.js",
                "~/_Content/Scale/js/bootstrap.js",
                "~/_Content/Scale/js/app.plugin.js",
                "~/_Content/Scale/js/app.js",
                "~/_Content/js/lib/jquery.pnotify.js",
                "~/_Content/js/lib/redactor.js",
                "~/_Content/js/lib/video.js",
                "~/_Content/js/lib/jquery.mask.js",
                "~/_Content/js/lib/jquery.colorbox.js",
                "~/_Content/Scale/js/scale.js",
                "~/_Content/js/scale.js",
                "~/_Content/js/toastr.min.js",
                "~/_Content/js/AjaxCommonMethods.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/scale/css").Include(
                "~/_Content/Scale/js/chosen/chosen.css",
                "~/_Content/Scale/css/bootstrap.css",
                "~/_Content/Scale/css/bootstrap_calendar.css",
                "~/_Content/Scale/css/font.css",
                "~/_Content/Scale/css/font-awesome.css",
                "~/_Content/Scale/css/animate.css",
                "~/_Content/Scale/css/icon.css",
                "~/_Content/Scale/css/app.css",
                "~/_Content/Scale/css/custom.css",
                "~/_Content/css/lib/jquery.pnotify.default.css",
                "~/_Content/css/lib/redactor.css",
                "~/_Content/css/lib/colorbox.css",
                "~/_Content/css/toastr.min.css",
                "~/_Content/Scale/css/scale.less"
                ));

            bundles.Add(new StyleBundle("~/bundles/scale/k6css").Include(
                "~/_Content/Scale/js/chosen/chosen.css",
                "~/_Content/Scale/k6css/bootstrap.css",
                "~/_Content/Scale/k6css/bootstrap_calendar.css",
                "~/_Content/Scale/k6css/font.css",
                "~/_Content/Scale/k6css/font-awesome.css",
                "~/_Content/Scale/k6css/animate.css",
                "~/_Content/Scale/k6css/icon.css",
                "~/_Content/Scale/k6css/app.css",
                "~/_Content/Scale/k6css/customk6.css",
                "~/_Content/css/lib/jquery.pnotify.default.css",
                "~/_Content/css/lib/redactor.css",
                "~/_Content/css/lib/colorbox.css",
                "~/_Content/css/toastr.min.css",
                "~/_Content/Scale/k6css/scale.less"
                ));


            //COMPANY BUNDLES
            bundles.Add(new ScriptBundle("~/bundles/company/js").Include(
                "~/_Content/js/company.js",
                "~/_Content/js/lib/audiojs/audio.js",
                "~/_Content/js/admin.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/company/css").Include(
                "~/_Content/css/company.css"
                ));

            //USERS BUNDLES
            bundles.Add(new ScriptBundle("~/bundles/user/js").Include(
                "~/_Content/js/lib/audiojs/audio.js",
                "~/_Content/js/lib/countdown.js",
                "~/_Content/js/user.js"
                ));

            //ADMIN BUNDLES
            bundles.Add(new ScriptBundle("~/bundles/admin/js").Include(
                "~/_Content/js/lib/audiojs/audio.js",
                "~/_Content/js/admin.js"
                ));
        }
    }
}