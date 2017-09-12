/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Web.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDUGraphAPI.Web.Services.GraphClients
{
    public class AADGraphClient : IGraphClient
    {
        private ActiveDirectoryClient activeDirectoryClient;

        public AADGraphClient(ActiveDirectoryClient activeDirectoryClient)
        {
            this.activeDirectoryClient = activeDirectoryClient;
        }

        public async Task<UserInfo> GetCurrentUserAsync()
        {
            var me = await activeDirectoryClient.Me.ExecuteAsync();
            return new UserInfo
            {
                Id = me.ObjectId,
                GivenName = me.GivenName,
                Surname = me.Surname,
                Mail = me.Mail,
                UserPrincipalName = me.UserPrincipalName,
                Roles = await GetRolesAsync(me)
            };
        }

        public async Task<TenantInfo> GetTenantAsync(string tenantId)
        {
            var tenant = await activeDirectoryClient.TenantDetails
                .Where(i => i.ObjectId == tenantId)
                .ExecuteSingleAsync();
            return new TenantInfo
            {
                Id = tenant.ObjectId,
                Name = tenant.DisplayName
            };
        }

        private async Task<string[]> GetRolesAsync(IUser user)
        {
            var roles = new List<string>();
            var directoryAdminRole = await GetDirectoryAdminRoleAsync();
            if (await directoryAdminRole.Members.AnyAsync(i => i.ObjectId == user.ObjectId))
                roles.Add(Constants.Roles.Admin);
            if (user.AssignedLicenses.Any(i => i.SkuId == Constants.O365ProductLicenses.Faculty || i.SkuId == Constants.O365ProductLicenses.FacultyPro))
                roles.Add(Constants.Roles.Faculty);
            if (user.AssignedLicenses.Any(i => i.SkuId == Constants.O365ProductLicenses.Student || i.SkuId == Constants.O365ProductLicenses.StudentPro))
                roles.Add(Constants.Roles.Student);
            return roles.ToArray();
        }

        private async Task<IDirectoryRole> GetDirectoryAdminRoleAsync()
        {
            var roles = await activeDirectoryClient.DirectoryRoles
               .Expand(i => i.Members)
               .ExecuteAllAsync();
            return roles
                .Where(i => i.DisplayName == Constants.AADCompanyAdminRoleName)
                .FirstOrDefault();
        }
    }
}