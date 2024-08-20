using test_project.Client.MenuAPI;

namespace test_project.Client.Menu
{
    public abstract class EntityMenu : MenuAPI.BuilderMenu
    {
        protected EntityMenu(string title, string description) : base(title, description, "", ClientMain.MenuPosition)
        {
            Menu.OnMenuOpen += (menu, data) =>
            {
                Populate();
            };
            Menu.OnMenuClose += (menu) =>
            {
                Menu.Clear();
            };
        }

        public void Populate()
        {
            BuilderListMenu<BuilderEntity> propsList = new BuilderListMenu<BuilderEntity>(ClientMain.Builder.Props, "Props",
                "Props", "", ClientMain.MenuPosition);
            AddChildMenu(propsList);
        }
    }
}