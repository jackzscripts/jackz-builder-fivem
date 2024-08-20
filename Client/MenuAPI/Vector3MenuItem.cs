using CitizenFX.Core;

namespace jackz_builder.Client.MenuAPI
{
    public class Vector3MenuItem
    {
        private Vector3 _vector;

        public Vector3 Vector
        {
            get => _vector;
            set => _vector = value;
        }

        public float StepSize { get; } = 1.0f;

        public delegate void VectorChangeEvent(Vector3 value);

        public Vector3MenuItem(Vector3 vector, BuilderMenu parentMenu, string label, float stepSize)
        {
            Vector = vector;
            StepSize = StepSize;
            BuilderMenuFloatSliderItem xItem =
                new BuilderMenuFloatSliderItem($"{label} (X)", "", Vector.X, StepSize);
            xItem.OnValueChange += (sender, value) =>
            {
                _vector.X = value;
                OnVectorChanged?.Invoke(Vector);
            };
            parentMenu.AddItem(xItem);
            
            BuilderMenuFloatSliderItem yItem =
                new BuilderMenuFloatSliderItem($"{label} (Y)", "", Vector.Y, StepSize);
            yItem.OnValueChange += (sender, value) =>
            {
                _vector.Y = value;
                OnVectorChanged?.Invoke(Vector);
            };
            parentMenu.AddItem(yItem);
            
            BuilderMenuFloatSliderItem zItem =
                new BuilderMenuFloatSliderItem($"{label} (Z)", "", Vector.Z, StepSize);
            zItem.OnValueChange += (sender, value) =>
            {
                _vector.Z = value;
                OnVectorChanged?.Invoke(Vector);
            };
            parentMenu.AddItem(zItem);
        }
        
        public event VectorChangeEvent OnVectorChanged;
    }
}