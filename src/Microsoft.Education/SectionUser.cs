/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Newtonsoft.Json;

namespace Microsoft.Education.Data
{
    public class SectionUser
    {
        [JsonProperty("mail")]
        public string Email { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType")]
        public string ObjectType { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Grade")]
        public string EducationGrade { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId")]
        public string SchoolId { get; set; }

        public virtual string UserId { get; }

        public int Position { get; set; }

        [JsonProperty("id")]
        public string O365UserId { get; set; }
        public string FavoriteColor { get; set; }
    }
}