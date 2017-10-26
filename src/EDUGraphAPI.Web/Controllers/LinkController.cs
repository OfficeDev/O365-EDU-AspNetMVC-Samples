/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using EDUGraphAPI.Data;
using EDUGraphAPI.Utils;
using EDUGraphAPI.Web.Infrastructure;
using EDUGraphAPI.Web.Models;
using EDUGraphAPI.Web.Properties;
using EDUGraphAPI.Web.Services;
using EDUGraphAPI.Web.Services.GraphClients;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace EDUGraphAPI.Web.Controllers
{
    [EduAuthorize, HandleAdalException]
    public class LinkController : Controller
    {
        static readonly string StateKey = typeof(LinkController).Name + "State";
        static readonly string UsernameCookie = Constants.UsernameCookie;
        static readonly string EmailCookie = Constants.EmailCookie;

        private ApplicationService applicationService;
        private ApplicationSignInManager signInManager;
        private ApplicationUserManager userManager;
        private CookieService cookieServie;

        public LinkController(ApplicationService applicationService, ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager,CookieService cookieServie)
        {
            this.applicationService = applicationService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.cookieServie = cookieServie;
        }

        //
        // GET: /Link/Index
        public async Task<ActionResult> Index()
        {
            var userContext = await applicationService.GetUserContextAsync();
            if (userContext.IsO365Account && !userContext.AreAccountsLinked)
            {
                var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();
                var graphClient = new MSGraphClient(graphServiceClient);
                var user = await graphClient.GetCurrentUserAsync();
                var email = user.Mail ?? user.UserPrincipalName;

                if (await userManager.Users.AnyAsync(i => i.Email == email))
                    ViewBag.LocalAccountExistedMessage = $"There is a local account: {email} matching your O365 account.";
            }
            return View(userContext);
        }

        //
        // POST: /Link/LoginO365
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult LoginO365()
        {
            var state = Guid.NewGuid().ToString();
            TempData[StateKey] = state;

            var redirectUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("ProcessCode");
            var authorizationUrl = AuthorizationHelper.GetUrl(redirectUrl, state, Constants.Resources.MSGraph, AuthorizationHelper.Prompt.Login);
            return new RedirectResult(authorizationUrl);
        }

        //
        // GET: /Link/ProcessCode
        public async Task<ActionResult> ProcessCode(string code, string error, string error_description, string resource, string state)
        {
            if (TempData[StateKey] as string != state)
            {
                TempData["Error"] = "Invalid operation. Please try again";
                return RedirectToAction("Index");
            }

            var authResult = await AuthenticationHelper.GetAuthenticationResultAsync(code);
            var tenantId = authResult.TenantId;
            var graphServiceClient = authResult.CreateGraphServiceClient();

            IGraphClient graphClient = new MSGraphClient(graphServiceClient);
            var user = await graphClient.GetCurrentUserAsync();
            var tenant = await graphClient.GetTenantAsync(tenantId);

            var isAccountLinked = await applicationService.IsO365AccountLinkedAsync(user.Id);
            if (isAccountLinked)
            {
                TempData["Error"] = $"Failed to link accounts. The Office 365 account '{ user.Mail ?? user.UserPrincipalName}' is already linked to another local account.";
                return RedirectToAction("Index");
            }

            // Link the AAD User with local user.
            var localUser = await applicationService.GetCurrentUserAsync();
            await applicationService.UpdateLocalUserAsync(localUser, user, tenant);

            // Re-sign in user. Required claims (roles, tenent id and user object id) will be added to current user's identity.
            await signInManager.SignInAsync(localUser, isPersistent: false, rememberBrowser: false);

            TempData["Message"] = Resources.LinkO365AccountSuccess;
            TempData[HandleAdalExceptionAttribute.ChallengeImmediatelyTempDataKey] = true;
            SetCookiesForO365User(user.GivenName + " " + user.Surname, user.UserPrincipalName);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Link/LoginLocal
        public async Task<ActionResult> LoginLocal(LoginViewModel model)
        {
            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();
            IGraphClient graphClient = new MSGraphClient(graphServiceClient);
            var user = await graphClient.GetCurrentUserAsync();
            var localUser = userManager.FindByEmail(user.Mail);
            if (localUser == null)
            {
                foreach (var modelValue in ModelState.Values)
                {
                    modelValue.Errors.Clear();
                }
                return View(model);
            }
            var tenantId = User.GetTenantId();            
            if (localUser.O365UserId.IsNotNullAndEmpty())
            {
                ModelState.AddModelError("Email", "The local account has already been linked to another Office 365 account.");
                return View(model);
            }

            var tenant = await graphClient.GetTenantAsync(tenantId);

            await applicationService.UpdateLocalUserAsync(localUser, user, tenant);
            SetCookiesForO365User(user.GivenName + " " + user.Surname, user.Mail);
            TempData["Message"] = Resources.LinkO365AccountSuccess;
            TempData[HandleAdalExceptionAttribute.ChallengeImmediatelyTempDataKey] = true;

            return RedirectToAction("Index", "Schools");
        }
        //
        // POST: /Link/LoginLocalPost
        [HttpPost, ActionName("LoginLocal"), ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginLocalPost(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var localUser = userManager.FindByEmail(model.Email);
            if (localUser == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            if (localUser.O365UserId.IsNotNullAndEmpty())
            {
                ModelState.AddModelError("Email", "The local account has already been linked to another Office 365 account.");
                return View(model);
            }
            if (!await userManager.CheckPasswordAsync(localUser, model.Password))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var tenantId = User.GetTenantId();
            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();

            IGraphClient graphClient = new MSGraphClient(graphServiceClient);
            var user = await graphClient.GetCurrentUserAsync();
            var tenant = await graphClient.GetTenantAsync(tenantId);

            await applicationService.UpdateLocalUserAsync(localUser, user, tenant);
            SetCookiesForO365User(user.GivenName + " " + user.Surname, user.Mail);

            return RedirectToAction("Index", "Schools");
        }



        //
        // GET: /Link/CreateLocalAccount
        public async Task<ActionResult> CreateLocalAccount()
        {
            var client = await AuthenticationHelper.GetGraphServiceClientAsync();

            var aadUser = await client.Me.Request().GetAsync();

            var viewModel = new EducationRegisterViewModel
            {
                FirstName = aadUser.GivenName,
                LastName = aadUser.Surname,
                Email = aadUser.Mail ?? aadUser.UserPrincipalName,
                FavoriteColors = Constants.FavoriteColors
            };

            return View(viewModel);
        }

        //
        // POST: /Link/CreateLocalAccount
        [HttpPost, ActionName("CreateLocalAccount"), ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateLocalAccountPost(EducationRegisterViewModel model)
        {
            var tenantId = User.GetTenantId();
            var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();

            IGraphClient graphClient = new MSGraphClient(graphServiceClient);
            var user = await graphClient.GetCurrentUserAsync();
            var tenant = await graphClient.GetTenantAsync(tenantId);

            model.Email = user.Mail ?? user.UserPrincipalName;
            model.FavoriteColors = Constants.FavoriteColors;

            // Create a new local user
            var localUser = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FavoriteColor = model.FavoriteColor
            };
            var result = await userManager.CreateAsync(localUser);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            // Update the local user
            await applicationService.UpdateLocalUserAsync(localUser, user, tenant);
            SetCookiesForO365User(user.GivenName + " " + user.Surname, user.Mail);
            return RedirectToAction("Index", "Schools");
        }

        //
        // GET: /Link/LoginO365Required
        public ActionResult LoginO365Required(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // GET: /Link/ReLoginO365
        public ActionResult ReLoginO365(string returnUrl)
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
                 new AuthenticationProperties { RedirectUri = returnUrl },
                 OpenIdConnectAuthenticationDefaults.AuthenticationType);
            return null;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private void SetCookiesForO365User(string username, string email)
        {

            Response.Cookies.Add(new HttpCookie(Constants.UsernameCookie, username)
            {
                Expires = DateTime.UtcNow.AddDays(30)
            });
            Response.Cookies.Add(new HttpCookie(Constants.EmailCookie, email)
            {
                Expires = DateTime.UtcNow.AddDays(30)
            });
        }
    }
}