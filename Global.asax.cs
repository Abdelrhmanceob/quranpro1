using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Helpers;
using System.Data.Entity;
using Maeen1_New.Models;
using Npgsql;

namespace Maeen1_New
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register Npgsql provider for Entity Framework
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"));
            // Use an app-specific anti-forgery cookie name to avoid conflicts
            // with other ASP.NET apps running on localhost.
            AntiForgeryConfig.CookieName = "Maeen1New.AntiForgery";
            // Allow antiforgery cookie on http://localhost (no SSL); avoids missing cookie on POST.
            AntiForgeryConfig.RequireSsl = false;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer<Maeen1_NewDbContext>(null);
        }
    }
}
