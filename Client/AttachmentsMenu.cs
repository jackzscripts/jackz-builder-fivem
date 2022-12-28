using jackz_builder.Client.lib;
using MenuAPI;

namespace jackz_builder.Client.JackzBuilder
{
    public class AttachmentsMenu : AdvMenu
    {
        public new int CurrentIndex { get; set; }

        public AttachmentsMenu() : base("Attachments", "Manage all attached entities")
        {
            AddMenuItem(new MenuSliderItem("Coordinate Sensitivity",
                    "Determines how much the values of coordinates increases. Bigger the number, the bigger the adjustment in coordinates",
                    1,
                    20,
                    (int)BuilderMain.EditorSensitivity),
                ((itemIndex, value, newValue) =>
                {
                    BuilderMain.EditorSensitivity = newValue;
                }));
            var edit = AddMenuItem(new MenuCheckboxItem("Free Edit",
                    "Allows you to move entities by holding the following keys:\nWASD -> Normal\nSHIFT/CTRL - Up and down\nNumpad 8/5 - Pitch\nNumpad 4/6 - Roll\nNumpad 7/9 - Rotation\n\nWill only work when hovering over an entity or stand is closed, disabled in entity list."),
                (itemIndex,
                    active) =>
                {
                    BuilderMain.FreeEdit = active;
                    
                });
            edit.Checked = BuilderMain.FreeEdit;

            // Switch the highlighted entity to the base entity
            OnMenuOpen += _ =>
            {
                BuilderMain.HighlightedEntity = CurrentBuildMenu.Build.Base.Entity;
            };

            AddDivider("Attachments");
        }
    }
}