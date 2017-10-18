/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Data;
using EDUGraphAPI.DifferentialQuery;
using EDUGraphAPI.Web.Infrastructure;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EDUGraphAPI.DataSync
{
    public delegate Task<string> GetTenantAccessTokenAsyncDelegate(string tenantId);

    /// <summary>
    /// An instance of the class handles syncing users in local database with differential query.
    /// </summary>
    public class UserSyncService
    {
        private static readonly string UsersQuery = "users";

        private TextWriter log;
        private ApplicationDbContext dbContext;
        private GetTenantAccessTokenAsyncDelegate getTenantAccessTokenAsync;

        public UserSyncService(ApplicationDbContext dbContext, GetTenantAccessTokenAsyncDelegate getTenantAccessTokenAsync, TextWriter log)
        {
            this.log = log;
            this.dbContext = dbContext;
            this.getTenantAccessTokenAsync = getTenantAccessTokenAsync;
        }

        public async Task SyncAsync()
        {
            try
            {
                await SyncAsyncCore();
            }
            catch (Exception ex)
            {
                await WriteLogAsync("Faild to sync users. Error: " + ex.Message);
            }
        }

        private async Task SyncAsyncCore()
        {
            var consentedOrganizations = await dbContext.Organizations
                .Where(i => i.IsAdminConsented)
                .ToArrayAsync();
            if (!consentedOrganizations.Any())
            {
                await WriteLogAsync($"No consented organization found. This sync was canceled.");
                return;
            }

            foreach (var org in consentedOrganizations)
            {
                try
                {
                    await SyncOrganizationUsersAsync(org);
                    await dbContext.SaveChangesAsync();
                    await WriteLogAsync($"All the changes were saved.");
                }
                catch (Exception ex)
                {
                    await WriteLogAsync($"Failed to sync users of {org.Name}. Error: {ex.Message}");
                }
            }
        }

        private async Task SyncOrganizationUsersAsync(Organization org)
        {
            await WriteLogAsync($"Starting to sync users for the {org.Name} organization.");

            var dataSyncRecord = await GetOrCreateDataSyncRecord(org.TenantId, UsersQuery);

            await WriteLogAsync($"Send Differential Query.");
            if (dataSyncRecord.Id == 0)
                await WriteLogAsync("First time executing differential query; all items will return.");
            var differentialQueryService = new DifferentialQueryService(() => getTenantAccessTokenAsync(org.TenantId));

            var result = await differentialQueryService.QueryAsync<User>(dataSyncRecord.DeltaLink);
            await WriteLogAsync($"Get {result.Items.Length} users.");

            foreach (var differentialUser in result.Items)
                await UpdateUserAsync(differentialUser);

            dataSyncRecord.DeltaLink = result.DeltaLink;
            dataSyncRecord.Updated = DateTime.UtcNow;
        }

        private async Task<DataSyncRecord> GetOrCreateDataSyncRecord(string tenantId, string query)
        {
            var record = await dbContext.DataSyncRecords
                .Where(i => i.TenantId == tenantId)
                .Where(i => i.Query == query)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                var url = $"{Constants.Resources.MSGraph}/{Constants.Resources.MSGraphVersion}/{query}/delta";
                record = new DataSyncRecord
                {
                    Query = query,
                    TenantId = tenantId,
                    DeltaLink = url
                };
                dbContext.DataSyncRecords.Add(record);
            }
            return record;
        }

        private async Task UpdateUserAsync(Delta<User> differentialUser)
        {
            var user = await dbContext.Users
                .Where(i => i.O365UserId == differentialUser.Entity.Id)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                await WriteLogAsync("Skipping updating user {0} who does not exist in the local database.", differentialUser.Entity.Id);
                return;
            }

            if (!differentialUser.IsRemoved)
            {
                if (differentialUser.ModifiedPropertyNames.Any())
                {
                    SimpleMapper.Map(differentialUser.Entity, user, differentialUser.ModifiedPropertyNames);
                    await WriteLogAsync("Updated user {0}. Changed properties: {1}", user.O365Email, string.Join(", ", differentialUser.ModifiedPropertyNames));
                }
                else
                    await WriteLogAsync("Skipped updating user {0}, because the properties that changed are not included in the local database.", differentialUser.Entity.Id);
            }
            else
            {
                dbContext.Users.Remove(user);
                await WriteLogAsync($"Deleted user {user.Email}.");
                return;
            }
        }

        private async Task WriteLogAsync(string message)
        {
            await log.WriteAsync($"[{DateTime.UtcNow}][SyncData] ");
            await log.WriteLineAsync(message);
        }

        private async Task WriteLogAsync(string message, params object[] args)
        {
            await WriteLogAsync(string.Format(message, args));
        }
    }
}