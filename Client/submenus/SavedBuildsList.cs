using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using jackz_builder.Client.JackzBuilder;
using jackz_builder.Client.lib;
using MenuAPI;

namespace jackz_builder.Client
{
    public class SavedBuildEntry : AdvMenu
    {
        public SavedBuildEntry(AdvMenu parent, BuildMetaData meta, string title, bool showRatings) : base(title, meta.Id)
        {
            MenuController.AddSubmenu(parent, this);
            SubmenuEntry = new MenuItem(meta.Id, meta.GetDescriptionText(showRatings));
            MenuController.BindMenuItem(parent, this, SubmenuEntry);
            parent.AddMenuItem(SubmenuEntry);
            SetParent(parent);
            
            this.meta = meta;
            this.OnMenuOpen += OnOpen;
            AddMenuItem(new MenuItem("Spawn"), (_menu) =>
            {
                LoadData(false).Wait();
            });
            AddMenuItem(new MenuItem("Edit"), async (_menu) =>
            {
                var build = await LoadData(false);
                CurrentBuildMenu.EditBuild(build);
                BuilderMain.Instance.ShowBuildMenu(-1);
                CloseMenu();
                await BaseScript.Delay(2);
                BuilderMain.CurrentBuildMenu.OpenMenu();
            });
            AddMenuItem(new MenuItem("Upload") { Enabled = false });
            AddMenuItem(new MenuItem("Delete"), async _ =>
            {
                if (BuildManager.DeleteBuild(meta.Id))
                {
                    this.CloseMenu();
                    BuilderMain.SaveBuildMenu.RemoveMenuItem(this.SubmenuEntry);
                    ClearMenuItems();
                    await BaseScript.Delay(2);
                    BuilderMain.SaveBuildMenu.OpenMenu();
                    Util.Alert($"Build ~c~{meta.Id}~c~ has been deleted", null, "success");
                }
            });
        }

        private BuildMetaData meta = null;

        private async void OnOpen(Menu menu1)
        {
            await LoadData(true);
        }

        private async Task<Build> LoadData(bool isPreview)
        {
            try
            {
                Debug.WriteLine($"Loading build data for {meta.Id}...");
                Util.ShowBusySpinner($"Loading {meta.Id}");
                return await BuildManager.GetBuild(meta.Id, isPreview);
            }
            catch (InvalidBuildData ex)
            {
                Debug.WriteLine($"Failed to load invalid build:\n\t{ex.Message}");
                Util.Alert(ex.Message, "Invalid Build", "error", 10000);
                GetMenuItems().Select(item => item.Enabled = false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unknown error occurred while spawning build: {ex.Message}\n\t{ex.StackTrace}");
                Util.Alert(ex.Message, "Build Spawn Error", "error", 10000);
                GetMenuItems().Select(item => item.Enabled = false);
            }
            finally
            {
                Util.HideBusySpinner();
            }
            return null;
        }
    }
    
    public class SavedBuildList
    {
        private BuildMetaData[] savedBuildMetas;
        private AdvMenu menu;
        public bool ShowRatings;
        public SavedBuildList(AdvMenu parent, string title, string desc)
        {
            menu = parent.CreateAdvSubMenu(title, desc);
            menu.OnMenuOpen += OnListOpened;
        }

        public void SetBuilds(BuildMetaData[] metas)
        {
            savedBuildMetas = metas;
            if (menu.Visible)
            {
                OnListOpened(menu);
            }
        }

        private void OnListOpened(Menu _menu)
        {
            menu.ClearMenuItems();
            foreach (var meta in savedBuildMetas)
            {
                new SavedBuildEntry(menu, meta, "Saved Builds", ShowRatings);
            }
        }
    }
}