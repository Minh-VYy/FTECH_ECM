using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FTECH_THUONGMAIDIENTU
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ProductBySlug",
                url: "product/{slug}.html",
                defaults: new { controller = "Product", action = "Index" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "LegacyProduct",
                url: "product.html",
                defaults: new { controller = "Product", action = "Index" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "LegacyReview",
                url: "reviewModule.html",
                defaults: new { controller = "Review", action = "Index" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "LegacyHome",
                url: "trangchu.html",
                defaults: new { controller = "Home", action = "Index" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "LegacyLogin",
                url: "login.html",
                defaults: new { controller = "Account", action = "Login" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "LegacyRegister",
                url: "register.html",
                defaults: new { controller = "Account", action = "Register" },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Controllers" }
            );
        }
    }
}
