using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.JackzBuilder;
using MenuAPI;
using jackz_builder.Client.lib;

namespace jackz_builder.Client
{
    internal class PreviewData
    {
        public Entity? Entity;
        public string? Id;
        public Vector3 Offset;

        private float heading;
        private int lastTime;


        public void Render(int time)
        {
            if (time - lastTime < 12 || Entity == null) return;
            heading += 2f;
            if (heading >= 360) heading = 0;
            var pos = Game.PlayerPed.GetOffsetPosition(Offset);
            Entity.Position = pos;
            Entity.Heading = heading;
            lastTime = time;
        }
    }
    internal class EntryMenu : AdvMenu
    {
        public EntryMenu(AdvMenu parent, BuildMetaData meta) : base("Saved Builds", meta.Id)
        {
            MenuController.AddSubmenu(parent, this);
            SubmenuEntry = new MenuItem(meta.Id, meta.GetDescriptionText());
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
    public class BuilderMain : BaseScript
    {
        public const bool DebugActive = true;
        public const string BuilderVersion = "1.6.0";
        public const string FormatVersion = "Jackz Builder Fivem " + BuilderVersion;
        
        private AdvMenu menu;
        private List<AdvMenu> submenus = new List<AdvMenu>();
        private List<MenuItem> menuItems = new List<MenuItem>();

        // No build menus:
        private MenuItem CreateVehicleMenu;

        // Have build menus:
        private MenuItem StartNewBuildMenu;
        public static CurrentBuildMenu CurrentBuildMenu;

        // Build menus:
        private AdvMenu BuildMetaMenu;
        public static AdvMenu SaveBuildMenu;
        private AdvMenu EntitiesMenu;
        
        public static dynamic TNotify;


        public static BuilderMain Instance { get; private set; }
        /// <summary>
        /// The currently highlighted entity
        /// </summary>
        public static Entity HighlightedEntity { get; set; }

        /// <summary>
        /// Is an editor window open?
        /// </summary>
        public static bool EditorActive;

        /// <summary>
        /// The sensitivity of the sliders (position offset, rotations, etc)
        /// </summary>
        public static float EditorSensitivity = 10f;

        /// <summary>
        /// Determines if Free Edit (WASD, Numpad controls) is active
        /// </summary>
        public static bool FreeEdit = false;

        /// <summary>
        /// The current preview entity
        /// </summary>
        private static readonly PreviewData Preview = new PreviewData();

        public static Vector3 PreviewOffset => Preview.Offset;

        public static void ClearPreview()
        {
            var oldPreview = Preview.Entity;
            if (oldPreview != null && oldPreview.Exists())
            {
                RemoveAllAttachments(oldPreview);
                oldPreview.Delete();
            }

            Preview.Entity = null;
            Debug.WriteLine("Cleared Build Preview");
        }

        public static void SetPreview(Entity entity, string id = null, float? range = null, Action renderFunc = null,
            dynamic renderData = null, float? rangeZ = null)
        {
            ClearPreview();
            Preview.Entity = entity;
            Preview.Id = id;
            Preview.Offset.Y = range.GetValueOrDefault(7.5f);
            Preview.Offset.Z = rangeZ.GetValueOrDefault(0.3f);
            SetupPreviewEntity(entity);
        }

        private static void SetupPreviewEntity(Entity entity)
        {
            API.SetEntityAlpha(entity.Handle, 150, 0);
            API.SetEntityCompletelyDisableCollision(entity.Handle, false, false);
            API.SetEntityInvincible(entity.Handle, true);
            API.SetEntityHasGravity(entity.Handle, false);
            if (entity.Model.IsVehicle)
            {
                API.DisableVehicleWorldCollision(entity.Handle);
                API.SetVehicleGravity(entity.Handle, false);
            }
        }

        public static void RemoveAllAttachments(Entity entity)
        {
            recurseRemoveAttachments(entity, World.GetAllProps());
            recurseRemoveAttachments(entity, World.GetAllVehicles());
            recurseRemoveAttachments(entity, World.GetAllPeds());
        }

        private static void recurseRemoveAttachments(Entity parent, Entity[] entities)
        {
            foreach (var entity in entities)
            {
                if (entity != parent)
                {
                    foreach (var subEntity in entities)
                    {
                        if (subEntity != entity && subEntity != parent && subEntity.IsAttachedTo(entity))
                        {
                            subEntity.Delete();
                        }
                    }
                    if (entity.IsAttachedTo(parent))
                    {
                        entity.Delete();
                    }
                }
            }
        } 
        
        [EventHandler("onResourceStop")]
        private void onResourceStop(string resource)
        {
            if (resource == API.GetCurrentResourceName())
            {
                Destroy();
            }
        }

        [Tick]
        public async Task OnTick()
        {
            var time = API.GetGameTimer();
            Preview.Render(time);
            
            if (CurrentBuildMenu.Build != null && EditorActive && HighlightedEntity != null)
            {
                Util.HighlightPosition(HighlightedEntity.Position);
                Util.DrawMarker(0, HighlightedEntity.Position, Color.White, false);

                if (HighlightedEntity == CurrentBuildMenu.Build.Base.Entity)
                {
                    API.SetDrawOrigin(HighlightedEntity.Position.X, HighlightedEntity.Position.Y, HighlightedEntity.Position.Z, 0);
                    Util.DrawRect(Vector2.Zero, new Vector2(0.2f, 0.1f), new Color(0, 0, 0, 76));
                    Util.AddText(new Vector2(-0.09f, -0.04f), CurrentBuildMenu.Build.Name, 0.3f, Color.White);
                    Util.AddText(new Vector2(-0.09f, -0.02f), CurrentBuildMenu.Build.Author, 0.25f, Color.White);
                    Util.DrawRect(new Vector2(0.001f, 0.0f), new Vector2(0.198f, 0.001f), new Color(255, 255, 255, 120));
                    API.ClearDrawOrigin();
                }
            }
        }

        public static EntityType GetType(Entity entity)
        {
            var type = EntityType.Prop;
            if (entity.Model.IsVehicle)
            {
                type = EntityType.Vehicle;
            } else if (entity.Model.IsPed)
            {
                type = EntityType.Ped;
            }

            return type;
        }
        
        public BuilderMain()
        {
            Instance = this;
            TNotify = Exports["t-notify"];
            menu = new AdvMenu("Jackz Builder");
            menu.OnMenuOpen += _menu =>
            {
                EditorActive = false;
            };
            MenuController.AddMenu(menu);
            CreateSavedBuildsList();
            
            ShowCreateMenus();
            if (DebugActive)
            {
                menu.OpenMenu();
            }
        }

        public void Destroy()
        {
            ClearPreview();
            foreach (var item in menuItems)
            {
                menu.RemoveMenuItem(item);
            }
            menuItems.Clear();
            foreach (var item in submenus)
            {
                menu.RemoveMenuItem(item.SubmenuEntry);
            }
            submenus.Clear();
            CurrentBuildMenu = null;
        }
        
        public void OpenMenu() { menu.OpenMenu(); }
        public void CloseMenu() { menu.CloseMenu(); }

        private void CreateSavedBuildsList()
        {
            SaveBuildMenu = menu.CreateAdvSubMenu("Saved Builds");
            SaveBuildMenu.OnMenuOpen += _menu =>
            {
                SaveBuildMenu.ClearMenuItems();
                foreach (var meta in BuildManager.Builds)
                {
                    new EntryMenu(SaveBuildMenu, meta);
                }
            };
        }

        private void CreateNewVehicle(int index)
        {
            var vehicle = Util.getVehicle();
            if (vehicle != null)
            {
                CurrentBuildMenu.Build = new Build(vehicle);
                ShowBuildMenu();
                menu.CurrentIndex = 1;
                Util.Alert("Created new vehicle", null, "success");
            }
        }
        
        public void ShowCreateMenus(int itemIndex = -1)
        {
            Destroy();
            
            CreateVehicleMenu = menu.AddMenuItem(new MenuItem("Set Current Vehicle As Base",
                    "Creates a new custom vehicle with your current vehicle as the base"),
                CreateNewVehicle);
            menuItems.Add(CreateVehicleMenu);
        }

        public void ShowBuildMenu(int itemIndex = -1)
        {
            Destroy();
            
            CurrentBuildMenu = new CurrentBuildMenu(menu);
            CurrentBuildMenu.CreateParentEntry(menu);
            submenus.Add(CurrentBuildMenu);
            StartNewBuildMenu = menu.AddMenuItem(new MenuItem("Start New Build",
                    "Will delete your current build"),
                ShowCreateMenus);
            menuItems.Add(StartNewBuildMenu);
        }

        public void AddAttachment(Attachment attachment)
        {
            CurrentBuildMenu.AddAttachment(attachment);
        }
        
        [Command("jvb")]
        private void OpenJVB()
        {
            OpenMenu();
        }
    }
}