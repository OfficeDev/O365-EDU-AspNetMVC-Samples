/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Newtonsoft.Json;

namespace EDUGraphAPI.DataSync
{
    /// <summary>
    /// An instance of the class represents a user in Office 365 Education.
    /// </summary>
    public class User
    {
        public User()
        {
        }

        public string Id { get; set; }

        public string JobTitle { get; set; }

        public string Department { get; set; }

        public string MobilePhone { get; set; }
    }
}