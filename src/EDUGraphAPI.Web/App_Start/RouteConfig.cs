/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Web.Mvc;
using System.Web.Routing;

namespace EDUGraphAPI.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                name: "Classes",
                url: "Schools/{schoolId}/Classes",
                defaults: new { controller = "Schools", action = "Classes" }
            );

            routes.MapRoute(
                name: "ClassesNext",
                url: "Schools/{schoolId}/Classes/Next",
                defaults: new { controller = "Schools", action = "ClassesNext" }
            );

            routes.MapRoute(
                name: "Users",
                url: "Schools/{schoolId}/Users",
                defaults: new { controller = "Schools", action = "Users" }
            );

            //routes.MapRoute(
            //    name: "Assignments",
            //    url: "Classes/{classId}/Assignments",
            //    defaults: new { controller = "Schools", action = "GetClassAssignments" }
            //);
            routes.MapRoute(
                name: "GetAssignmentResources",
                url: "Classes/{sectionId}/Assignments/{assignmentId}/Resources",
                defaults: new { controller = "Schools", action = "GetAssignmentResources" }
            );
            routes.MapRoute(
                name: "GetAssignmentSubmissions",
                url: "Classes/{sectionId}/Assignments/{assignmentId}/Submissions",
                defaults: new { controller = "Schools", action = "GetAssignmentSubmissions" }
            );
            routes.MapRoute(
                name: "GetAssignmentResourcesSubmission",
                url: "Classes/{sectionId}/Assignments/{assignmentId}/ResourcesSubmission",
                defaults: new { controller = "Schools", action = "GetAssignmentResourcesSubmission" }
            );

            routes.MapRoute(
                name: "UsersNext",
                url: "Schools/{schoolId}/Users/Next",
                defaults: new { controller = "Schools", action = "UsersNext" }
            );

            routes.MapRoute(
                name: "StudentsNext",
                url: "Schools/{schoolId}/Students/Next",
                defaults: new { controller = "Schools", action = "StudentsNext" }
            );

            routes.MapRoute(
                name: "TeachersNext",
                url: "Schools/{schoolId}/Teachers/Next",
                defaults: new { controller = "Schools", action = "TeachersNext" }
            );

            routes.MapRoute(
                name: "ClassDetails",
                url: "Schools/{schoolId}/Classes/{sectionId}",
                defaults: new { controller = "Schools", action = "ClassDetails" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
