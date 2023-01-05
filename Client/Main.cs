using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.JackzBuilder;
using jackz_builder.Client.lib;
using jackz_builder.Client.submenus;
using MenuAPI;

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

    public class BuilderMain : BaseScript
    {
        public const bool DebugActive = true;
        public static readonly Semver BuilderVersion = new Semver("1.6.0");
        public const string FormatPrefix = "Jackz Builder: Fivem ";
        private static readonly string[] FreeEditInstructions = {
            "~INPUT_VEH_FLY_THROTTLE_UP~/~INPUT_VEH_FLY_THROTTLE_DOWN~ = Forward/Backwards",
            "~INPUT_VEH_FLY_YAW_LEFT~/~INPUT_VEH_FLY_YAW_RIGHT~ = Left/Right",
            "~INPUT_VEH_CINEMATIC_UP_ONLY~/~INPUT_VEH_CINEMATIC_DOWN_ONLY~ = Up/Down",
            "~INPUT_VEH_FLY_ROLL_LEFT_ONLY~/~INPUT_VEH_FLY_ROLL_RIGHT_ONLY~ = Rotate Left/Right",
            "~INPUT_VEH_FLY_PITCH_UP_ONLY~/~INPUT_VEH_FLY_PITCH_DOWN_ONLY~ = Rotate Up/Down",
            "~INPUT_VEH_FLY_SELECT_TARGET_LEFT~/~INPUT_VEH_FLY_SELECT_TARGET_RIGHT~ = Roll Sideways",
            "Hold ~INPUT_SPRINT~/~INPUT_DUCK~ = Speed Up/Slow Down"
        };

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
        private SavedBuildList SavedBuildList;
        public static SettingsMenu Settings { get; private set; }

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

        private static Entity _freeEditEntity = null;

        /// <summary>
        /// If non null, free edit (WASD, Numpad controls) is active for entity
        /// </summary>
        public static Entity FreeEdit
        {
            get => _freeEditEntity;
            set
            {
                _freeEditEntity = value;
                showFreeEditInstructions();
            }
        }

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
        
        private static void showFreeEditInstructions()
        {
            API.BeginTextCommandDisplayHelp("CELL_EMAIL_BCON");

            foreach (string s in FreeEditInstructions)
            {
                API.AddTextComponentSubstringPlayerName(s);
            }

            API.EndTextCommandDisplayHelp(0, false, true, 5000);
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

                if (Settings.ShowOverlay && HighlightedEntity == CurrentBuildMenu.Build.Base.Entity)
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
            Settings = new SettingsMenu(menu);

            SavedBuildList = new SavedBuildList(menu, "Saved Builds", "View & Manage your saved builds");
            SavedBuildList.SetBuilds(BuildManager.Builds.ToArray());
            
            ShowCreateMenus();
            if (DebugActive)
            {
                menu.OpenMenu();
            }
        }

        public void Destroy()
        {
            Settings.Save();
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
        
        [Command("jvb")]
        private void OpenJVB()
        {
            OpenMenu();
        }
    }
}