using System;
using System.Linq;
using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Infrastructure
{
    public static class RoleKeys
    {
        public const string SuperAdmin = "super-admin";
        public const string ContentManager = "content-manager";
        public const string AffiliateManager = "affiliate-manager";
        public const string UserAccountManager = "user-account-manager";
        public const string Customer = "customer";
    }

    public static class RoleNames
    {
        public const string SuperAdmin = "Super Admin";
        public const string ContentManager = "Content Manager";
        public const string AffiliateManager = "Affiliate Manager";
        public const string UserAccountManager = "User Account Manager";
        public const string Customer = "customer";
    }

    public class SessionRoleAuthorizeAttribute : ActionFilterAttribute
    {
        public string SessionKey { get; set; }

        public string AllowedRolesCsv { get; set; }

        public string LoginUrl { get; set; } = "/Account/Login";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext?.Session;
            var role = session?[SessionKey] as string;

            if (string.IsNullOrWhiteSpace(role))
            {
                filterContext.Result = new RedirectResult(LoginUrl);
                return;
            }

            if (!string.IsNullOrWhiteSpace(AllowedRolesCsv))
            {
                var allowedRoles = AllowedRolesCsv
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .ToArray();

                if (allowedRoles.Length > 0 && !allowedRoles.Any(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase)))
                {
                    filterContext.Result = new RedirectResult(LoginUrl);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}