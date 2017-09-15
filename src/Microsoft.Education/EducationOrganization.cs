/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public abstract class EducationOrganization : GraphEntity
    {
        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("externalSource")]
        public EducationExternalSource ExternalSource { get; set; }
    }
}