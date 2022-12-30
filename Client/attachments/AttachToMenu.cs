using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.lib;
using MenuAPI;

namespace jackz_builder.Client.JackzBuilder
{
    public class AttachToMenu
    {
        private Menu menu;
        private MenuItem menuEntry;
        private Attachment attachment;
        private Menu parent;

        private List<Attachment> attachments = new List<Attachment>();
        public AttachToMenu(AdvMenu parent, Attachment attachment)
        {
            this.parent = parent;
            this.attachment = attachment;
            menu = new Menu(parent.MenuTitle, "Attach To");
            MenuController.AddSubmenu(parent, menu);
            menuEntry = new MenuItem("Attach To:", "Change the parent entity this attachment is attached to.");
            menuEntry.Label = $"#{this.attachment.Id}";
            MenuController.BindMenuItem(parent, menu, menuEntry);
            parent.AddMenuItem(menuEntry);

            PopulateItems();
            menu.OnItemSelect += OnItemSelected;
        }

        private async void OnItemSelected(Menu menu1, MenuItem menuitem, int index)
        {
            if (index == 0)
            {
                // World
                attachment.SetParent(null);
                Util.Alert("Entity is now attached to the world", null, "success");
                menuEntry.Label = "World";
            }
            else if (index == 1)
            {
                // Base
                attachment.SetParent(CurrentBuildMenu.Build.Base);
                Util.Alert("Entity parent reverted to builder base", null, "success");
                menuEntry.Label = "Base";
            }
            else
            {
                var newParent = attachments[index - 2];
                Util.Alert($"Entity parent changed to {newParent.Name} ({newParent.Id})", null, "success");
                menuEntry.Label = $"#{newParent.Id}";
                API.SetEntityNoCollisionEntity(attachment.Entity.Handle, newParent.Entity.Handle, false);
            }
            attachment.Attach();

            menu.CloseMenu();
            await BaseScript.Delay(2);
            parent.OpenMenu();
        }
        
        private void PopulateItems()
        {
            attachments.Clear();
            menu.ClearMenuItems();
            menu.AddMenuItem(new MenuItem("World",
                "Entity will be positioned in the world at the base vehicle's position with the specified offset.\nDoes not internally attach the entity")
            {
                Enabled = attachment.ParentEntity != null
            });
            menu.AddMenuItem(new MenuItem("Base Entity", "Restores the parent of this attachment back to the base entity")
            {
                Enabled = attachment.Entity != CurrentBuildMenu.Build.Base.Entity
            });
            foreach (var attach in CurrentBuildMenu.Build.Attachments.Values)
            {
                if (attach.Id == attachment.Id || attach.Entity == CurrentBuildMenu.Build.Base.Entity) continue;
                if (attach.ParentId == attachment.Id || attach.Id == attachment.ParentId) continue;
                
                menu.AddMenuItem(new MenuItem(attach.Name, $"Handle: {attach.Entity.Handle}\nType: {attach.Type.ToString()}\nId: {attach.Id}"));
                attachments.Add(attach);
            }
        }
    }
}