/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDUGraphAPI
{
    public static class AADGraphExtensions
    {
        public static async Task<T[]> ExecuteAllAsync<T>(this IPagedCollection<T> collection)
        {
            var list = new List<T>();

            var c = collection;
            while (true)
            {
                list.AddRange(c.CurrentPage);
                if (c.MorePagesAvailable) c = await c.GetNextPageAsync();
                else break;
            }
            return list.ToArray();
        }

        public static async Task<T[]> ExecuteAllAsync<T>(this IReadOnlyQueryableSet<T> set)
        {
            var pagedCollection = await set.ExecuteAsync();
            return await ExecuteAllAsync(pagedCollection);
        }

        public static async Task<bool> AnyAsync<T>(this IPagedCollection<T> collection, Func<T, bool> predicate)
        {
            var c = collection;
            while (true)
            {
                if (c.CurrentPage.Any(predicate)) return true;
                if (c.MorePagesAvailable) c = await c.GetNextPageAsync();
                else break;
            }
            return false;
        }
    }
}