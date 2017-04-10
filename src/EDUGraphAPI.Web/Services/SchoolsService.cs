/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Data;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.ViewModels;
using Microsoft.Education;
using Microsoft.Education.Data;
using Microsoft.Graph;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
            var currentUser = userContext.IsStudent
                ? await educationServiceClient.GetStudentAsync() as SectionUser
                : await educationServiceClient.GetTeacherAsync() as SectionUser;

            var schools = (await educationServiceClient.GetSchoolsAsync())
                .OrderBy(i => i.Name)
                .ToArray();
            BingMapService mapServices = new BingMapService();
            for (var i = 0; i < schools.Count(); i++)
            {
                var address = string.Format("{0}/{1}/{2}", schools[i].State, HttpUtility.HtmlEncode(schools[i].City), HttpUtility.HtmlEncode(schools[i].Address));
                if (!string.IsNullOrEmpty(schools[i].Address))
                {
                    var longitudeAndLatitude = await mapServices.GetLongitudeAndLatitudeByAddress(address);
                    if (longitudeAndLatitude.Count() == 2)
                    {
                        schools[i].Latitude = longitudeAndLatitude[0].ToString();
                        schools[i].Longitude = longitudeAndLatitude[1].ToString();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(schools[i].Zip))
                        schools[i].Address = "-";
                }
            }

            var mySchools = schools
                .Where(i => i.SchoolId == currentUser.SchoolId)
                .ToArray();

            var myFirstSchool = mySchools.FirstOrDefault();
            var grade = userContext.IsStudent ? currentUser.EducationGrade : myFirstSchool?.EducationGrade;

            var sortedSchools = mySchools
                .Union(schools.Except(mySchools));
            return new SchoolsViewModel(sortedSchools)
            {
                IsStudent = userContext.IsStudent,
                UserId = currentUser.UserId,
                EducationGrade = grade,
                UserDisplayName = currentUser.DisplayName,
                MySchoolId = currentUser.SchoolId,
                BingMapKey = Constants.BingMapKey
            };
        }

        /// <summary>
        /// Get SectionsViewModel of the specified school
        /// </summary>
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId, int top)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMySectionsAsync(school.SchoolId);
            mySections = mySections.OrderBy(c => c.CombinedCourseNumber).ToArray();
            var allSections = await educationServiceClient.GetAllSectionsAsync(school.SchoolId, top, null);
            return new SectionsViewModel(userContext, school, allSections, mySections);
        }

        /// <summary>
        /// Get SectionsViewModel of the specified school
        /// </summary>
        public async Task<SectionsViewModel> GetSectionsViewModelAsync(UserContext userContext, string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var mySections = await educationServiceClient.GetMySectionsAsync(school.SchoolId);
            var allSections = await educationServiceClient.GetAllSectionsAsync(school.SchoolId, top, nextLink);

            return new SectionsViewModel(userContext.UserO365Email, school, allSections, mySections);
        }

        /// <summary>
        /// Get users, teachers and students of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolUsersAsync(string objectId, int top)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var users = await educationServiceClient.GetMembersAsync(objectId, top, null);
            var students = await educationServiceClient.GetStudentsAsync(school.SchoolId, top, null);
            var teachers = await educationServiceClient.GetTeachersAsync(school.SchoolId, top, null);
            return new SchoolUsersViewModel(school, users, students, teachers);
        }

        /// <summary>
        /// Get users of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolUsersAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var users = await educationServiceClient.GetMembersAsync(objectId, top, nextLink);
            return new SchoolUsersViewModel(school, users, null, null);
        }

        /// <summary>
        /// Get students of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolStudentsAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var students = await educationServiceClient.GetStudentsAsync(school.SchoolId, top, nextLink);
            return new SchoolUsersViewModel(school, null, students, null);
        }

        /// <summary>
        /// Get teachers of the specified school
        /// </summary>
        public async Task<SchoolUsersViewModel> GetSchoolTeachersAsync(string objectId, int top, string nextLink)
        {
            var school = await educationServiceClient.GetSchoolAsync(objectId);
            var teachers = await educationServiceClient.GetTeachersAsync(school.SchoolId, top, nextLink);
            return new SchoolUsersViewModel(school, null, null, teachers);
        }

        /// <summary>
        /// Get SectionDetailsViewModel of the specified section
        /// </summary>
        public async Task<SectionDetailsViewModel> GetSectionDetailsViewModelAsync(string schoolId, string classId, IGroupRequestBuilder group)
        {
            var school = await educationServiceClient.GetSchoolAsync(schoolId);
            var section = await educationServiceClient.GetSectionAsync(classId);
            var driveRootFolder = await group.Drive.Root.Request().GetAsync();
            foreach (var user in section.Students)
            {
                var seat = dbContext.ClassroomSeatingArrangements.Where(c => c.O365UserId == user.O365UserId && c.ClassId == classId).FirstOrDefault();
                user.Position = (seat == null ? 0 : seat.Position);
                var userInDB = dbContext.Users.Where(c => c.O365UserId == user.O365UserId).FirstOrDefault();
                user.FavoriteColor = userInDB == null ? "" : userInDB.FavoriteColor;
            }
            return new SectionDetailsViewModel
            {
                School = school,
                Section = section,
                Conversations = await group.Conversations.Request().GetAllAsync(),
                SeeMoreConversationsUrl = string.Format(Constants.O365GroupConversationsUrl, section.Email),
                DriveItems = await group.Drive.Root.Children.Request().GetAllAsync(),
                SeeMoreFilesUrl = driveRootFolder.WebUrl
            };
        }

        /// <summary>
        /// Get my classes
        /// </summary>
        public async Task<string[]> GetMyClassesAsync()
        {
            var myClasses = await educationServiceClient.GetMySectionsAsync();
            return myClasses
                .Select(i => i.SectionName)
                .ToArray();
        }
    }
}