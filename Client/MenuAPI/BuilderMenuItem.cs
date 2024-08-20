using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public abstract class BuilderMenuItem
    {
        private UIMenuItem _item;

        public UIMenuItem Item => _item;
        
        public string Label { get => _item.Label; set => _item.Label = value; }
        public string Description { get => _item.Description; set => _item.Description = value; }
        public bool Enabled { get => _item.Enabled; set => _item.Enabled = value; }

        public BuilderMenuItem(UIMenuItem item)
        {
            _item = item;
        }

        public BuilderMenuItem(string title) : this(new UIMenuItem(title))
        {
        }
    }
}