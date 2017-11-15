/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Web;
using System.Web.Optimization;

namespace EDUGraphAPI.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                        "~/Scripts/site.js"));


            bundles.Add(new ScriptBundle("~/bundles/sections").Include(
                        "~/Scripts/moment.min.js",
                        "~/Scripts/sections.js"));

            bundles.Add(new ScriptBundle("~/bundles/classdetail").Include(
                        "~/Scripts/jquery.tablesorter.min.js",
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/moment.min.js",
                        "~/Scripts/classdetail.js",
                        "~/Scripts/Assignments.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/users").Include(
                        "~/Scripts/users.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/login").Include(
                        "~/Content/login.css"));

            bundles.Add(new StyleBundle("~/Content/register").Include(
                        "~/Content/register.css"));
            bundles.Add(new StyleBundle("~/Content/classdetail").Include(
            "~/Content/jquery-ui.css"));
        }
    }
}
