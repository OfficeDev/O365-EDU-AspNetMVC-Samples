/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using Newtonsoft.Json;

namespace Microsoft.Education
{
    public class User : GraphEntity
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty("accountEnabled")]
        public bool? AccountEnabled { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("preferredLanguage")]
        public string PreferredLanguage { get; set; }

        [JsonProperty("refreshTokensValidFromDateTime")]
        public DateTimeOffset? RefreshTokensValidFromDateTime { get; set; }

        [JsonProperty("showInAddressList")]
        public bool? ShowInAddressList { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }

    }
}