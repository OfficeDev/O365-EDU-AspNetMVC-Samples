/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Education.Legacy;

namespace Microsoft.Education.Data.Legacy
{
    public class Section
    {
        public Section()
        {
            this.Members = new List<SectionUser>();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType")]
        public string EducationObjectType { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("mail")]
        public string Email { get; set; }

        [JsonProperty("securityEnabled")]
        public bool SecurityEnabled { get; set; }

        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }
        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Period")]
        public string Period { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_CourseNumber")]
        public string CourseNumber { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_CourseDescription")]
        public string CourseDescription { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_CourseName")]
        public string CourseName { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_CourseId")]
        public string CourseId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_TermEndDate")]
        public string TermEndDate { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_TermStartDate")]
        public string TermStartDate { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_TermName")]
        public string TermName { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_TermId")]
        public string TermId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SectionNumber")]
        public string SectionNumber { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SectionName")]
        public string SectionName { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SectionId")]
        public string SectionId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId")]
        public string SchoolId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource")]
        public string SyncSource { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_AnchorId")]
        public string AnchorId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Status")]
        public string EducationStatus { get; set; }

        public string CombinedCourseNumber => CourseName.Substring(0, 3).ToUpper() + Regex.Match(CourseNumber, @"\d+").Value;

        public List<SectionUser> Members { get; set; }

        public IEnumerable<SectionUser> Students => Members.Where(c => c.ObjectType == EduConstants.StudentObjectType);

        public IEnumerable<SectionUser> Teachers => Members.Where(c => c.ObjectType == EduConstants.TeacherObjectType);
    }
}