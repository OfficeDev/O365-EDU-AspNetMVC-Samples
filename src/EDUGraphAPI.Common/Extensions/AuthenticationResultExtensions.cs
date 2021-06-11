/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Utils;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace EDUGraphAPI
{
    public static class AuthenticationResultExtensions
    {
        public static GraphServiceClient CreateGraphServiceClient(this AuthenticationResult authenticationResult)
        {
            return AuthenticationHelper.GetGraphServiceClient(authenticationResult);
        }
    }
}