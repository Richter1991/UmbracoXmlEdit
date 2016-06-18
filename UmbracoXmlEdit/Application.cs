using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace UmbracoXmlEdit
{
    public class Application : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.MenuRendering += TreeControllerBaseMenuRendering;
        }

        void TreeControllerBaseMenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            bool isContentTree = sender.TreeAlias == "content";
            bool userIsAdmin = sender.Security.CurrentUser.UserType.Alias == "admin";

            // Add custom menu item
            if (isContentTree && userIsAdmin && IsContentNode(e))
            {
                AddMenuItem(e);
            }
        }

        private bool IsContentNode(MenuRenderingEventArgs e)
        {
            bool isRootNode = e.NodeId == "-1";
            bool isRecycleBin = e.NodeId == "-20";

            if (isRootNode || isRecycleBin)
            {
                return false;
            }

            return true;
        }

        private void AddMenuItem(MenuRenderingEventArgs e)
        {
            var menuItem = new MenuItem("xmlEdit", "XmlEdit")
            {
                Icon = "code",
                SeperatorBefore = true,
                AdditionalData = {}
            };
            menuItem.AdditionalData.Add("actionView", "/App_Plugins/UmbracoXmlEdit/BackOffice/content/edit.html");
            e.Menu.Items.Add(menuItem);
        }
    }
}