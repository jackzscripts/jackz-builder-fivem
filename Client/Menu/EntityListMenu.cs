using jackz_builder.Client.Builder;
using jackz_builder.Client.MenuAPI;
using ScaleformUI.Menu;

namespace jackz_builder.Client.Menu
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