/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDUGraphAPI.Web.Models;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.Services.GraphClients
{
    public class MSGraphClient : IGraphClient
    {
        private GraphServiceClient graphServiceClient;

        public MSGraphClient(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public async Task<UserInfo> GetCurrentUserAsync()
        {
            var me = await graphServiceClient.Me.Request()
                .Select("id,givenName,surname,userPrincipalName,assignedLicenses")
                .GetAsync();

            return new UserInfo
            {
                Id = me.Id,
                GivenName = me.GivenName,
                Surname = me.Surname,
                Mail = me.Mail,
                UserPrincipalName = me.UserPrincipalName,
                Roles = await GetRolesAsync(me)
            };
        }

        public async Task<TenantInfo> GetTenantAsync(string tenantId)
        {
            var tenant = await graphServiceClient.Organization[tenantId].Request().GetAsync();
            return new TenantInfo
            {
                Id = tenant.Id,
                Name = tenant.DisplayName
            };
        }

        public async Task<string[]> GetRolesAsync(User user)
        {
            var roles = new List<string>();
            var directoryAdminRole = await GetDirectoryAdminRoleAsync();

            if (await directoryAdminRole.Members.AnyAsync(i => i.Id == user.Id))
                roles.Add(Constants.Roles.Admin);
            if (roles.Count == 0)
            {
                var educationServiceClient = Microsoft.Education.EducationServiceClient.GetEducationServiceClient(this.graphServiceClient.AuthenticationProvider);
                var me = await educationServiceClient.GetUserAsync();
                if (me.PrimaryRole == Microsoft.Education.EducationRole.Student)
                    roles.Add(Constants.Roles.Student);
                if (me.PrimaryRole == Microsoft.Education.EducationRole.Teacher)
                    roles.Add(Constants.Roles.Faculty);
            }
            return roles.ToArray();
        }

        private async Task<DirectoryRole> GetDirectoryAdminRoleAsync()
        {
            var roles = await graphServiceClient.DirectoryRoles.Request()
                .Expand(i => i.Members)
                .GetAllAsync();
            return roles
                .Where(i => i.DisplayName == Constants.AADCompanyAdminRoleName)
                .FirstOrDefault();
        }
    }
}