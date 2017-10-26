/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class PhysicalAddress
    {
        [JsonProperty("type")]
        public PhysicalAddressType Type { get; set; }

        [JsonProperty("postOfficeBox")]
        public string PostOfficeBox { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("countryOrRegion")]
        public string CountryOrRegion { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }
}