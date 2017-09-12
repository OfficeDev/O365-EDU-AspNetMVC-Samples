/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Microsoft.Education.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Education
{
    /// <summary>
    /// An instance of the EducationServiceClient class handles building requests, sending them to Office 365 Education API, and processing the responses.
    /// </summary>
    public class EducationServiceClient
    {
        private readonly string serviceRoot;
        private readonly Func<Task<string>> accessTokenGetter;

        public EducationServiceClient(Uri serviceRoot, Func<Task<string>> accessTokenGetter)
        {
            this.serviceRoot = serviceRoot.ToString().TrimEnd('/');
            this.accessTokenGetter = accessTokenGetter;
        }

        #region schools
        /// <summary>
        /// Get all schools that exist in the Azure Active Directory tenant. 
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/school-rest-operations#get-all-schools
        /// </summary>
        /// <returns></returns>
        public async Task<School[]> GetSchoolsAsync()
        {
            var schools = await HttpGetArrayAsync<School>("administrativeUnits");
            return schools.Where(c => c.EducationObjectType == "School").ToArray();
        }

        /// <summary>
        /// Get a school by using the object_id.
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/school-rest-operations#get-a-school.
        /// </summary>
        /// <param name="objectId">The Object ID of the school administrative unit in Azure Active Directory.</param>
        /// <returns></returns>
        public Task<School> GetSchoolAsync(string objectId)
        {
            return HttpGetObjectAsync<School>($"administrativeUnits/{objectId}");
        }

        #endregion

        #region sections
        /// <summary>
        /// Get sections within a school.
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/school-rest-operations#get-sections-within-a-school.
        /// </summary>
        /// <param name="schoolId">The ID of the school in the School Information System (SIS).</param>
        /// <returns></returns>
        public Task<ArrayResult<Section>> GetAllSectionsAsync(string schoolId, int top, string nextLink)
        {
            var relativeUrl = $"groups?$filter=extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType%20eq%20'Section'%20and%20extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId%20eq%20'{schoolId}'";
            return HttpGetArrayAsync<Section>(relativeUrl, top, nextLink);
        }

        /// <summary>
        /// Get my sections
        /// </summary>
        /// <returns></returns>
        public async Task<Section[]> GetMySectionsAsync(bool loadMembers = false)
        {
            var relativeUrl = $"me/memberOf/$/microsoft.graph.group";
            var memberOf = await HttpGetArrayAsync<Section>(relativeUrl);
            var sections = memberOf
                .Where(i => i.EducationObjectType == "Section")
                .ToArray();
            if (loadMembers == false) return sections;

            // Get sections with members
            var tasks = new List<Task>();
            var sectionBag = new ConcurrentBag<Section>();
            foreach (var section in sections)
            {
                var task = Task.Run(async () =>
                {
                    var s = await GetSectionAsync(section.Id);
                    sectionBag.Add(s);
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            return sectionBag.ToArray();
        }

        /// <summary>
        /// Get my sections within a school
        /// </summary>
        /// <param name="schoolId">The ID of the school in the School Information System (SIS).</param>
        /// <returns></returns>
        public async Task<Section[]> GetMySectionsAsync(string schoolId)
        {
            var sections = await GetMySectionsAsync(true);
            return sections
                .Where(i => i.SchoolId == schoolId)
                .ToArray();
        }

        /// <summary>
        /// Get a section by using the object_id.
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/section-rest-operations#get-a-section.
        /// </summary>
        /// <param name="sectionId">The Object ID of the section group in Azure Active Directory.</param>
        /// <returns></returns>
        public async Task<Section> GetSectionAsync(string sectionId)
        {
            return await HttpGetObjectAsync<Section>($"groups/{sectionId}?$expand=members");
        }

        #endregion

        #region student and teacher
        /// <summary>
        /// You can get the current logged in user and check if that user is a student.
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/student-rest-operations#get-current-user.
        /// </summary>
        /// <returns></returns>
        public Task<Student> GetStudentAsync()
        {
            return HttpGetObjectAsync<Student>("me");
        }

        /// <summary>
        /// You can get the current logged in user and check if that user is a teacher.
        /// Reference URL: https://msdn.microsoft.com/office/office365/api/student-rest-operations#get-current-user.
        /// </summary>
        /// <returns></returns>
        public Task<Teacher> GetTeacherAsync()
        {
            return HttpGetObjectAsync<Teacher>("me");
        }

        /// <summary>
        /// Get members within a school
        /// Reference URL: https://msdn.microsoft.com/en-us/office/office365/api/school-rest-operations#get-school-members
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public async Task<ArrayResult<SectionUser>> GetMembersAsync(string objectId, int top, string nextLink)
        {
            return await HttpGetArrayAsync<SectionUser>($"administrativeUnits/{objectId}/members", top, nextLink);
        }

        /// <summary>
        /// Get students within a school
        /// Reference URL: https://msdn.microsoft.com/en-us/office/office365/api/school-rest-operations#get-school-members
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        public async Task<ArrayResult<SectionUser>> GetStudentsAsync(string schoolId, int top, string nextLink)
        {
            return await HttpGetArrayAsync<SectionUser>($"users?$filter=extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId%20eq%20'{schoolId}'%20and%20extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType%20eq%20'Student'", top, nextLink);
        }

        /// <summary>
        /// Get teachers within a school
        /// Reference URL: https://msdn.microsoft.com/en-us/office/office365/api/school-rest-operations#get-school-members
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        public async Task<ArrayResult<SectionUser>> GetTeachersAsync(string schoolId, int top, string nextLink)
        {
            return await HttpGetArrayAsync<SectionUser>($"users?$filter=extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId%20eq%20'{schoolId}'%20and%20extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType%20eq%20'Teacher'", top, nextLink);
        }

        #endregion

        #region HttpGet
        private async Task<string> HttpGetAsync(string relativeUrl)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", await accessTokenGetter());

            var uri = serviceRoot + "/" + relativeUrl;
            var response = await client.GetAsync(uri);
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
            var responseString = await HttpGetAsync(relativeUrl);
            var array = JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
            List<T> result = new List<T>();
            result.AddRange(array.Value);
            while (!string.IsNullOrEmpty(array.NextLink) && array.NextLink.IndexOf('?') >= 0)
            {
                var token = Regex.Match(array.NextLink, @"[$]skiptoken[=][^&]+", RegexOptions.Compiled).Value;
                if (!string.IsNullOrEmpty(token))
                {
                    var str = relativeUrl.IndexOf('?') >= 0 ? "&" : "?";
                    responseString = await HttpGetAsync(relativeUrl + str + token);
                    array = JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
                    result.AddRange(array.Value);
                }
            }
            return result.ToArray();
        }

        private async Task<ArrayResult<T>> HttpGetArrayAsync<T>(string relativeUrl, int top, string nextLink)
        {
            var str = relativeUrl.IndexOf('?') >= 0 ? "&" : "?";
            relativeUrl += $"{str}$top={top}";
            if (!string.IsNullOrEmpty(nextLink) && nextLink.IndexOf('?') >= 0)
            {
                var token = Regex.Match(nextLink, @"[$]skiptoken[=][^&]+", RegexOptions.Compiled).Value;
                if (!string.IsNullOrEmpty(token))
                {
                    relativeUrl += $"&{token}";
                }
            }
            var responseString = await HttpGetAsync(relativeUrl);
            return JsonConvert.DeserializeObject<ArrayResult<T>>(responseString);
        }
        #endregion
    }
}