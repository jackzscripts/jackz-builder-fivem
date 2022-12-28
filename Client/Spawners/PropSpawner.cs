using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.lib;
using MenuAPI;

namespace jackz_builder.Client.JackzBuilder.Spawners
{
    public class PropSpawner : BaseSpawner
    {
        
        public static readonly List<string> CuratedList = Data.CuratedProps;
        // private static readonly List<string> PropList = Data.PropsList;

        public PropSpawner(AdvMenu parent) : base(parent, "props", typeof(Attachment))
        {
        }
        
        public override Task<Entity> CreatePreviewEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return CreateProp(hash, pos, ang, true);
        }
        
        public override Task<Entity> CreateEntity(uint hash, Vector3 pos, Vector3 ang)
        {
            return CreateProp(hash, pos, ang);
        }
        
        public static async Task<Entity> CreateProp(uint hash, Vector3 pos, Vector3 ang, bool isPreview = false)
        {
            var entity = isPreview
                ? Entity.FromHandle(API.CreateObject((int)hash, pos.X, pos.Y, pos.Z, false, false, false))
                : await World.CreateProp((int)hash, pos, ang, false, false);
            return entity;
        }

        protected override Menu CreateCuratedList()
        {
            var curatedPropsMenu = CreateSubmenu("Curated Props");
            foreach (var name in CuratedList)
            {
                var item = new MenuItem(name, name);
                item.ItemData = name;
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