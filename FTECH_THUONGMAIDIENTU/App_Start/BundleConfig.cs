using System.Web;
using System.Web.Optimization;

namespace FTECH_THUONGMAIDIENTU
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // ============ SHARED ADMIN CSS BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/admin/css").Include(
                      "~/Content/css/common/admin-shell.css",
                      "~/Content/css/common/inline-utilities.css",
                      "~/Content/css/common/base.css",
                      "~/Content/css/common/layout.css"));

            // ============ SUPER ADMIN AREA BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/superadmin/css").Include(
                      "~/Content/css/pages/dashboard.css",
                      "~/Content/css/pages/manage-posts.css",
                      "~/Content/css/pages/manage-partners.css",
                      "~/Content/css/pages/manage-accounts.css"));

            // ============ AFFILIATE MANAGER AREA BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/affiliatemanager/css").Include(
                      "~/Content/css/pages/manage-affiliates.css"));

            // ============ CONTENT MANAGER AREA BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/contentmanager/css").Include(
                      "~/Content/css/pages/content-manager.css"));

            // ============ USER ACCOUNT AREA BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/user/css").Include(
                      "~/Content/css/pages/profileManager.css"));

            // ============ PUBLIC PAGES BUNDLES ============
            bundles.Add(new StyleBundle("~/Content/auth/css").Include(
                      "~/Content/css/pages/login.css",
                      "~/Content/css/pages/register.css",
                      "~/Content/css/pages/reset-password.css",
                      "~/Content/css/common/inline-utilities.css"));

            bundles.Add(new StyleBundle("~/Content/public/css").Include(
                      "~/Content/css/pages/trangchu.css",
                      "~/Content/css/pages/product.css",
                      "~/Content/css/pages/reviewModule.css"));
        }
    }
}
