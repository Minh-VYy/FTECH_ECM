using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager
{
    public class AffiliateManagerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "AffiliateManager"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "AffiliateManager_default",
                url: "AffiliateManager/{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
