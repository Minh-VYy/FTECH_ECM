using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.UserAccount
{
    public class UserAccountAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "UserAccount"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "UserAccount_default",
                url: "UserAccount/{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Dashboard", id = UrlParameter.Optional }
            );
        }
    }
}
