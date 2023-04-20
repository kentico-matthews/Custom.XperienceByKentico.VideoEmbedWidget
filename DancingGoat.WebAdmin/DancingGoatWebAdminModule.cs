using DancingGoat.WebAdmin;

using Kentico.Xperience.Admin.Base;

[assembly: CMS.AssemblyDiscoverable]
[assembly: CMS.RegisterModule(typeof(DancingGoatWebAdminModule))]

// Adds a new application category 
[assembly: UICategory(DancingGoatWebAdminModule.CUSTOM_CATEGORY, "Custom", Icons.CustomElement, 100)]

namespace DancingGoat.WebAdmin
{
    internal class DancingGoatWebAdminModule : AdminModule
    {
        public const string CUSTOM_CATEGORY = "dancinggoat.web.admin.category";

        public DancingGoatWebAdminModule()
            : base("DancingGoat.Web.Admin")
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            // Makes the module accessible to the admin UI
            RegisterClientModule("dancinggoat", "web-admin");
        }
    }
}
