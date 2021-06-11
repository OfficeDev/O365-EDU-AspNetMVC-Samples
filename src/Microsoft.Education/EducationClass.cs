/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


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

        [JsonProperty("classCode")]
        public string ClassCode { get; set; }

        [JsonProperty("externalName")]
        public string ExternalName { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("externalSource")]
        public EducationExternalSource ExternalSource { get; set; }

        [JsonProperty("createdBy")]
        public IdentitySet CreatedBy { get; set; }

        [JsonProperty("term")]
        public EducationTerm Term { get; set; }

        public IList<EducationUser> Members { get; set; }

        public IEnumerable<EducationUser> Students => Members.Where(m => m.PrimaryRole == EducationRole.Student);

        public IList<EducationUser> Teachers { get; set; }

        public IList<EducationSchool> Schools { get; set; }
    }
}