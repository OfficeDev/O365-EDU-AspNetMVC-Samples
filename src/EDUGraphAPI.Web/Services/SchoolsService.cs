/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Data;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.ViewModels;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Education;

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

        /// <summary>
        /// Get SchoolsViewModel
        /// </summary>
        public async Task<SchoolsViewModel> GetSchoolsViewModelAsync(UserContext userContext)
        {
            EducationUser currentUser = await educationServiceClient.GetJoinableUserAsync();
            
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

            var sortedSchools = mySchools
                .Union(schools.Except(mySchools));
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
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId, int top)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMyClassesAsync(school.SchoolNumber);

            // Courses not currently represented.
            mySections = mySections.OrderBy(c => c.ClassNumber).ToArray();
            var allSections = await educationServiceClient.GetAllClassesAsync(school.SchoolNumber, null);
            return new SectionsViewModel(userContext, school, allSections, mySections);
        }

        /// <summary>
        /// Get SectionsViewModel of the specified school
        /// </summary>
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMyClassesAsync(school.SchoolNumber);
            var allSections = await educationServiceClient.GetAllClassesAsync(school.SchoolNumber, nextLink);

            return new SectionsViewModel(userContext.UserO365Email, school, allSections, mySections);
        }

        /// <summary>
        /// Get users, teachers and students of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolUsersAsync(UserContext userContext, string objectId)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var users = await educationServiceClient.GetSchoolUsersAsync(objectId, null);
            var students = await educationServiceClient.GetStudentsAsync(school.SchoolNumber, null);
            var teachers = await educationServiceClient.GetTeachersAsync(school.SchoolNumber, null);
            ArrayResult<EducationUser> studentsInMyClasses = null;
            if (userContext.IsFaculty)
            {
                var mySections = await educationServiceClient.GetMyClassesAsync(true);
                studentsInMyClasses = new ArrayResult<EducationUser>();
                List<EducationUser> studentsList = new List<EducationUser>();
                foreach (var item in mySections)
                {
                    if (item.ExternalId == school.ExternalId)
                    {
                        foreach (var user in item.Members)
                        {
                            if (user.PrimaryRole == EducationRole.Student && !studentsList.Any(s => s.Id == user.Id))
                            {
                                studentsList.Add(user);
                            }
                        }
                    }
                }

                studentsInMyClasses.Value = studentsList.ToArray();
            }
            return new SchoolUsersViewModel(userContext, school, users, students, teachers, studentsInMyClasses);
        }

        /// <summary>
        /// Get users of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolUsersAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var users = await educationServiceClient.GetSchoolUsersAsync(objectId, nextLink);
            return new SchoolUsersViewModel(school, users, null, null);
        }

        /// <summary>
        /// Get students of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolStudentsAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var students = await educationServiceClient.GetStudentsAsync(school.SchoolNumber, nextLink);
            return new SchoolUsersViewModel(school, null, students, null);
        }

        /// <summary>
        /// Get teachers of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolTeachersAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var teachers = await educationServiceClient.GetTeachersAsync(school.SchoolNumber, nextLink);
            return new SchoolUsersViewModel(school, null, null, teachers);
        }

        /// <summary>
        /// Get SectionDetailsViewModel of the specified section
        /// </summary>
        public async Task<SectionDetailsViewModel> GetSectionDetailsViewModelAsync(string schoolId, string classId, IGroupRequestBuilder group)
        {
            var school = await educationServiceClient.GetSchoolAsync(schoolId);
            var @class = await educationServiceClient.GetClassAsync(classId);
            var driveRootFolder = await group.Drive.Root.Request().GetAsync();
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
                SeeMoreFilesUrl = driveRootFolder.WebUrl
            };
        }

        /// <summary>
        /// Get my classes
        /// </summary>
        public async Task<string[]> GetMyClassesAsync()
        {
            var myClasses = await educationServiceClient.GetMyClassesAsync();
            return myClasses
                .Select(i => i.ExternalName)
                .ToArray();
        }
    }
}