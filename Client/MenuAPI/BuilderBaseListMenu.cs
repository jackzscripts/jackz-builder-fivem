using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public abstract class BuilderBaseListMenu<T> : BuilderMenu
    {
        public delegate void SelectedItemEvent(object sender, int index, T selectedItem);

        protected List<T> Items;
        public int Index => Menu.CurrentSelection;
        public T SelectedItem => Items[Index];
        
        public UIMenuItem SelectedMenuItem => Menu.CurrentItem;
        
        /// <summary>
        /// The message shown when there are no items in the list or loading
        /// </summary>
        public string NoItemMessageText { get; set; } = "No items found";

        public bool ShowSelection { get; set; }
        private int _lastIndex;

        public BuilderBaseListMenu(string title, string header, string description, PointF offset) : base(title, header, description,
            offset)
        {
            Menu.ScrollingType = ScrollingType.PAGINATED;
            Menu.BuildingAnimation = MenuBuildingAnimation.NONE;
            Menu.MaxItemsOnScreen = 15;

            Menu.OnItemSelect += (sender, item, index) =>
            {
                OnItemSelected?.Invoke(this, this.Index, SelectedItem);
            };

            Menu.OnIndexChange += (sender, index) =>
            {
                OnItemHovered?.Invoke(this, this.Index, SelectedItem);
                if(ShowSelection)
                    Item.Label = $"{header}: {SelectedItem.ToString()}";
            };

            Menu.OnMenuOpen += async (menu, data) =>
            {
                Items = await PopulateItems();
                // Repopulate items
                foreach (T item in Items)
                {
                    Menu.AddItem(new UIMenuItem(item.ToString()));
                }
                // Remove "list is empty" if we have items
                if (Items.Count > 0)
                {
                    Menu.RemoveItemAt(0);
                    // If the list has shrunk and selection no longer exists, select the last item
                    if (_lastIndex >= Items.Count)
                    {
                        _lastIndex = Items.Count - 1;
                    }
                }
                else
                {
                    _lastIndex = 0;
                }
                
                Menu.CurrentSelection = _lastIndex;
                OnMenuOpen?.Invoke(menu, data);
                if (Items?.Count > 0)
                {
                    OnItemHovered?.Invoke(this, this.Index, SelectedItem);
                }
            };
            Menu.OnMenuClose += menu =>
            {
                _lastIndex = menu.CurrentSelection;
                Menu.Clear();
                OnMenuClose?.Invoke(menu);
                // Need to add an item so we can open it
                Menu.AddItem(new UIMenuItem(NoItemMessageText) { Enabled = false });
            };
            Menu.AddItem(new UIMenuItem(NoItemMessageText) { Enabled = false });
        }

        protected virtual Task<List<T>> PopulateItems()
        {
            return Task.FromResult(Items);
        }

        public event SelectedItemEvent OnItemSelected;
        public event SelectedItemEvent OnItemHovered;
        public event MenuOpenedEvent OnMenuOpen;
        public event MenuClosedEvent OnMenuClose;
    }
}