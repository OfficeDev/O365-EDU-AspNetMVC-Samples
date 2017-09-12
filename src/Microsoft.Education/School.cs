/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace Microsoft.Education.Data
{
    public class School
    {
        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SyncSource_SchoolId")]
        public string SchoolId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SchoolPrincipalName")]
        public string PrincipalName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SchoolPrincipalEmail")]
        public string Email { get; set; }


        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_HighestGrade")]
        public string HighestGrade { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_LowestGrade")]
        public string LowestGrade { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_SchoolNumber")]
        public string SchoolNumber { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Phone")]
        public string Phone { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Zip")]
        public string Zip { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_State")]
        public string State { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_City")]
        public string City { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_Address")]
        public string Address { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_AnchorId")]
        public string AnchorId { get; set; }

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_StateId")]
        public string StateId { get; set; }

        public string EducationGrade => $"{LowestGrade} - {HighestGrade}";

        [JsonProperty("extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType")]
        public string EducationObjectType { get; set; }
    }
}