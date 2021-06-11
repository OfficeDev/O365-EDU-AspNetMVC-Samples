/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Education.Data;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Education
{
    /// <summary>
    /// An instance of the EducationServiceClient class handles building requests,
    /// sending them to Office 365 Education API, and processing the responses.
    /// </summary>
    public class EducationServiceClient
    {
        private readonly string serviceRoot;
        private readonly IAuthenticationProvider authenticationProvider;

        public EducationServiceClient(Uri serviceRoot, IAuthenticationProvider authenticationProvider)
        {
            this.serviceRoot = serviceRoot.ToString().TrimEnd('/');
            this.authenticationProvider = authenticationProvider;
        }

        #region schools
        /// <summary>
        /// Get all schools that exist in the Office 365 tenant. 
        /// </summary>
        /// <returns></returns>
        public async Task<EducationSchool[]> GetSchoolsAsync()
        {
            var schools = await HttpGetArrayAsync<EducationSchool>("education/schools");
            return schools.ToArray();
        }

        /// <summary>
        /// Get a school by using the object_id.
        /// </summary>
        /// <param name="objectId">The Object ID of the school administrative unit in Office 365.</param>
        /// <returns></returns>
        public Task<EducationSchool> GetSchoolAsync(string objectId)
        {
            return HttpGetObjectAsync<EducationSchool>($"education/schools/{objectId}");
        }

        #endregion

        #region classes

        /// <summary>
        /// Get classes within a school.
        /// </summary>
        /// <param name="schoolId">The ID of the school in the School Information System (SIS).</param>
        /// <param name="nextLink">The nextlink for a server-side paged collection.</param>
        /// <returns></returns>
        public async Task<ArrayResult<EducationClass>> GetAllClassesAsync(string schoolId, string nextLink)
        {
            if (string.IsNullOrEmpty(schoolId))
            {
                return new ArrayResult<EducationClass>
                {
                    Value = new EducationClass[] { },
                    NextLink = nextLink
                };
            }
            else
            {
                var relativeUrl = $"education/schools/{schoolId}/classes?$top=12";
                return await HttpGetArrayAsync<EducationClass>(relativeUrl, nextLink);


            }
        }


        /// <summary>
        /// Get my classes
        /// </summary>
        /// <returns>The set of classes</returns>
        public async Task<EducationClass[]> GetMyClassesAsync(bool loadMembers = false, string expandField = "members")
        {
            var relativeUrl = $"education/me/classes";

            // Important to do this in one round trip, not in a sequence of calls.
            if (loadMembers)
            {
                relativeUrl += "?$expand=" + expandField;
            }

            var memberOf = await HttpGetArrayAsync<EducationClass>(relativeUrl);
            var classes = memberOf.ToArray();

            return classes;
        }


        /// <summary>
        /// Get my classes within a school
        /// </summary>
        /// <param name="schoolId">The ID of the school in the School Information System (SIS).</param>
        /// <returns>The set of classes</returns>
        public async Task<EducationClass[]> GetMyClassesAsync(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId))
            {
                return new EducationClass[] { };
            }
            else
            {
                // First get classes with members
                var classesWithMembers = await GetMyClassesAsync(true);

                // Then get classes with schools - we can't get both at the same time today.
                var classesWithSchools = await GetMyClassesAsync(true, "schools");


                //return classesWithMembers;
                int i = 0;
                var result = classesWithMembers
                    .Where(c =>
                    {
                        return c.Id.Equals(classesWithSchools[i].Id, StringComparison.Ordinal) &&
                            classesWithSchools[i++]
                            .Schools.Any(s => schoolId.Equals(s.ExternalId, StringComparison.OrdinalIgnoreCase));
                    })
                    .ToArray();
                foreach (var item in result)
                {
                    item.Teachers = item.Members.Where(c => c.PrimaryRole == EducationRole.Teacher).ToList();
                }
                return result;
            }
        }

        /// <summary>
        /// Get a class by using the object_id.
        /// </summary>
        /// <param name="classId">The ID of the class in Office 365.</param>
        /// <returns>The class.</returns>
        public async Task<EducationClass> GetClassAsync(string classId)
        {
            return await HttpGetObjectAsync<EducationClass>($"education/classes/{classId}?$expand=members");
        }

        #endregion

        #region student and teacher

        /// <summary>
        /// Get the current logged in user.
        /// </summary>
        /// <returns>User.</returns>
        public Task<EducationUser> GetUserAsync()
        {
            return HttpGetObjectAsync<EducationUser>("education/me");
        }

        /// <summary>
        /// Get all teachers within a school
        /// Reference URL: https://msdn.microsoft.com/en-us/office/office365/api/school-rest-operations#get-school-members
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        public async Task<EducationUser[]> GetAllTeachersAsync(string schoolId)
        {
            return await HttpGetArrayAsync<EducationUser>($"users?$filter=extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId%20eq%20'{schoolId}'%20and%20extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType%20eq%20'Teacher'");
        }



        /// <summary>
        /// Get the current logged in user with expanded relationships suitab le for joining
        /// </summary>
        /// <returns>User.</returns>
        public Task<EducationUser> GetJoinableUserAsync()
        {
            return HttpGetObjectAsync<EducationUser>("education/me?$expand=schools,classes");
        }




        #endregion

        #region Assignments
        //string classId

        /// <summary>
        /// Get a class's assignments.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationclass_list_assignments.md
        /// </summary>
        /// <param name="id">Class id.</param>
        /// <returns></returns>
        public Task<Assignment[]> GetAssignmentsByClassIdAsync(string id)
        {
            return HttpGetArrayAsync<Assignment>($"education/classes/{id}/assignments");
        }
        /// <summary>
        /// Create an assignment.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationclass_post_assignments.md
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
        public Task<Assignment> CreateAssignmentAsync(Assignment assignment)
        {
            string url = $"education/classes/{assignment.ClassId}/assignments";
            var assignmentObject = new JObject();
            assignmentObject["displayName"] = assignment.DisplayName;
            assignmentObject["status"] = assignment.Status;
            assignmentObject["dueDateTime"] = assignment.DueDateTime;
            assignmentObject["allowStudentsToAddResourcesToSubmission"] = assignment.AllowStudentsToAddResourcesToSubmission;
            var assignTo = new JObject();
            assignTo["@odata.type"] = "#microsoft.graph.educationAssignmentClassRecipient";
            assignmentObject["assignTo"] = assignTo;
            var json = JsonConvert.SerializeObject(assignmentObject);
            return HttpPostAsync<Assignment>(url, json);
        }

        /// <summary>
        /// Add resource to an assignment.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationassignment_post_resources.md
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="fileName"></param>
        /// <param name="resourceUrl"></param>
        /// <returns></returns>
        public async Task<EducationAssignmentResource> AddAssignmentResourcesAsync(string classId, string assignmentId, string fileName, string resourceUrl)
        {
            string url = $"education/classes/{classId}/assignments/{assignmentId}/resources";
            var jsonToPost = new JObject();

            //below works for link type resources
            //var link = new JObject();
            //link["displayName"] = "Bing";
            //link["link"] = "http://www.bing.com";
            //link["@odata.type"] = "#microsoft.education.assignments.api.educationLinkResource";
            //jsonToPost["resource"] = link;

            //below try to post a file
            var resource = new JObject();
            resource["displayName"] = fileName;
            resource["@odata.type"] = GetFileType(fileName); //This type must match real type.
            var file = new JObject();
            file["odataid"] = resourceUrl;
            resource["file"] = file;
            jsonToPost["resource"] = resource;

            var json = JsonConvert.SerializeObject(jsonToPost);
            var result = await HttpPostAsync<EducationAssignmentResource>(url, json);
            return result;
        }

        /// <summary>
        /// Publish an assignment. Set its status from draft to published.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationassignment_publish.md
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task<Assignment> PublishAssignmentAsync(string classId, string assignmentId)
        {
            string url = $"education/classes/{classId}/assignments/{assignmentId}/publish";
            return HttpPostAsync<Assignment>(url, ""); ;
        }

        /// <summary>
        /// Add resources to an assignment submission.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationsubmission_post_resources.md
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="fileName"></param>
        /// <param name="resourceUrl"></param>
        /// <returns></returns>
        public async Task<EducationAssignmentResource> AddSubmissionResourceAsync(string classId, string assignmentId, string submissionId, string fileName, string resourceUrl)
        {
            string url = $"education/classes/{classId}/assignments/{assignmentId}/submissions/{submissionId}/resources";
            var jsonToPost = new JObject();
            var resource = new JObject();
            resource["displayName"] = fileName;
            resource["@odata.type"] = GetFileType(fileName); //This type must match real type.
            var file = new JObject();
            file["odataid"] = resourceUrl;
            resource["file"] = file;
            jsonToPost["resource"] = resource;

            var json = JsonConvert.SerializeObject(jsonToPost);
            var result = await HttpPostAsync<EducationAssignmentResource>(url, json);
            return result;
        }
        /// <summary>
        /// Get an assignment.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationassignment_get.md
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public Task<Assignment> GetAssignmentAsync(string sectionId, string assignmentId)
        {
            return HttpGetObjectAsync<Assignment>($"education/classes/{sectionId}/assignments/{assignmentId}");
        }

        public Task<ResourcesFolder> GetAssignmentResourceFolder(string sectionId, string assignmentId)
        {
            return HttpGetObjectAsync<ResourcesFolder>($"education/classes/{sectionId}/assignments/{assignmentId}/GetResourcesFolderUrl");

        }

        /// <summary>
        /// Get resources of an assignment.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationsubmission_list_resources.md
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public Task<EducationAssignmentResource[]> GetAssignmentResourcesAsync(string sectionId, string assignmentId)
        {
            return HttpGetArrayAsync<EducationAssignmentResource>($"education/classes/{sectionId}/assignments/{assignmentId}/resources");
        }



        /// <summary>
        /// Get submissions of an assignment.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationassignment_list_submissions.md
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public Task<Submission[]> GetAssignmentSubmissionsAsync(string sectionId, string assignmentId)
        {
            return HttpGetArrayAsync<Submission>($"education/classes/{sectionId}/assignments/{assignmentId}/submissions");
        }



        /// <summary>
        /// Get a user's assignment submissions.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationuser_list_assignments.md
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<Submission[]> GetAssignmentSubmissionsByUserAsync(string sectionId, string assignmentId, string userId)
        {
            return HttpGetArrayAsync<Submission>($"education/classes/{sectionId}/assignments/{assignmentId}/submissions?$filter=submittedBy/user/id eq '{userId}'");
        }

        /// <summary>
        /// Get a submission's resources.
        /// Reference URL: https://github.com/OfficeDev/O365-EDU-Tools/blob/master/EDUGraphAPIs/Assignments/api/educationsubmission_list_resources.md
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        public Task<EducationSubmissionResource[]> GetSubmissionResourcesAsync(string sectionId, string assignmentId, string submissionId)
        {
            return HttpGetArrayAsync<EducationSubmissionResource>($"education/classes/{sectionId}/assignments/{assignmentId}/submissions/{submissionId}/resources");
        }

        #endregion
        #region HttpGet
        private async Task<string> HttpGetAsync(string relativeUrl)
        {
            var uri = serviceRoot + "/" + relativeUrl;
            if (relativeUrl.ToLower().IndexOf("https://") >= 0)
            {
                uri = relativeUrl;
            }
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            await authenticationProvider.AuthenticateRequestAsync(message);

            var client = new HttpClient();
            var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        private async Task<T> HttpGetObjectAsync<T>(string relativeUrl)
        {
            var responseString = await HttpGetAsync(relativeUrl);
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        private async Task<T[]> HttpGetArrayAsync<T>(string relativeUrl)
        {
            string responseString = await HttpGetAsync(relativeUrl);
            var array = JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
            var result = new List<T>();
            result.AddRange(array.Value);

            // NEVER do path-math on a nextToken - they are defined as opaque
            while (!string.IsNullOrEmpty(array.NextLink))
            {
                responseString = await HttpGetAsync(array.NextLink);
                array = JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
                result.AddRange(array.Value);
            }
            return result.ToArray();
        }

        private async Task<ArrayResult<T>> HttpGetArrayAsync<T>(string relativeUrl, string nextLink)
        {
            // NEVER do path-math on a nextToken - they are defined as opaque
            if (!string.IsNullOrEmpty(nextLink))
            {
                relativeUrl = nextLink;
            }

            string responseString = await HttpGetAsync(relativeUrl);
            return JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
        }
        private async Task<T> HttpPostAsync<T>(string relativeUrl, string json)
        {
            var uri = serviceRoot + "/" + relativeUrl;
            if (relativeUrl.ToLower().IndexOf("https://") >= 0)
            {
                uri = relativeUrl;
            }
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            await authenticationProvider.AuthenticateRequestAsync(message);
            var client = new HttpClient();
            var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString);

        }


        private string GetFileType(string fileName)
        {
            string defaultFileType = string.Empty;
            string ext = Path.GetExtension(fileName);
            switch (ext)
            {
                case ".docx":
                    defaultFileType = "#microsoft.graph.educationWordResource";
                    break;
                case ".xlsx":
                    defaultFileType = "#microsoft.graph.educationExcelResource";
                    break;
                default:
                    defaultFileType = "#microsoft.graph.educationFileResource"; //"#microsoft.graph.educationFileResource";
                    break;
            }
            return defaultFileType;
        }
        #endregion

        /// <summary>
        /// Get an instance of EducationServiceClient
        /// </summary>
        public static EducationServiceClient GetEducationServiceClient(IAuthenticationProvider authenticationProvider)
        {
            var serviceRoot = new Uri(new Uri(Constants.Resources.MSGraph), Constants.Resources.MSGraphVersion);
            return new EducationServiceClient(serviceRoot, authenticationProvider);
        }
    }
}