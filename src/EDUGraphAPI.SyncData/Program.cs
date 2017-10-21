/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using Microsoft.Azure.WebJobs;

namespace EDUGraphAPI.SyncData
{
    class Program
    {
        static void Main()
        {
            // Uncomment the following line of code to debug quickly (no need to wait for the scheduled time to arrive).
            Functions.SyncUsersAsync(null, System.Console.Out).Wait();
            Console.ReadLine();

            //JobHostConfiguration config = new JobHostConfiguration();
            //config.UseTimers();

            //var host = new JobHost(config);
            //host.RunAndBlock();
        }
    }
}