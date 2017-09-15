/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class IdentitySet
    {
        [JsonProperty("application")]
        public Identity Application { get; set; }

        [JsonProperty("device")]
        public Identity Device { get; set; }

        [JsonProperty("user")]
        public Identity User { get; set; }
    }
}