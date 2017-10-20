/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDUGraphAPI.Data
{
    /// <summary>
    /// An instance of the class represents a user
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }
        
        public string O365UserId { get; set; }

        public string O365Email { get; set; }

        public string JobTitle { get; set; }

        public string Department { get; set; }

        public string MobilePhone { get; set; }

        public string FavoriteColor { get; set; }

        public string FullName => FirstName + " " + LastName;

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            if (Organization != null)
                userIdentity.AddTenantIdClaim(Organization.TenantId);

            if (O365UserId != null)
                userIdentity.AddObjectIdentifierClaim(O365UserId);

            if (FirstName.IsNotNullAndEmpty())
                userIdentity.AddClaim(ClaimTypes.GivenName, FirstName);
            if (LastName.IsNotNullAndEmpty())
                userIdentity.AddClaim(ClaimTypes.Surname, LastName);

            var roles = await manager.GetRolesAsync(Id);
            foreach (var role in roles) userIdentity.AddClaim(ClaimTypes.Role, role);

            return userIdentity;
        }
    }
}