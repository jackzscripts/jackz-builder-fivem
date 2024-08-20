namespace test_project.Client.Menu
{
    public class SpawnersMenu : MenuAPI.BuilderMenu 
    {
        public SpawnersMenu() : base("Spawn Entities", BuilderUtil.GetBreadcrumbs("Entity Spawner"), "", ClientMain.MenuPosition)
        {
            AddChildMenu(new PropSpawnerMenu());
        }
    }
}