using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.JackzBuilder.Spawners;
using jackz_builder.Client.lib;
using MenuAPI;
using Newtonsoft.Json;
using static jackz_builder.Client.BuilderMain;

namespace jackz_builder.Client.JackzBuilder
{
    internal class AddMenuMenus
    {
        public AdvMenu ParentList;
        
        public AdvMenu CuratedList;
        public AdvMenu RecentsList;
        public AdvMenu FavoritesList;
        public AdvMenu BrowseList;
    }
    public class CurrentBuildMenu : AdvMenu
    {
        public static Build Build;
        public AdvMenu BuildMetaMenu { get; private set; }
            #region Meta Menus
            public MenuItem SaveBuildItem { get; private set; }
            public MenuItem BuildAuthorItem { get; private set; }
            public MenuItem BuildNameItem { get; private set; }
            public MenuItem UploadBuildItem { get; private set; }
            #endregion
        
        public static AttachmentsMenu AttachmentsMenu { get; private set; }
        
        public AdvMenu AddVehicleMenu { get; private set; }
        public AdvMenu AddPropMenu { get; private set; }
        public AdvMenu AddPedMenu { get; private set; }
        
        
        
        public CurrentBuildMenu(AdvMenu parent) : base("Current Build", "Edit your current build")
        {
            AdvParentMenu = parent;
            SetupBuildMetaMenu(this);
            AttachmentsMenu = new AttachmentsMenu();
            AttachmentsMenu.CreateParentEntry(this);
            AddVehicleMenu = this.CreateAdvSubMenu("Add Vehicle");
            BaseSpawner.VehicleSpawner = new VehicleSpawner(AddVehicleMenu);
            AddPropMenu = this.CreateAdvSubMenu("Add Prop");
            BaseSpawner.PropSpawner = new PropSpawner(AddPropMenu);
            AddPedMenu = this.CreateAdvSubMenu("Add Ped");
            BaseSpawner.PedSpawner = new PedSpawner(AddPedMenu);

            OnMenuOpen += menu =>
            {
                EditorActive = true;
                HighlightedEntity = Build.Base.Entity;
            };
        }

        public async void AddAttachment(Attachment attachment)
        {
            SetNoCollision(attachment.Entity);
            attachment.Attach();
            Build.Attachments.Add(attachment.Entity, attachment);
            attachment.CreateEditMenu(AttachmentsMenu);
            // Show the new attachment
            MenuController.CloseAllMenus();
            await BaseScript.Delay(1);
            AttachmentsMenu.OpenMenu();
            await BaseScript.Delay(2);
            AttachmentsMenu.CurrentIndex = attachment.Root.SubmenuEntry.Index;
        }

        public void DeleteAttachment(Attachment attachment)
        {
            Build.Attachments.Remove(attachment.Entity);
            attachment.Entity.Delete();
            AttachmentsMenu.RemoveMenuItem(attachment.Root.SubmenuEntry);
            AttachmentsMenu.OpenMenu();
        }

        private void SetNoCollision(Entity entity)
        {
            API.SetEntityNoCollisionEntity(Build.Base.Entity.Handle, entity.Handle, false);
            foreach (var entity2 in Build.Attachments.Keys)
            {
                API.SetEntityNoCollisionEntity(entity2.Handle, entity.Handle, false);
            }
        }

        private void SetupBuildMetaMenu(AdvMenu parent)
        {
            BuildMetaMenu = parent.CreateAdvSubMenu("Build",
                "Save, upload, change author, or clear the build");
            // Items:
            SaveBuildItem = BuildMetaMenu.AddMenuItem(new MenuItem("Save Build", "Enter a name to save the build as"),
                async itemIndex =>
                {
                    if (!Build.HasName)
                    {
                        Util.Alert("Add a name to the build to save");
                        return;
                    }

                    BuildManager.SaveBuild(Build);
                    Util.Alert("Build saved successfully", null, "success");
                });
            UploadBuildItem = BuildMetaMenu.AddMenuItem(new MenuItem("Upload Build", "Upload your build to the cloud"));
            UploadBuildItem.Enabled = false;
            BuildNameItem = BuildMetaMenu.AddMenuItem(new MenuItem("Rename Build", "Change the build's name"),
                async itemIndex =>
                {
                    Build.Name = await Util.GetUserInput("Enter a new name", Build.HasName ? Build.Name : "", 60);
                    BuildNameItem.Label = Build.Name;
                });
            BuildNameItem.Label = Build.Name;
            BuildAuthorItem = BuildMetaMenu.AddMenuItem(new MenuItem("Author", "Change the build's author"),
                async itemIndex =>
                {
                    Build.Author = await Util.GetUserInput("Enter your name", Build.HasAuthor ? Build.Author : "", 60);
                    BuildAuthorItem.Label = Build.Author;
                });
            BuildAuthorItem.Label = Build.Author;

            BuildMetaMenu.AddDivider("Danger Zone", MenuItem.Icon.WARNING);
            BuildMetaMenu.AddMenuItem(
                new MenuItem("Remove All Attachments",
                    "This will remove all attachments from vehicle and delete them from the world.")
                {
                    LeftIcon = MenuItem.Icon.VEHICLE_DEATHMATCH,
                },
                itemIndex =>
                {
                    // TODO: Ask for Confirm
                    AttachmentsMenu.ClearMenuItems();
                    RemoveAllAttachments(Build.Base.Entity);
                    Build.Attachments.Clear();
                });
            BuildMetaMenu.AddMenuItem(
                new MenuItem("Delete",
                    "This will delete your build, including all attachments")
                {
                    LeftIcon = MenuItem.Icon.DEATHMATCH,
                },
                async itemIndex =>
                {
                    bool result = await Util.ShowConfirmDialog("Delete Confirmation",
                        "Are you sure you want to delete this build? All attached entities will also be deleted.");
                    Debug.WriteLine($"DONE DIALOG . RESULT IS: {result}");
                    if (!result)
                        return;
                    AttachmentsMenu.ClearMenuItems();
                    RemoveAllAttachments(Build.Base.Entity);
                    Build.Attachments.Clear();
                    Build = null;
                    BuildMetaMenu.ClearMenuItems();
                    BuildMetaMenu.CloseMenu();
                    await BaseScript.Delay(1);
                    Instance.ShowCreateMenus();
                    Instance.OpenMenu();
                });
        }

        public static void EditBuild(Build build)
        {
            Build = build;
        }
    }
}