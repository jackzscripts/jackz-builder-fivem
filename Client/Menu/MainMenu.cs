using System;
using System.Drawing;
using System.Reflection;
using ScaleformUI.Menu;

namespace test_project.Client.Menu
{
    public class MainMenu : MenuAPI.BuilderMenu
    {
        private MetaMenu metaMenu = new MetaMenu();
        private BuilderMenu builderMenu = new BuilderMenu();
        
        public MainMenu() : base("", $"~q~Jackz Builder ~y~{ClientMain.Version}", "", new PointF(0, 0))
        {
            Menu.BuildingAnimation = MenuBuildingAnimation.LEFT;
            Menu.AnimationType = MenuAnimationType.BACK_INOUT;
            AddChildMenu(metaMenu);
            AddChildMenu(builderMenu);
            // AddChildMenu(new CloudMenu());
            // AddChildMenu(new BuildsMenu());
            // AddChildMenu(new BuilderMenu());
            Menu.Visible = true;
        }

        
    }
}