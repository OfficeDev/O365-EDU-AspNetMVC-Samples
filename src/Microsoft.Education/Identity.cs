/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
 using Newtonsoft.Json;

namespace Microsoft.Education
{
    public struct Identity
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}