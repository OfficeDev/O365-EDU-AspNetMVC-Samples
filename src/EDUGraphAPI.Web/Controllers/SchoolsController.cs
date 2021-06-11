/*
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
 *   * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EDUGraphAPI.Data;
using EDUGraphAPI.Infrastructure;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Infrastructure;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.Services;
using EDUGraphAPI.Web.ViewModels;
using Microsoft.Education;
using Microsoft.Education.Data;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.Controllers
{
    [HandleAdalException, EduAuthorize]
    public class SchoolsController : Controller
    {
        private ApplicationService applicationService;
        private ApplicationDbContext dbContext;

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
                return View(new SchoolsViewModel() { AreAccountsLinked = false, IsLocalAccount = userContext.IsLocalAccount });
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
            var model = await schoolsService.GetSectionsViewModelAsync(userContext, schoolId);
            return View(model);
        }

        //
        // GET: /Schools/{Id of a school}/Classes/Next
        public async Task<JsonResult> ClassesNext(string schoolId, string nextLink)
        {
            var userContext = await applicationService.GetUserContextAsync();
            var schoolsService = await GetSchoolsServiceAsync();
            var model = await schoolsService.GetSectionsViewModelAsync(userContext, schoolId, nextLink);
            var classes = new List<Microsoft.Education.EducationClass>(model.Classes.Value);
            classes.AddRange(model.MyClasses);
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
            var assignmentServices = await GetAssignmentsServiceAsync();
            model.Assignments = await assignmentServices.GetAssignmentsByClassId(model.Class.Id);
            return View(model);
        }

        public async Task<JsonResult> GetAssignmentResources(string sectionId, string assignmentId)
        {
            var assignmentServices = await GetAssignmentsServiceAsync();
            var model = await assignmentServices.GetAssignmentResourcesAsync(sectionId, assignmentId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAssignmentSubmissions(string sectionId, string assignmentId)
        {
            var assignmentServices = await GetAssignmentsServiceAsync();
            var model = await assignmentServices.GetAssignmentSubmissions(sectionId, assignmentId);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetAssignmentResourcesSubmission(string sectionId, string assignmentId)
        {
            var userContext = await applicationService.GetUserContextAsync();
            var assignmentServices = await GetAssignmentsServiceAsync();
            var resources = await assignmentServices.GetAssignmentResourcesAsync(sectionId, assignmentId);
            var submissions = await assignmentServices.GetAssignmentSubmissionByUserAsync(sectionId, assignmentId, userContext.User.O365UserId);
            object model = new { resources = resources, submission = submissions.Length > 0 ? submissions[0] : null };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> NewAssignment(string schoolId, string classId, string status, string name, string duedate, string duetime, string FilesToBeUploaded, List<HttpPostedFileBase> fileUpload)
        {
            Assignment assignment = new Assignment();
            assignment.AllowStudentsToAddResourcesToSubmission = true;
            assignment.ClassId = classId;
            assignment.DisplayName = name;
            assignment.DueDateTime = Convert.ToDateTime(duedate + " " + duetime).ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'");
            assignment.AllowLateSubmissions = true;
            assignment.Status = "draft";

            var assignmentServices = await GetAssignmentsServiceAsync();
            assignment = await assignmentServices.CreateAssignment(assignment);
            if (status == "assigned")
            {
                await assignmentServices.PublishAssignmentAsync(classId, assignment.Id);
            }
            var resourceFolder = await assignmentServices.GetAssignmentResourceFolderURL(classId, assignment.Id);

            var graphServiceClient = await AuthenticationHelper.GetGraphAssignmentServiceClientAsync();
            foreach (HttpPostedFileBase item in fileUpload)
            {
                if (item != null)
                {
                    DriveItem file = await UploadFile(classId, graphServiceClient, item, resourceFolder.ResourceFolderURL);
                    string resourceUrl = string.Format("https://graph.microsoft.com/v1.0/drives/{0}/items/{1}", file.ParentReference.DriveId, file.Id);
                    await assignmentServices.AddAssignmentResourcesAsync(classId, assignment.Id, file.Name, resourceUrl);
                }
            }

            return RedirectToAction("ClassDetails", new { schoolId = schoolId, sectionId = classId, tab = "assignments" });
        }

        [HttpPost]
        public async Task<ActionResult> NewAssignmentSubmissionResource(string schoolId, string classId, string assignmentId, string submissionId, List<HttpPostedFileBase> newResource)
        {
            var userContext = await applicationService.GetUserContextAsync();
            var assignmentServices = await GetAssignmentsServiceAsync();
            var graphServiceClient = await AuthenticationHelper.GetGraphAssignmentServiceClientAsync();

            var submissions = await assignmentServices.GetAssignmentSubmissionByUserAsync(classId, assignmentId, userContext.User.O365UserId);
            if (submissions.Length > 0)
            {
                foreach (HttpPostedFileBase item in newResource)
                {
                    if (item != null)
                    {
                        DriveItem file = await UploadFile(classId, graphServiceClient, item, submissions[0].ResourcesFolderUrl);
                        string resourceUrl = string.Format("https://graph.microsoft.com/v1.0/drives/{0}/items/{1}", file.ParentReference.DriveId, file.Id);
                        await assignmentServices.AddSubmissionResourceAsync(classId, assignmentId, submissionId, file.Name, resourceUrl);
                    }
                }
            }
            return RedirectToAction("ClassDetails", new { schoolId = schoolId, sectionId = classId, tab = "assignments" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAssignment(string schoolId, string classId, string assignmentId, string assignmentStatus, List<HttpPostedFileBase> newResource)
        {
            var assignmentServices = await GetAssignmentsServiceAsync();
            var assignment = await assignmentServices.GetAssignmentByIdAsync(classId, assignmentId);
            if (assignment.Status.Equals("draft") && assignmentStatus.Equals("assigned"))
            {
                assignment = await assignmentServices.PublishAssignmentAsync(classId, assignmentId);
            }
            var graphServiceClient = await AuthenticationHelper.GetGraphAssignmentServiceClientAsync();
            var resourceFolder = await assignmentServices.GetAssignmentResourceFolderURL(classId, assignmentId);
            foreach (HttpPostedFileBase item in newResource)
            {
                if (item != null)
                {
                    DriveItem file = await UploadFile(classId, graphServiceClient, item, resourceFolder.ResourceFolderURL);
                    string resourceUrl = string.Format("https://graph.microsoft.com/v1.0/drives/{0}/items/{1}", file.ParentReference.DriveId, file.Id);
                    await assignmentServices.AddAssignmentResourcesAsync(classId, assignment.Id, file.Name, resourceUrl);
                }
            }
            return RedirectToAction("ClassDetails", new { schoolId = schoolId, sectionId = classId, tab = "assignments" });
        }

        private async Task<DriveItem> UploadFile(string classId, GraphServiceClient graphServiceClient, HttpPostedFileBase item, string resourceFolder)
        {
            string[] ids = GetIdsFromResourceFolder(resourceFolder);
            var exsitItems = await graphServiceClient.Drives[ids[0]].Items[ids[1]].Children.Request().Filter($"Name eq '{item.FileName}'").GetAsync();
            string itemId = string.Empty;
            string[] separatingChars = { "\\" };
            string filename = item.FileName;
            if (filename.IndexOf("\\") >= 0)
            {
                var a = filename.Split(separatingChars, StringSplitOptions.None);
                filename = a[a.Length - 1];
            }
            if (exsitItems.Count == 0)
            {
                var newItem = await graphServiceClient.Drives[ids[0]]
                            .Items[ids[1]].Children.Request().AddAsync(new DriveItem
                            {
                                Name = filename,
                                File = new Microsoft.Graph.File()
                            });
                itemId = newItem.Id;
            }
            else
            {
                itemId = exsitItems[0].Id;
            }
            return await graphServiceClient.Drives[ids[0]]
                .Items[itemId].Content.Request()
            .PutAsync<DriveItem>(item.InputStream);
        }

        //
        //GET: /Schools/{Id of a school}/Classes/6510F0FC-53B3-4D9B-9742-84C9C8FA2BE4/UserId
        public async Task<ActionResult> AddCoTeacher(string schoolId, string sectionId, string userId)
        {
            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();
            var directoryObj = new Microsoft.Graph.DirectoryObject
            {
                Id = userId
            };
            try
            {
                await graphServiceClient.Groups[sectionId].Members.References.Request().AddAsync(directoryObj);
                await graphServiceClient.Groups[sectionId].Owners.References.Request().AddAsync(directoryObj);
            }
            catch (ServiceException)
            {
            }
            return RedirectToAction("ClassDetails", new { schoolId = schoolId, sectionId = sectionId });
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
            var accessToken = await AuthenticationHelper.GetAccessTokenAsync(Constants.Resources.MSGraph, Permissions.Delegated);
            var educationServiceClient = EducationServiceClient.GetEducationServiceClient(new BearerAuthenticationProvider(accessToken));
            return new SchoolsService(educationServiceClient, dbContext);
        }

        private async Task<SchoolsService> GetAssignmentsServiceAsync()
        {
            var educationServiceClient = await AuthenticationHelper.GetAssignmentServiceClientAsync();
            return new SchoolsService(educationServiceClient, dbContext);
        }

        private string[] GetIdsFromResourceFolder(string resourceFolder)
        {
            string[] array = resourceFolder.Split('/');
            int length = array.Length;
            string[] result = new string[2] { "", "" };
            if (array.Length >= 3)
            {
                result[0] = array[array.Length - 3];
                result[1] = array[array.Length - 1];
            }
            return result;
        }
    }
}