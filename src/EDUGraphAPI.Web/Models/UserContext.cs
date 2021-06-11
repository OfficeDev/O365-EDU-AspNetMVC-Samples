/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Web;
using EDUGraphAPI.Data;
using Microsoft.AspNet.Identity;

namespace EDUGraphAPI.Web.Models
{
    /// <summary>
    /// Context for the logged-in user
    /// </summary>
    public class UserContext
    {
        public UserContext(HttpContext httpContext, ApplicationUser user)
        {
            this.HttpContext = httpContext;
            this.User = user;
        }

        /// <summary>
        /// The HttpContext
        /// </summary>
        public HttpContext HttpContext { get; private set; }

        /// <summary>
        /// The underlying user
        /// </summary>
        /// For unlinked Office 365 user, the property is null.
        /// <remarks>
        public ApplicationUser User { get; private set; }

        /// <summary>
        /// Display name of the logged-in user
        /// </summary>
        public string UserDisplayName => HttpContext.User.Identity.GetUserName();

        /// <summary>
        /// Is the logged-in account a local account
        /// </summary>
        public bool IsLocalAccount => User != null && User.Id == HttpContext.User.Identity.GetUserId();

        /// <summary>
        /// Is the logged-in acount an Office 365 account
        /// </summary>
        public bool IsO365Account => !IsLocalAccount;

        /// <summary>
        /// Are the local account and Office 365 account linked
        /// </summary>
        public bool AreAccountsLinked => User != null && User.O365UserId.IsNotNullAndEmpty();

        /// <summary>
        /// Is the logged-in user a faculty (teacher)
        /// </summary>
        public bool IsFaculty => HttpContext.User.IsInRole(Constants.Roles.Faculty);

        /// <summary>
        /// Is the logged-in user a student 
        /// </summary>
        public bool IsStudent => HttpContext.User.IsInRole(Constants.Roles.Student);

        /// <summary>
        /// Is the logged-in user an administrator
        /// </summary>
        public bool IsAdmin => HttpContext.User.IsInRole(Constants.Roles.Admin);

        /// <summary>
        /// Is the use's tenant consented by a admin
        /// </summary>
        public bool IsTenantConsented => User != null && User.Organization != null && User.Organization.IsAdminConsented;

        /// <summary>
        /// Get the email of the Office 365 account
        /// </summary>
        /// <remarks>
        /// For unlinked local account, the value is null
        /// </remarks>
        public string UserO365Email => AreAccountsLinked
            ? User.O365Email
            : (IsO365Account ? HttpContext.User.Identity.Name : null);
    }
}