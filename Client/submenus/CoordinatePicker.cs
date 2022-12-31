using System;
using CitizenFX.Core;
using jackz_builder.Client.lib;
using MenuAPI;
using Newtonsoft.Json;

namespace jackz_builder.Client.submenus
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CoordinatePicker
    {
        public Menu Menu { get; }

        public string XAxisTitle = "X";
        public string YAxisTitle = "Y";
        public string ZAxisTitle = "Z";

        public int Maximum = 1_000_000;
        public int Minimum = -1_000_000;

        public bool Toggleable { get; }
        public bool Enabled { get; private set; }

        /// <summary>
        /// Determines how sensitive the slider is. The displayed number is divided by this
        /// </summary>
        public int Sensitivity = 100;

        [JsonProperty("x")] public float X;
        [JsonProperty("y")] public float Y;
        [JsonProperty("z")]  public float Z;

        private int _x
        {
            get => (int)(X * 100);
            set => X = value / 100f;
        }
        
        private int _y
        {
            get => (int)(Y * 100);
            set => Y = value / 100f;
        }
        
        private int _z
        {
            get => (int)(Z * 100);
            set => Z = value / 100f;
        }

        private Action<Vector3, bool> closeCallback;

        private MenuSliderItem XSlider;
        private MenuSliderItem YSlider;
        private MenuSliderItem ZSlider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sub"></param>
        /// <param name="desc"></param>
        /// <param name="onClose"></param>
        /// <param name="showEnable">Null to disable enable toggle. Otherwise value is the active state of the checkbox</param>
        public CoordinatePicker(Menu parent, string sub, string desc, Action<Vector3, bool> onClose, Vector3? defaultPos, bool canEnable)
        {
            Menu = Util.CreateSubMenu(parent, sub, desc);
            if (canEnable)
            {
                Toggleable = true;
                Menu.AddMenuItem(new MenuCheckboxItem("Enabled", defaultPos != null));
                Menu.OnCheckboxChange += OnCheckboxChange;
            }

            if (defaultPos != null)
            {
                X = defaultPos.Value.X;
                Y = defaultPos.Value.Y;
                Z = defaultPos.Value.Z;
            }

            XSlider = new MenuSliderItem(XAxisTitle, $"{_x:F2}", Minimum, Maximum, _x);
            Menu.AddMenuItem(XSlider);
            YSlider = new MenuSliderItem(YAxisTitle, $"{_y:F2}", Minimum, Maximum, _y);
            Menu.AddMenuItem(YSlider);
            ZSlider = new MenuSliderItem(ZAxisTitle, $"{_z:F2}", Minimum, Maximum, _z);
            Menu.AddMenuItem(ZSlider);
            
            Menu.AddMenuItem(new MenuItem("Set To Current Position", "Set the coordinates to be your current position"));
            Menu.OnSliderItemSelect += OnSelect;
            Menu.OnSliderPositionChange += OnChange;
            Menu.OnItemSelect += OnItemSelect;
            Menu.OnMenuClose += OnClose;
            closeCallback = onClose;
        }
        
        private void OnCheckboxChange(Menu menu, MenuCheckboxItem menuitem, int itemindex, bool newcheckedstate)
        {
            Enabled = newcheckedstate;
        }

        private void OnItemSelect(Menu menu, MenuItem menuitem, int itemindex)
        {
            X = Game.PlayerPed.Position.X;
            Y = Game.PlayerPed.Position.Y;
            Z = Game.PlayerPed.Position.Z;
            XSlider.Description = $"{X:F2}";
            YSlider.Description = $"{Y:F2}";
            ZSlider.Description = $"{Z:F2}";
        }

        private void OnClose(Menu menu1)
        {
            closeCallback(new Vector3(X, Y, Z), Enabled);
        }

        private async void OnSelect(Menu menu1, MenuSliderItem sliderItem, int sliderposition, int index)
        {
            var output = await Util.GetUserInput("Enter new value:", null, 10);
            float newValue;
            if (float.TryParse(output, out newValue))
            {
                if (sliderItem == XSlider)
                {
                    X = newValue;
                    XSlider.Description = $"{X:F2}";
                } else if (sliderItem == YSlider)
                {
                    Y = newValue;
                    YSlider.Description = $"{Y:F2}";
                } else if (sliderItem == ZSlider)
                {
                    Z = newValue;
                    ZSlider.Description = $"{Z:F2}";
                }
            }
            else
            {
                Util.Alert("New value is invalid");
            }
        }

        private void OnChange(Menu menu1, MenuSliderItem sliderItem, int oldValue, int newValue, int index)
        {
            if (sliderItem == XSlider)
            {
                _x = newValue;
                XSlider.Description = $"{X:F2}";
            } else if (sliderItem == YSlider)
            {
                _y = newValue;
                YSlider.Description = $"{Y:F2}";
            } else if (sliderItem == ZSlider)
            {
                _z = newValue;
                ZSlider.Description = $"{Z:F2}";
            }
        }
    }
}