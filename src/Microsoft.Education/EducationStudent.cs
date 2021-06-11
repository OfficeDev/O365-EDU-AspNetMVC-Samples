/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class EducationStudent
    {
        [JsonProperty("graduationYear")]
        public string GraduationYear { get; set; }

        [JsonProperty("grade")]
        public string Grade { get; set; }

        [JsonProperty("birthDate")]
        public DateTimeOffset BirthDate { get; set; }

        [JsonProperty("gender")]
        public EducationGender Gender { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("studentNumber")]
        public string StudentNumber { get; set; }
    }
}