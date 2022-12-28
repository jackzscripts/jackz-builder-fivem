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
    public class PedSpawner : BaseSpawner
    {
        
        public static readonly Dictionary<string, string> CuratedList = Data.CuratedPeds;
        // private static readonly List<string> PropList = Data.PropsList;

        public PedSpawner(AdvMenu parent) : base(parent, "peds", typeof(Attachment))
        {
        }
        
        public override async Task<Entity> CreatePreviewEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return await CreatePed(hash, pos, ang, true);

        }
        
        public override async Task<Entity> CreateEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return await CreatePed(hash, pos, ang);
        }

        public static async Task<Entity> CreatePed(uint hash, Vector3 pos, Vector3 ang, bool isPreview = false)
        {
            var entity = isPreview
                ? Entity.FromHandle(API.CreatePed(0, hash, 0f, 0f, 0f, ang.Z, false, false))
                : await World.CreatePed((int)hash, Vector3.Zero, ang.Z);
            setupEntity(entity, pos);
            return entity;
        }

        private static void setupEntity(Entity entity, Vector3 pos)
        {
            entity.Position = pos;
            API.SetBlockingOfNonTemporaryEvents(entity.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(entity.Handle, true);
            API.FreezeEntityPosition(entity.Handle, true);
        }

        protected override Menu CreateCuratedList()
        {
            var curatedPropsMenu = CreateSubmenu("Curated Peds");
            foreach (var kv in CuratedList)
            {
                var item = new MenuItem(kv.Value ?? kv.Key, kv.Key);
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