using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.lib;
using MenuAPI;
using Newtonsoft.Json;

namespace jackz_builder.Client.submenus
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsMenu
    {
        [JsonProperty("spawnInVehicle")] private bool _spawnInVehicle = true;
        [JsonProperty("autosave")] private bool _autosave = true;
        [JsonProperty("showOverlay")] private bool _showOverlay = true;
        private const string KvpSaveName = "jackzbuilder:settings";


        public bool SpawnInVehicle
        {
            get => _spawnInVehicle;
            set
            {
                _spawnInVehicle = value;
                Save();
            }
        }
        
        public bool AutoSave
        {
            get => _autosave;
            set
            {
                _autosave = value;
                Save();
            }
        }
        
        public bool ShowOverlay
        {
            get => _showOverlay;
            set
            {
                _showOverlay = value;
                Save();
            }
        }

        private AdvMenu menu;

        public SettingsMenu(AdvMenu parent)
        {
            menu = parent.CreateAdvSubMenu("Settings");
            menu.OnMenuOpen += ShowSettings;
            menu.OnMenuClose += HideSettings;
            Load();
        }

        private void HideSettings(Menu menu1)
        {
            menu.ClearMenuItems();
        }

        private void ShowSettings(Menu menu1)
        {
            menu.AddMenuItem(
                new MenuCheckboxItem("Spawn In Vehicle", "Should you spawn in the build if possible when spawning?",
                    SpawnInVehicle),
                (index, active) =>
                {
                    SpawnInVehicle = active;
                });
            menu.AddMenuItem(
                new MenuCheckboxItem("Autosave", "Should autosave be enabled? Recommended to keep on",
                    AutoSave
                ),
                (index, active) =>
                {
                    AutoSave = active;
                });
            menu.AddMenuItem(
                new MenuCheckboxItem("Show Overlays", "Should overlays be shown for the build and attachments?",
                    ShowOverlay
                ),
                (index, active) =>
                {
                    ShowOverlay = active;
                });
        }


        public void Save()
        {
            API.SetResourceKvp(KvpSaveName, JsonConvert.SerializeObject(this));
        }

        public void Load()
        {
            var data = API.GetResourceKvpString(KvpSaveName);
            if (data != null)
            {
                Debug.WriteLine($"Loading saved settings:\n{data}");
                JsonConvert.PopulateObject(data, this);
            }
        }
    }
}