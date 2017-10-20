/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EDUGraphAPI.DifferentialQuery
{
    /// <summary>
    /// An instance of the class handles building request, sending it to the service endpoint, and processing the responses.
    /// </summary>
    public class DifferentialQueryService
    {
        private Func<Task<string>> accessTokenGetter;

        public DifferentialQueryService(Func<Task<string>> accessTokenGetter)
        {
            this.accessTokenGetter = accessTokenGetter;
        }

        public async Task<DeltaResult<TEntity>> QueryAsync<TEntity>(string url) where TEntity : class, new()
        {
            var items = new List<Delta<TEntity>>();

            var deltaLink = url;
            var nextLink = url;

            while (true)
            {
                var json = await HttpGetAsync(nextLink);
                var result = JsonConvert.DeserializeObject<DeltaResult<TEntity>>(json, new DeltaJsonConverter<TEntity>());
                items.AddRange(result.Items);

                deltaLink = result.DeltaLink;
                nextLink = result.NextLink;

                if (nextLink.IsNullOrEmpty()) break;
            }

            return new DeltaResult<TEntity>
            {
                DeltaLink = deltaLink,
                Items = items.ToArray()
            };
        }

        private async Task<string> HttpGetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", await accessTokenGetter());
                // Equivalent not supported YET by MSGraph
                // client.DefaultRequestHeaders.Add("ocp-aad-dq-include-only-changed-properties", "true");

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}