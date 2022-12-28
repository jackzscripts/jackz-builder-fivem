using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.lib;
using MenuAPI;
using Newtonsoft.Json;

namespace jackz_builder.Client.JackzBuilder.Spawners
{
    public class RecentEntry
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("count")] public int Count;
    }
    public abstract class BaseSpawner
    {
        private const int MaxSearchEntries = 40;
        private const int MaxRecentEntries = 50;

        public static PropSpawner PropSpawner;
        public static VehicleSpawner VehicleSpawner;
        public static PedSpawner PedSpawner;

        private Type AttachmentType;
        
        protected List<string> sortedSearchResults;
        protected Dictionary<string, RecentEntry> RecentEntries = new Dictionary<string, RecentEntry>();
        protected Dictionary<string, string> FavoriteEntries = new Dictionary<string, string>();
        protected string Id;
        protected AdvMenu ParentMenu;

        protected BaseSpawner(AdvMenu parent, string id, Type attachmentType)
        {
            Id = id;
            ParentMenu = parent;
            AttachmentType = attachmentType ?? typeof(Attachment);
            parent.OnMenuOpen += _ =>
            {
                CreateMenus();
                LoadData();
            };
        }

        public void CreateMenus()
        {
            ParentMenu.ClearMenuItems();
            CreateCuratedList();
            CreateSearchList();
            CreateFavoritesList();
            CreateRecentsList();
            CreateManualInputList();
            CreateBrowseList();
        }
        public async void OnSearchOpen(Menu menu)
        {
            var query = (await Util.GetUserInput("Enter search", "", 50)).ToLower();
            if (query == "") return;
            menu.ClearMenuItems();
            var props = GetEntities();
            var results = new Dictionary<string, int>();
            foreach(string prop in props)
            {
                var distance = prop.IndexOf(query);
                if (distance >= 0)
                {
                    results.Add(prop, distance);
                }
            }
            sortedSearchResults = results.OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();

            for (int i = 0; i <= MaxSearchEntries; i++)
            {
                // Util.CreateSubMenu(menu, sortedSearchResults[i]);
                menu.AddMenuItem(new MenuItem(sortedSearchResults[i]));
            }

            menu.OnItemSelect += OnSearchResultSelected;
        }

        private void OnSearchResultSelected(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // CreateEntitySection(sortedSearchResults[itemIndex]);
        }

        protected string[] GetEntities()
        {
            var str = API.LoadResourceFile(API.GetCurrentResourceName(), $"Client/data/{Id}.txt");
            if (str == null) throw new Exception($"Missing data file: {Id}.txt");
            return str.Split('\n');
        }

        public abstract Task<Entity> CreateEntity(uint hash, Vector3 pos, Vector3 ang);
        public abstract Task<Entity> CreatePreviewEntity(uint hash, Vector3 pos, Vector3 ang);
        protected async void SpawnPreviewEntity(string name)
        {
            Debug.WriteLine($"Fetching preview: {name}");
            var hash = (uint) API.GetHashKey(name);
            if (!API.IsModelValid(hash))
            {
                Debug.WriteLine($"SpawnPreviewEntity({name}: Model {hash} is invalid");
                return;
            }
            await Util.RequestModel(hash);
            // BuilderMain.ClearPreview();
            var pos = CurrentBuildMenu.Build.Base.Entity.GetOffsetPosition(BuilderMain.PreviewOffset);
            var entity = await CreatePreviewEntity(hash, pos, CurrentBuildMenu.Build.Base.Entity.Rotation);
            API.SetModelAsNoLongerNeeded(hash);
            BuilderMain.SetPreview(entity, name);
            Debug.WriteLine($"Preview done loading for {name}");
        }

        private void spawnPreviewEntityCallback(Menu _, MenuItem oldItem, MenuItem item, int ____, int newIndex)
        {
            string name = item.Description;
            SpawnPreviewEntity(name);
        }

        protected async Task<bool> SpawnEntity(string name)
        {
            var hash = (uint) API.GetHashKey(name);
            if (!API.IsModelValid(hash))
            {
                return false;
            }

            // TODO: Confirm
            var entity = await CreateEntity(hash, CurrentBuildMenu.Build.Base.Entity.Position, CurrentBuildMenu.Build.Base.Entity.Rotation);
            var attach = (Attachment) Activator.CreateInstance(AttachmentType, new object[] { CurrentBuildMenu.Build.NextId,CurrentBuildMenu.Build.Base.Entity, entity, name });
            BuilderMain.CurrentBuildMenu.AddAttachment(attach);
            AddRecentEntry(name);
            return true;
        }
        
        private void spawnEntityCallback(Menu _menu, MenuItem item, int index)
        {
            SpawnEntity(item.Description);
            _menu.CloseMenu();
        }

        protected Menu CreateSubmenu(string title, string desc = null)
        {
            var submenu = ParentMenu.CreateSubMenu(title, desc);
            submenu.OnItemSelect += spawnEntityCallback;
            submenu.OnIndexChange += spawnPreviewEntityCallback;
            submenu.OnMenuClose += ClearPreview;
            return submenu;
        }

        private void LoadData()
        {
            RecentEntries = JsonConvert.DeserializeObject<Dictionary<string, RecentEntry>>(API.GetResourceKvpString($"testc:recents.{Id}"));
            FavoriteEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(API.GetResourceKvpString($"testc:favorites.{Id}"));
        }

        public void AddRecentEntry(string id, string name = null)
        {
            if (!RecentEntries.ContainsKey(id))
            {
                RecentEntries[id] = new RecentEntry()
                {
                    Name = name,
                    Count = 1
                };
                // Remove the least used item
                if (RecentEntries.Count > MaxRecentEntries)
                {
                    var key = RecentEntries.OrderBy(kv => kv.Value.Count).First().Key;
                    RecentEntries.Remove(key);
                }
            }

            RecentEntries[id].Count++;
            
            API.SetResourceKvp($"testc:recents.{Id}", JsonConvert.SerializeObject(RecentEntries));
        }


        public void AddFavoritesEntry(string id, string name = null)
        {
            FavoriteEntries.Add(id, name);
            
            API.SetResourceKvp($"testc:favorites.{Id}", JsonConvert.SerializeObject(FavoriteEntries));
        }

        public bool RemoveFavorites(string id)
        {
            var value = FavoriteEntries.Remove(id);
            
            API.SetResourceKvp($"testc:favorites.{Id}", JsonConvert.SerializeObject(FavoriteEntries));
            return value;
        }

        protected virtual Menu CreateCuratedList()
        {
            CreateDummy("Curated");
            return null;
        }
        
        protected Menu CreateSearchList()
        {
            var searchMenu = CreateSubmenu("Search");
            searchMenu.OnMenuOpen += OnSearchOpen;
            return searchMenu;
        }

        protected virtual void CreateManualInputList()
        {
            CreateDummy("Manual Input");
        }

        protected virtual Menu CreateRecentsList() {
            var recentMenu = CreateSubmenu("Recent Props");
            recentMenu.OnMenuOpen += _menu =>
            {
                recentMenu.ClearMenuItems();
                var entries = RecentEntries
                    .OrderByDescending(kv => kv.Value.Count);
                foreach (var recent in entries)
                {
                    var item = new MenuItem(recent.Value.Name ?? recent.Key, $"Id: {recent.Key}\nTimes Used: {recent.Value.Count}")
                    {
                        ItemData = recent.Key
                    };
                    recentMenu.AddMenuItem(item);
                }
            };
            return recentMenu;
        }

        protected virtual Menu CreateFavoritesList()
        {
            var favoritesMenu = CreateSubmenu("Favorites");
            favoritesMenu.OnMenuOpen += menu =>
            {
                menu.ClearMenuItems();
                foreach (var kv in FavoriteEntries)
                {
                    menu.AddMenuItem(new MenuItem(kv.Value ?? kv.Key, kv.Key));
                }
            };
            return null;
        }

        protected virtual void CreateBrowseList()
        {
            CreateDummy("Browse");
        }

        private void CreateDummy(string name)
        {
            ParentMenu.AddMenuItem(new MenuItem(name)
            {
                Enabled = false
            });
        }

        protected void ClearPreview(Menu menu)
        {
            BuilderMain.ClearPreview();
        }
    }
}