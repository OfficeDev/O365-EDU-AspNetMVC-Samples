/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Data;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Infrastructure;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.Services;
using EDUGraphAPI.Web.ViewModels;
using Microsoft.Education.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EDUGraphAPI.Web.Controllers
{
    [HandleAdalException, EduAuthorize]
    public class SchoolsController : Controller
    {
        private ApplicationService applicationService;
        private ApplicationDbContext dbContext;
        private int pageSize = 12;

        public SchoolsController(ApplicationService applicationService, ApplicationDbContext dbContext)
        {
            this.applicationService = applicationService;
            this.dbContext = dbContext;
        }

        //
        // GET: /Schools/Index
        public async Task<ActionResult> Index()
        {
            var userContext = await applicationService.GetUserContextAsync();
            if (!userContext.AreAccountsLinked)
            {
                return View(new SchoolsViewModel() { AreAccountsLinked = false,IsLocalAccount = userContext.IsLocalAccount });
            }
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSchoolsViewModelAsync(userContext);
            model.AreAccountsLinked = userContext.AreAccountsLinked;
            
            return View(model);
        }

        //
        // GET: /Schools/{Id of a school}/Classes
        public async Task<ActionResult> Classes(string schoolId)
        {
            var userContext = await applicationService.GetUserContextAsync();
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSectionsViewModelAsync(userContext, schoolId, pageSize);
            return View(model);
        }

        //
        // GET: /Schools/{Id of a school}/Classes/Next
        public async Task<JsonResult> ClassesNext(string schoolId, string nextLink)
        {
            var userContext = await applicationService.GetUserContextAsync();
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSectionsViewModelAsync(userContext, schoolId, pageSize, nextLink);
            var sections = new List<Section>(model.Sections.Value);
            sections.AddRange(model.MySections);
            foreach (var section in sections)
            {
                if (!string.IsNullOrEmpty(section.TermStartDate))
                {
                    section.TermStartDate = Convert.ToDateTime(section.TermStartDate).ToString("yyyy-MM-ddTHH:mm:ss");
                }
                if (!string.IsNullOrEmpty(section.TermEndDate))
                {
                    section.TermEndDate = Convert.ToDateTime(section.TermEndDate).ToString("yyyy-MM-ddTHH:mm:ss");
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Schools/{Id of a school}/Users
        public async Task<ActionResult> Users(string schoolId)
        {
            var schoolsService = await GetSchoolsServiceAsync();
            var userContext = await applicationService.GetUserContextAsync();
            var model = await schoolsService.GetSchoolUsersAsync(userContext,schoolId, pageSize);
            return View(model);
        }

        //
        // GET: /Schools/{Id of a school}/Users/Next
        public async Task<JsonResult> UsersNext(string schoolId, string nextLink)
        {
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSchoolUsersAsync(schoolId, pageSize, nextLink);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Schools/{Id of a school}/Students/Next
        public async Task<JsonResult> StudentsNext(string schoolId, string nextLink)
        {
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSchoolStudentsAsync(schoolId, pageSize, nextLink);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Schools/{Id of a school}/Teachers/Next
        public async Task<JsonResult> TeachersNext(string schoolId, string nextLink)
        {
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSchoolTeachersAsync(schoolId, pageSize, nextLink);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Schools/{Id of a school}/Classes/6510F0FC-53B3-4D9B-9742-84C9C8FA2BE4
        public async Task<ActionResult> ClassDetails(string schoolId, string sectionId)
        {
            var userContext = await applicationService.GetUserContextAsync();

            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();
            var group = graphServiceClient.Groups[sectionId];

            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSectionDetailsViewModelAsync(schoolId, sectionId, group);
            model.IsStudent = userContext.IsStudent;
            model.O365UserId = userContext.User.O365UserId;
            model.MyFavoriteColor = userContext.User.FavoriteColor;
            
            return View(model);
        }

        //
        // POST: /Schools/SaveSeatingArrangements
        [HttpPost]
        public async Task<JsonResult> SaveSeatingArrangements(List<SeatingViewModel> seatingArrangements)
        {
            await applicationService.SaveSeatingArrangements(seatingArrangements);
            return Json("");
        }
        
        private async Task<SchoolsService> GetSchoolsServiceAsync()
        {
            var educationServiceClient = await AuthenticationHelper.GetEducationServiceClientAsync();
            return new SchoolsService(educationServiceClient, dbContext);
        }
    }
}