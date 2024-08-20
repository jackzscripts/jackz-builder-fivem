using jackz_builder.Client.Builder;

namespace jackz_builder.Client.Menu.Spawner
{
    public class SpawnersMenu : MenuAPI.BuilderMenu 
    {
        public SpawnersMenu() : base("Spawn Entities", BuilderUtil.GetBreadcrumbs("Entity Spawner"), "", ClientMain.MenuPosition)
        {
            AddChildMenu(new PropSpawnerMenu());
        }
    }
}