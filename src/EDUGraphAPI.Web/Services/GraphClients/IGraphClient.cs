/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Threading.Tasks;
using EDUGraphAPI.Web.Models;

namespace EDUGraphAPI.Web.Services.GraphClients
{
    public interface IGraphClient
    {
        Task<UserInfo> GetCurrentUserAsync();

        Task<TenantInfo> GetTenantAsync(string tenantId);
    }
}