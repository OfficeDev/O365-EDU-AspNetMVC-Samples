/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
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
    /// An instance of the EducationServiceClient class handles building requests,
    /// sending them to Office 365 Education API, and processing the responses.
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
            var relativeUrl = $"education/classes?$expand=schools";
            var interim = await HttpGetArrayAsync<EducationClass>(relativeUrl, nextLink);
            interim = new ArrayResult<EducationClass>
            {
                Value = interim.Value.Where(c =>
                                   c.Schools.Any(
                                       s => s.ExternalId.Equals(schoolId, StringComparison.OrdinalIgnoreCase)))
                               .ToArray(),
                NextLink = interim.NextLink
            };
            foreach (EducationClass theClass in interim.Value)
            {
                theClass.Schools.Clear();
            }
            return interim;
        }

        /// <summary>
        /// Get my classes
        /// </summary>
        /// <returns>The set of classes</returns>
        public async Task<EducationClass[]> GetMyClassesAsync(bool loadMembers = false)
        {
            var relativeUrl = $"education/me/classes";
            
            // Important to do this in one round trip, not in a sequence of calls.
            if (loadMembers)
            {
                relativeUrl += "?$expand=members";
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
            var sections = await GetMyClassesAsync(true);
            return sections
                .Where(s => s.ExternalId.Equals(schoolId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
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
        /// Get the current logged in user with expanded relationships suitab le for joining
        /// </summary>
        /// <returns>User.</returns>
        public Task<EducationUser> GetJoinableUserAsync()
        {
            return HttpGetObjectAsync<EducationUser>("education/me?$expand=schools,classes");
        }

        /// <summary>
        /// Get members within a school
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public async Task<ArrayResult<EducationUser>> GetSchoolUsersAsync(string objectId, string nextLink)
        {
            return await HttpGetArrayAsync<EducationUser>($"education/schools/{objectId}/users", nextLink);
        }

        /// <summary>
        /// Get students within a school
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns>Students</returns>
        public async Task<ArrayResult<EducationUser>> GetStudentsAsync(string schoolId, string nextLink)
        {
            var interim = await HttpGetArrayAsync<EducationUser>($"users?$expand=schools", nextLink);
            interim = new ArrayResult<EducationUser>
            {
                Value = interim.Value.Where(u => u.PrimaryRole == EducationRole.Student &&
                                                 u.Schools.Any(s =>
                                                     s.ExternalId.Equals(schoolId,
                                                         StringComparison.OrdinalIgnoreCase))).ToArray(),
                NextLink = interim.NextLink
            };

            foreach (var user in interim.Value)
            {
                user.Schools.Clear();
            }
            return interim;
        }

        /// <summary>
        /// Get teachers within a school
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns>Teachers</returns>
        public async Task<ArrayResult<EducationUser>> GetTeachersAsync(string schoolId, string nextLink)
        {
            var interim = await HttpGetArrayAsync<EducationUser>($"users?$expand=schools", nextLink);
            interim = new ArrayResult<EducationUser>
            {
                Value = interim.Value.Where(u => u.PrimaryRole == EducationRole.Teacher &&
                                                 u.Schools.Any(s =>
                                                     s.ExternalId.Equals(schoolId,
                                                         StringComparison.OrdinalIgnoreCase))).ToArray(),
                NextLink = interim.NextLink
            };

            foreach (var user in interim.Value)
            {
                user.Schools.Clear();
            }
            return interim;
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
        #endregion

        /// <summary>
        /// Get an instance of EducationServiceClient
        /// </summary>
        public static EducationServiceClient GetEducationServiceClient(string accessToken)
        {
            var serviceRoot = new Uri(new Uri(Constants.Resources.MSGraph), Constants.Resources.MSGraphVersion);
            return new EducationServiceClient(serviceRoot, () => Task.FromResult(accessToken));
        }
    }
}