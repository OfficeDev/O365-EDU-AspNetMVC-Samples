/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class EducationTeacher 
    {
        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("teacherNumber")]
        public string TeacherNumber { get; set; }
    }
}