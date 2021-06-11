/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EDUGraphAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public ApplicationDbContext(string nameOrConnectionString = "DefaultConnection")
            : base(nameOrConnectionString, throwIfV1Schema: false)
        {
        }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<UserTokenCache> UserTokenCacheList { get; set; }

        public DbSet<DataSyncRecord> DataSyncRecords { get; set; }

        public DbSet<ClassroomSeatingArrangements> ClassroomSeatingArrangements { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}