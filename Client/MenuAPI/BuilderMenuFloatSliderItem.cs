using System.Threading.Tasks;
using ScaleformUI.Menu;

namespace jackz_builder.Client.MenuAPI
{
    public class BuilderMenuFloatSliderItem : UIMenuDynamicListItem
    {
        public float Value { get; set; }
        public float? MinValue { get; set; }
        public float? MaxValue { get; set; }
        
        private float StepSize { get; set; }
        
        /// <summary>
        /// The number of decimal places to show
        /// </summary>
        public int Precision { get; set; } = 4;

        public delegate void ValueChangeEvent(BuilderMenuFloatSliderItem sender, float value);
        
        public BuilderMenuFloatSliderItem(string text, string description, float startValue, float stepSize = 1.0f) :
            base(text, description, "", UpdateDisplay)
        {
            Value = startValue;
            CurrentListItem = startValue.ToString($"F{Precision}");
            StepSize = stepSize;
        }

        private static Task<string> UpdateDisplay(UIMenuDynamicListItem sender, ChangeDirection direction)
        {
            BuilderMenuFloatSliderItem item = (BuilderMenuFloatSliderItem)sender;
            if (direction == ChangeDirection.Left)
            {
                item.Value -= item.StepSize;
                if (item.MinValue.HasValue && item.Value <= item.MinValue.Value)
                {
                    item.Value = item.MinValue.Value;
                }
            }
            else
            {
                item.Value += item.StepSize;
                if (item.MaxValue.HasValue && item.Value <= item.MaxValue.Value)
                {
                    item.Value = item.MaxValue.Value;
                }
            }
            item.OnValueChange?.Invoke(item, item.Value);

            return Task.FromResult(item.Value.ToString($"F{item.Precision}"));
        }
        
        public event ValueChangeEvent OnValueChange;

    }
}