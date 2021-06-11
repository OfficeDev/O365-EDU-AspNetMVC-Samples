/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using EDUGraphAPI.Data;
using EDUGraphAPI.DataSync;
using EDUGraphAPI.Utils;
using Microsoft.Azure.WebJobs;

namespace EDUGraphAPI.SyncData
{
    public class Functions
    {
        private static readonly string CertPath = ConfigurationManager.AppSettings["CertPath"];
        private static readonly string CertPassword = ConfigurationManager.AppSettings["CertPassword"];

        public async static Task SyncUsersAsync([TimerTrigger("0 * * * * *")] TimerInfo timerInfo, TextWriter log)
        {
            var dbContext = new ApplicationDbContext("SyncDataWebJobDefaultConnection");
            var userSyncService = new UserSyncService(dbContext, GetTenantAccessTokenAsync, log);
            await userSyncService.SyncAsync();
        }

        private static Task<string> GetTenantAccessTokenAsync(string tenantId)
        {
            return AuthenticationHelper.GetAppOnlyAccessTokenForDaemonAppAsync(
                Constants.Resources.MSGraph, CertPath, CertPassword, tenantId);
        }
    }
}