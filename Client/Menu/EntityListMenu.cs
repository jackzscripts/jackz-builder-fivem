using Mono.CSharp;
using ScaleformUI.Elements;
using ScaleformUI.Menu;
using test_project.Client.MenuAPI;

namespace test_project.Client.Menu
{
    public class EntityListMenu : MenuAPI.BuilderMenu
    {
        private UIMenuCheckboxItem _freeEditOption = new UIMenuCheckboxItem("Free Edit", false, "");
        private BuilderMenuFloatSliderItem _sensitivityOption;
        public EntityListMenu() : base("Entities", "Entities", "Manage spawned entities", ClientMain.MenuPosition)
        {
            _sensitivityOption = new BuilderMenuFloatSliderItem("Sensitivity", "", ClientMain.Builder.StepSize, 0.1f)
            {
                MinValue = 0.1f,
            };
            _sensitivityOption.OnValueChange += (sender, value) =>
            {
                ClientMain.Builder.StepSize = value;
            };
            AddItem(_freeEditOption);
            AddItem(_sensitivityOption);
            AddItem(new UIMenuSeparatorItem("Entities", false));
            AddChildMenu(new EntitySubListMenu("Props", BuilderEntity.EntityType.Prop));
            AddChildMenu(new EntitySubListMenu("Vehicles", BuilderEntity.EntityType.Vehicle));
            AddChildMenu(new EntitySubListMenu("Peds", BuilderEntity.EntityType.Ped));
        }
    }
}