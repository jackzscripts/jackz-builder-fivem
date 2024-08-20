using System.Drawing;
using CitizenFX.Core;
using test_project.Client.ExtensionMethods;

namespace test_project.Client.Menu
{
    public class BuilderMenu : MenuAPI.BuilderMenu
    {
        public BuilderMenu() : base("Builder", "Builder", "", new PointF(0, 0)) {
            // AddChildMenu();
            /**
             * 
             */
            AddChildMenu(new SpawnersMenu());
            AddChildMenu(new EntityListMenu());
            CreateItemHandler("Create Preview", async (item, index) =>
            {
                Vector3 pos = Game.PlayerPed.Position.Offset(Game.PlayerPed.Rotation, 0, 5, 1);
                Vehicle vehicle = await World.CreateRandomVehicle(pos, Game.PlayerPed.Rotation.Y);
                ClientMain.Builder.PreviewEntity(vehicle);
            });
            CreateItemHandler("Stop Preview", (item, index) =>
            {
                ClientMain.Builder.ClearPreview();
            });
            
        }
    }
}