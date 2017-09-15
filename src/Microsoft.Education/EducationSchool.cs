/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class EducationSchool : EducationOrganization
    {
        public EducationSchool()
        {
            this.Users = new List<EducationUser>();
            this.Classes = new List<EducationClass>();
        }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("principalName")]
        public string PrincipalName { get; set; }

        [JsonProperty("principalEmail")]
        public string PrincipalEmail { get; set; }

        [JsonProperty("externalPrincipalId")]
        public string ExternalPrincipalId { get; set; }

        [JsonProperty("highestGrade")]
        public string HighestGrade { get; set; }

        [JsonProperty("lowestGrade")]
        public string LowestGrade { get; set; }

        [JsonProperty("schoolNumber")]
        public string SchoolNumber { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("fax")]
        public string Fax { get; set; }

        [JsonProperty("createdBy")]
        public IdentitySet CreatedBy { get; set; }

        [JsonProperty("address")]
        public PhysicalAddress Address { get; set; }

        public string EducationGrade => $"{LowestGrade} - {HighestGrade}";

        public IEnumerable<EducationClass> Classes { get; set; }

        public IEnumerable<EducationUser> Users { get; set; }
    }
}