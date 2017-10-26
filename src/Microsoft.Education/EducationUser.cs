/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class EducationUser : User
    {
        public EducationUser()
        {
            this.Schools = new List<EducationSchool>();
            this.Classes = new List<EducationClass>();
        }

        [JsonProperty("primaryRole")]
        public EducationRole PrimaryRole { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [JsonProperty("externalSource")]
        public EducationExternalSource ExternalSource { get; set; }

        [JsonProperty("residenceAddress")]
        public PhysicalAddress ResidenceAddress { get; set; }

        [JsonProperty("mailingAddress")]
        public PhysicalAddress MailingAddress { get; set; }

        [JsonProperty("student")]
        public EducationStudent Student { get; set; }

        [JsonProperty("teacher")]
        public EducationTeacher Teacher { get; set; }

        [JsonProperty("createdBy")]
        public IdentitySet CreatedBy { get; set; }

        #region Local DB stored properties
        public string FavoriteColor { get; set; }

        public int Position { get; set; }
        #endregion

        public IList<EducationClass> Classes { get; set; }

        public IList<EducationSchool> Schools { get; set; }

        public string ExternalId => 
            this.PrimaryRole == EducationRole.Teacher && this.Teacher != null
            ? this.Teacher.ExternalId
            : (this.PrimaryRole == EducationRole.Student && this.Student != null
                ? this.Student.ExternalId
                : (this.Teacher != null
                    ? this.Teacher.ExternalId
                    : (this.Student != null ? this.Student.ExternalId 
                        : null)));
    }
}