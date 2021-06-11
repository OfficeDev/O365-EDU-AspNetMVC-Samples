/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace EDUGraphAPI
{
    public static class MSGraphExtension
    {
        public static async Task<Conversation[]> GetAllAsync(this IGroupConversationsCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        public static async Task<DriveItem[]> GetAllAsync(this IDriveItemChildrenCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        public static async Task<DirectoryRole[]> GetAllAsync(this IGraphServiceDirectoryRolesCollectionRequest request)
        {
            var collectionPage = await request.GetAsync();
            return await GetAllAsync(collectionPage);
        }

        public static async Task<bool> AnyAsync(this IDirectoryRoleMembersCollectionWithReferencesPage members, Func<DirectoryObject, bool> predicate)
        {
            return await AnyAsync<DirectoryObject>(members, predicate);
        }

        private static async Task<TItem[]> GetAllAsync<TItem>(ICollectionPage<TItem> collectionPage)
        {
            var list = new List<TItem>();

            dynamic page = collectionPage;
            while (true)
            {
                list.AddRange(page.CurrentPage);
                if (page.NextPageRequest == null) break;
                page = await page.NextPageRequest.GetAsync();
            }

            return list.ToArray();
        }

        private static async Task<bool> AnyAsync<TItem>(ICollectionPage<TItem> collectionPage, Func<TItem, bool> predicate)
        {
            dynamic page = collectionPage;
            while (true)
            {
                if (Enumerable.Any(page.CurrentPage, predicate)) return true;
                if (page.NextPageRequest == null) return false;
                page = await page.NextPageRequest.GetAsync();
            }
        }
    }
}
