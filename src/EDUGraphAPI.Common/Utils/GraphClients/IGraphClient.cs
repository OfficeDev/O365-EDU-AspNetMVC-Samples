/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Web.Models;
using System.Threading.Tasks;

namespace EDUGraphAPI.Web.Services.GraphClients
{
    public interface IGraphClient
    {
        Task<UserInfo> GetCurrentUserAsync();

        Task<TenantInfo> GetTenantAsync(string tenantId);
    }
}