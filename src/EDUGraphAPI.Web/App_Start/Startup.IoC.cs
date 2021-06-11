/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using EDUGraphAPI.Data;
using EDUGraphAPI.Web.Controllers;
using EDUGraphAPI.Web.Services;
using Microsoft.AspNet.Identity.Owin;
using Owin;

namespace EDUGraphAPI.Web
{
    public partial class Startup
    {
        public void ConfigureIoC(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            Register(builder);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // OWIN MVC SETUP:
            // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();
        }

        private void Register(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(HomeController).Assembly);
            builder.RegisterType<ApplicationService>().AsSelf().InstancePerRequest();
            builder.RegisterType<SchoolsService>().AsSelf().InstancePerRequest();
            builder.RegisterType<DemoHelperService>().AsSelf().InstancePerRequest();
            builder.RegisterType<CookieService>().AsSelf().InstancePerRequest();

            // The following types are registered in Startup.Auth.Identity.cs
            // app.CreatePerOwinContext(ApplicationDbContext.Create);
            // app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            // app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            builder.Register(c => HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>());
            builder.Register(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>());
            builder.Register(c => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>());
        }
    }
}