/*
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
 *   * See LICENSE in the project root for license information.
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using EDUGraphAPI.Data;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace EDUGraphAPI.Models
{
    public class AdalTokenCache : TokenCache
    {
        private static readonly string MachinKeyProtectPurpose = "ADALCache";

        private string userId;

        public AdalTokenCache(string signedInUserId)
        {
            this.userId = signedInUserId;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            GetCahceAndDeserialize();
        }

        public override void Clear()
        {
            base.Clear();
            ClearUserTokenCache(userId);
        }

        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            GetCahceAndDeserialize();
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                SerializeAndUpdateCache();
                this.HasStateChanged = false;
            }
        }


        private void GetCahceAndDeserialize()
        {
            var cacheBits = GetUserTokenCache(userId);
            if (cacheBits != null)
            {
                try
                {
                    var data = MachineKey.Unprotect(cacheBits, MachinKeyProtectPurpose);
                    this.DeserializeAdalV3(data);
                }
                catch { }
            }
        }

        private void SerializeAndUpdateCache()
        {
            var cacheBits = MachineKey.Protect(this.SerializeAdalV3(), MachinKeyProtectPurpose);
            UpdateUserTokenCache(userId, cacheBits);
        }


        private byte[] GetUserTokenCache(string userId)
        {
            using (var db = new ApplicationDbContext())
            {
                var cache = GetUserTokenCache(db, userId);
                return cache?.cacheBits;
            }
        }

        private void UpdateUserTokenCache(string userId, byte[] cacheBits)
        {
            using (var db = new ApplicationDbContext())
            {
                var cache = GetUserTokenCache(db, userId);
                if (cache == null)
                {
                    cache = new UserTokenCache { webUserUniqueId = userId };
                    db.UserTokenCacheList.Add(cache);
                }

                cache.cacheBits = cacheBits;
                cache.LastWrite = DateTime.UtcNow;

                db.SaveChanges();
            }
        }

        private UserTokenCache GetUserTokenCache(ApplicationDbContext db, string userId)
        {
            return db.UserTokenCacheList
                   .OrderByDescending(i => i.LastWrite)
                   .FirstOrDefault(c => c.webUserUniqueId == userId);
        }

        private void ClearUserTokenCache(string userId)
        {
            using (var db = new ApplicationDbContext())
            {
                var cacheEntries = db.UserTokenCacheList
                    .Where(c => c.webUserUniqueId == userId)
                    .ToArray();
                db.UserTokenCacheList.RemoveRange(cacheEntries);
                db.SaveChanges();
            }
        }

        public static async Task ClearUserTokenCacheAsync()
        {
            using (var db = new ApplicationDbContext())
            {
                var cacheEntries = db.UserTokenCacheList
                    .ToArray();
                db.UserTokenCacheList.RemoveRange(cacheEntries);
                await db.SaveChangesAsync();
            }
        }
    }
}
