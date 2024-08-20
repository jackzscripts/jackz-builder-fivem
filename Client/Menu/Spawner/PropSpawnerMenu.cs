using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using ScaleformUI.Elements;
using ScaleformUI.Menu;
using ScaleformUI.Scaleforms;
using test_project.Client.ExtensionMethods;
using test_project.Client.MenuAPI;

namespace test_project.Client.Menu
{
    public class PropSpawnerMenu : MenuAPI.BuilderMenu
    {
        public static readonly List<string> CuratedProps = new List<string>
        {
            "prop_barriercrash_04",
            "prop_barier_conc_01a",
            "prop_barier_conc_01b",
            "prop_barier_conc_03a",
            "prop_barier_conc_02c",
            "prop_mc_conc_barrier_01",
            "prop_barier_conc_05b",
            "prop_metal_plates01",
            "prop_metal_plates02",
            "prop_woodpile_01a",
            "prop_weed_pallet",
            "prop_water_ramp_03",
            "prop_water_ramp_02",
            "prop_mp_ramp_02",
            "prop_mp_ramp_01_tu",
            "prop_roadcone02a",
            "prop_beer_neon_01",
            "prop_sign_road_03b",
            "prop_prlg_snowpile",
            "prop_logpile_06b",
            "prop_windmill_01",
            "prop_cactus_01e",
            "prop_minigun_01",
            "v_ilev_gold",
            "bkr_prop_bkr_cashpile_07",
            "ex_cash_pile_07",
            "prop_cs_dildo_01",
            "prop_ld_bomb_01"
        };
        public PropSpawnerMenu() : base("Props", BuilderUtil.GetBreadcrumbs("Spawner", "Props"), "", ClientMain.MenuPosition)
        {
            BuilderListMenu<string> curated = new BuilderListMenu<string>(CuratedProps, "Curated Props", "Curated Props", "", ClientMain.MenuPosition);
            curated.OnItemSelected += async (sender, index, item) =>
            {
                using var token = new CancellationTokenSource();
                uint? model = await ClientMain.Builder.RequestModel(item, token.Token);
                if (model != null)
                {
                    Prop prop = await World.CreateProp(new Model((int)model), Game.PlayerPed.GetOffsetPosition(new Vector3(0, 5, 0)),
                        Game.PlayerPed.Rotation, false, true);
                    prop.IsCollisionEnabled = true;
                    ClientMain.Builder.AddEntity(prop, item);
                }

                ClientMain.Builder.ClearPreview();
            };
            curated.OnItemHovered += (menu, index, item) =>
            {
                PreviewItem(curated);
            };
            curated.OnMenuClose += menu =>
            {
                ClientMain.Builder.ClearPreview();
            };
            AddChildMenu(curated);
            AddChildMenu(new BuilderListMenu<string>(new List<string>(), "Favorites", "Favorites", "", ClientMain.MenuPosition));
            
            // TODO: on menu open, add drag mode 
        }

        private async void PreviewItem(BuilderListMenu<string> menu)
        {
            var item = menu.SelectedMenuItem;
            // Compute some metadata if we haven't already:
            if (string.IsNullOrEmpty(item.Description))
            {
                var modelId = menu.SelectedItem;
                var entity = await ClientMain.Builder.PreviewProp(modelId);
                if (entity != null)
                {
                    Vector3 size = entity.Model.GetDimensions();
                    item.Description =
                        $"Hash: {entity.Model.Hash.ToString()}\nSize: {size.X:F1}u x {size.Y:F1}u x {size.Z:F1}u";
                }
            }
        }
    }
}