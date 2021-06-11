/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace EDUGraphAPI.DifferentialQuery
{
    public class DeltaResult<TEntity> where TEntity : class
    {
        [JsonProperty("@odata.deltaLink")]
        public string DeltaLink { get; set; }

        [JsonProperty("@odata.nextLink")]
        public string NextLink { get; set; }

        [JsonProperty("value")]
        public Delta<TEntity>[] Items { get; set; }
    }
}