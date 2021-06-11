/*
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
 *   * See LICENSE in the project root for license information.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using EDUGraphAPI.Data;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.ViewModels;
using Microsoft.Education;
using Microsoft.Education.Data;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.Services
{
    /// <summary>
    /// A service class used to get education data by controllers
    /// </summary>
    public class SchoolsService
    {
        private EducationServiceClient educationServiceClient;
        private ApplicationDbContext dbContext;

        public SchoolsService(EducationServiceClient educationServiceClient, ApplicationDbContext dbContext)
        {
            this.educationServiceClient = educationServiceClient;
            this.dbContext = dbContext;
        }

        public async Task<Assignment[]> GetAssignmentsByClassId(string id)
        {
            var result = await educationServiceClient.GetAssignmentsByClassIdAsync(id);
            foreach (var item in result)
            {
                item.ResourceFiles = Assignment.GetResourcesFiles(item.Resources);
                item.DueDateTime = DateTimeOffset.Parse(item.DueDateTime).UtcDateTime.ToLocalTime().ToString("MM/dd/yyyy");
            }
            return result;
        }

        public async Task<Assignment> CreateAssignment(Assignment assignment)
        {
            var result = await educationServiceClient.CreateAssignmentAsync(assignment);
            return result;
        }

        public async Task<EducationAssignmentResource> AddAssignmentResourcesAsync(string classId, string assignmentId, string fileName, string resourceUrl)
        {
            return await educationServiceClient.AddAssignmentResourcesAsync(classId, assignmentId, fileName, resourceUrl);
        }

        public async Task<Assignment> PublishAssignmentAsync(string classId, string assignmentId)
        {
            return await educationServiceClient.PublishAssignmentAsync(classId, assignmentId);
        }

        public async Task<EducationAssignmentResource> AddSubmissionResourceAsync(string classId, string assignmentId, string submissionId, string fileName, string resourceUrl)
        {
            return await educationServiceClient.AddSubmissionResourceAsync(classId, assignmentId, submissionId, fileName, resourceUrl);
        }

        public async Task<EducationAssignmentResource[]> GetAssignmentResourcesAsync(string sectionId, string assignmentId)
        {
            var result = await educationServiceClient.GetAssignmentResourcesAsync(sectionId, assignmentId);
            return result;
        }

        public async Task<Assignment> GetAssignmentByIdAsync(string sectionId, string assignmentId)
        {
            var result = await educationServiceClient.GetAssignmentAsync(sectionId, assignmentId);
            return result;
        }

        public async Task<ResourcesFolder> GetAssignmentResourceFolderURL(string sectionId, string assignmentId)
        {
            var result = await educationServiceClient.GetAssignmentResourceFolder(sectionId, assignmentId);
            return result;
        }

        public async Task<Submission[]> GetAssignmentSubmissions(string sectionId, string assignmentId)
        {
            var result = await educationServiceClient.GetAssignmentSubmissionsAsync(sectionId, assignmentId);
            foreach (var item in result)
            {
                if (null != item.SubmittedBy.User && !string.IsNullOrEmpty(item.SubmittedBy.User.Id))
                {
                    item.SubmittedBy.User.DisplayName = await GetUsername(item.SubmittedBy.User.Id);
                }
                if (!string.IsNullOrEmpty(item.SubmittedDateTime))
                {
                    item.SubmittedDateTime = Convert.ToDateTime(item.SubmittedDateTime).ToShortDateString();
                }
                else
                {
                    item.SubmittedDateTime = "";
                }
                item.Resources = (await educationServiceClient.GetSubmissionResourcesAsync(sectionId, assignmentId, item.Id)).ToList();
            }
            return result;
        }

        public async Task<Submission[]> GetAssignmentSubmissionByUserAsync(string sectionId, string assignmentId, string userId)
        {
            var result = await educationServiceClient.GetAssignmentSubmissionsByUserAsync(sectionId, assignmentId, userId);
            return result;
        }

        private async Task<string> GetUsername(string userId)
        {
            var client = await AuthenticationHelper.GetGraphServiceClientAsync(Permissions.Application);
            var user = await client.Users[userId].Request().GetAsync();
            return user.DisplayName;
        }

        /// <summary>
        /// Get SchoolsViewModel
        /// </summary>
        public async Task<SchoolsViewModel> GetSchoolsViewModelAsync(UserContext userContext)
        {
            var currentUser = await educationServiceClient.GetJoinableUserAsync();

            var schools = (await educationServiceClient.GetSchoolsAsync())
                .OrderBy(i => i.Name)
                .ToArray();
            for (var i = 0; i < schools.Count(); i++)
            {
                if (schools[i].Address != null && string.IsNullOrEmpty(schools[i].Address.Street) &&
                    string.IsNullOrEmpty(schools[i].Address.PostalCode))
                {
                    schools[i].Address.Street = "-";
                }
            }

            var mySchools = currentUser.Schools.ToArray();

            var myFirstSchool = mySchools.FirstOrDefault();

            // Specific grade for students will be coming in later releases of the API.
            var grade = myFirstSchool?.EducationGrade;

            var sortedSchools = mySchools.Count() > 0 ? (schools.Where(c => c.Id == mySchools.First().Id)
                .Union(schools.Where(c => c.Id != mySchools.First().Id).ToList())) : schools;
            return new SchoolsViewModel(sortedSchools)
            {
                IsStudent = userContext.IsStudent,
                UserId = currentUser.ExternalId ?? "",
                EducationGrade = grade,
                UserDisplayName = currentUser.DisplayName,
                MySchoolId = myFirstSchool?.ExternalId ?? ""
            };
        }

        /// <summary>
        /// Get SectionsViewModel of the specified school
        /// </summary>
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMyClassesAsync(school.SchoolNumber);

            // Courses not currently represented.
            mySections = mySections.OrderBy(c => c.DisplayName).ToArray();
            var allSections = await educationServiceClient.GetAllClassesAsync(school.Id, null);
            foreach (var section in allSections.Value)
            {
                foreach (var mySection in mySections)
                {
                    if (section.Id == mySection.Id)
                    {
                        section.Teachers = mySection.Teachers;
                        section.Members = mySection.Members;
                    }
                }
            }
            return new SectionsViewModel(userContext, school, allSections, mySections);
        }

        /// <summary>
        /// Get SectionsViewModel of the specified school
        /// </summary>
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMyClassesAsync(school.SchoolNumber);
            var allSections = await educationServiceClient.GetAllClassesAsync(school.Id, nextLink);
            foreach (var section in allSections.Value)
            {
                foreach (var mySection in mySections)
                {
                    if (section.Id == mySection.Id)
                    {
                        section.Teachers = mySection.Teachers;
                        section.Members = mySection.Members;
                    }
                }
            }
            return new SectionsViewModel(userContext.UserO365Email, school, allSections, mySections);
        }

        /// <summary>
        /// Get SectionDetailsViewModel of the specified section
        /// </summary>
        public async Task<SectionDetailsViewModel> GetSectionDetailsViewModelAsync(string schoolId, string classId, IGroupRequestBuilder group)
        {
            var school = await educationServiceClient.GetSchoolAsync(schoolId);
            var @class = await educationServiceClient.GetClassAsync(classId);
            @class.Teachers = @class.Members.Where(c => c.PrimaryRole == EducationRole.Teacher).ToList();
            var driveRootFolder = await group.Drive.Root.Request().GetAsync();

            var schoolTeachers = await educationServiceClient.GetAllTeachersAsync(school.SchoolNumber);
            foreach (var sectionTeacher in @class.Teachers)
            {
                schoolTeachers = schoolTeachers.Where(t => t.Id != sectionTeacher.Id).ToArray();
            }

            foreach (var user in @class.Students)
            {
                var seat = dbContext.ClassroomSeatingArrangements.FirstOrDefault(c =>
                    c.O365UserId == user.Id && c.ClassId == classId);
                user.Position = seat?.Position ?? 0;
                var userInDB = dbContext.Users.Where(c => c.O365UserId == user.Id).FirstOrDefault();
                user.FavoriteColor = userInDB == null ? "" : userInDB.FavoriteColor;
            }
            return new SectionDetailsViewModel
            {
                School = school,
                Class = @class,
                Conversations = await group.Conversations.Request().GetAllAsync(),
                SeeMoreConversationsUrl = string.Format(Constants.O365GroupConversationsUrl, @class.MailNickname),
                DriveItems = await group.Drive.Root.Children.Request().GetAllAsync(),
                SeeMoreFilesUrl = driveRootFolder.WebUrl,
                SchoolTeachers = schoolTeachers
            };
        }

        /// <summary>
        /// Get my classes
        /// </summary>
        public async Task<string[]> GetMyClassesAsync()
        {
            var myClasses = await educationServiceClient.GetMyClassesAsync();
            return myClasses
                .Select(i => i.DisplayName)
                .ToArray();
        }
    }
}