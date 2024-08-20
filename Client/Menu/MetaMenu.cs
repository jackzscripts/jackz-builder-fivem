using System.Collections.Generic;
using System.Drawing;
using ScaleformUI.Menu;

namespace test_project.Client.Menu
{
    public class MetaMenu : MenuAPI.BuilderMenu
    {
        public MetaMenu() : base("Meta", "~q~Builder ~s~> Meta", "Information and methods related to the resource itself", new PointF(40, 0))
        {
            UIMenuItem mVersion = CreateItem($"Version: {ClientMain.Version}", "The current version of this resource");
            mVersion.SetRightBadge(BadgeIcon.INFO);
            
            UIMenuItem mChannel = CreateItem($"Release Channel: {ClientMain.ReleaseChannel}", "");
            // mChannel.SetRightLabel(ClientMain.ReleaseChannel);
            mChannel.SetRightBadge(BadgeIcon.INFO);

            UIMenuItem mUploadLogs = CreateItem("Upload Logs (WIP)",
                "Uploads your diagnostic logs to paste.jackz.me, and copies url to clipboard. Send for support");
            mUploadLogs.Enabled = false;
            
        }

    }
}