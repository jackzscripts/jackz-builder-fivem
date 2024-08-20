using CitizenFX.Core;
using jackz_builder.Client.Builder;
using ScaleformUI.Menu;

namespace jackz_builder.Client.Menu
{
    public class EntitySubListMenu : MenuAPI.BuilderMenu
    {
        private BuilderEntity.EntityType type;
        // TODO: refactor to BuilderDynamicListMenu?
        public EntitySubListMenu(string title, BuilderEntity.EntityType type) : base(title, title, "", ClientMain.MenuPosition)
        {
            this.type = type;
            Menu.OnMenuOpen += (menu, data) =>
            {
                var entities = ClientMain.Builder.Entities.FindAll(e =>
                {
                    Debug.WriteLine($"e.Type: {e.Type} this.type: ${this.type}");
                    return e.Type == this.type;
                });
                foreach (var entry in entities)
                {
                    AddChildMenu(entry.Menu);
                }
                // Clear "loading" entry
                if (entities.Count > 0)
                {
                    Menu.RemoveItemAt(0);
                }
            };
            Menu.OnMenuClose += (menu) =>
            {
                Menu.Clear();
                Menu.AddItem(new UIMenuItem("No spawned entities") { Enabled = false });
            };
            Menu.AddItem(new UIMenuItem("No spawned entities") { Enabled = false });
        }
    }
}