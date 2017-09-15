/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Microsoft.Education
{
    public class EducationClass : GraphEntity
    {
        public EducationClass()
        {
            this.Members = new List<EducationUser>();
            this.Schools = new List<EducationSchool>();
            this.Teachers = new List<EducationUser>();
        }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }

        [JsonProperty("period")]
        public string Period { get; set; }

        [JsonProperty("classNumber")]
        public string ClassNumber { get; set; }

        [JsonProperty("externalName")]
        public string ExternalName { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("externalSource")]
        public EducationExternalSource ExternalSource { get; set; }

        [JsonProperty("createdBy")]
        public IdentitySet CreatedBy { get; set; }

        public List<EducationUser> Members { get; set; }

        public IEnumerable<EducationUser> Students => Members.Where(m => m.PrimaryRole == EducationRole.Student );

        public IEnumerable<EducationUser> Teachers { get; set; }

        public IEnumerable<EducationSchool> Schools { get; set; }
    }
}