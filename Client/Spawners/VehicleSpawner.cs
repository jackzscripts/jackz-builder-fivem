using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.lib;
using MenuAPI;

namespace jackz_builder.Client.JackzBuilder.Spawners
{
    public class VehicleSpawner : BaseSpawner
    {
        
        public static readonly Dictionary<string, string> CuratedList = Data.CuratedVehicles;
        // private static readonly List<string> PropList = Data.PropsList;

        public VehicleSpawner(AdvMenu parent) : base(parent, "vehicles", typeof(VehicleAttachment))
        {
        }
        
        public override async Task<Entity> CreatePreviewEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return await CreateVehicle(hash, pos, ang, true);
        }
        
        public override async Task<Entity> CreateEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return await CreateVehicle(hash, pos, ang);
        }
        
        public static async Task<Vehicle> CreateVehicle(uint hash, Vector3 pos, Vector3 ang, bool isPreview = false)
        {
            Vehicle entity = isPreview
                ? (Vehicle) Entity.FromHandle(API.CreateVehicle(hash, pos.X, pos.Y, pos.Z, ang.Z, false, false))
                : await World.CreateVehicle((int) hash, pos, ang.Z);
            return entity;
        }

        protected override Menu CreateCuratedList()
        {
            var curatedPropsMenu = CreateSubmenu("Curated Vehicles");
            foreach (var kv in CuratedList)
            {
                var item = new MenuItem(kv.Value, kv.Key);
                item.ItemData = kv.Key;
                curatedPropsMenu.AddMenuItem(item);
            }

            return curatedPropsMenu;
        }
        
        protected override async void CreateManualInputList()
        {
            ParentMenu.AddMenuItem(new MenuItem("Manual Input"), async index =>
            {
                var name = await Util.GetUserInput("Enter model name", "", 50);
                if (!await SpawnEntity(name))
                {
                    Util.Alert($"The model specified is invalid");
                }
            });

        }

        protected override void CreateBrowseList()
        {
            var items = GetEntities();
            var itemList = new MenuDynamicListItem("Browse", items.First(), (item, left) =>
            {
                if (item.Index == 0 || item.Index >= items.Length)
                {
                    return null;
                }

                return items[item.Index];
            });
            ParentMenu.AddMenuItem(itemList, (listItem, currentItem) =>
            {
                SpawnEntity(currentItem);
            }, (item, value, newValue) =>
            {
                SpawnPreviewEntity(newValue);
            });
        }
    }
}