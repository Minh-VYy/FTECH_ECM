using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.ContentManager
{
    public class ContentManagerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ContentManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "ContentManager_default",
                url: "ContentManager/{controller}/{action}/{id}",
                defaults: new { controller = "Post", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "FTECH_THUONGMAIDIENTU.Areas.ContentManager.Controllers" }
            );
        }
    }
}
