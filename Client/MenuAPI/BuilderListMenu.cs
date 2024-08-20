using System;
using System.Collections.Generic;
using System.Drawing;
using CitizenFX.Core;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public class BuilderListMenu<T> : BuilderMenu
    {
        public delegate void SelectedItemEvent(object sender, int index, T selectedItem);

        private List<T> items;
        public int Index => Menu.CurrentSelection;

        public T SelectedItem => items[Index];
        public UIMenuItem SelectedMenuItem => Menu.CurrentItem;

        private string headerText;
        
        public bool ShowSelection { get; set; }
        private int _lastIndex;

        public BuilderListMenu(List<T> items, string title, string header, string description, PointF offset) : base(title, header, description,
            offset)
        {
            headerText = header;
            Menu.ScrollingType = ScrollingType.PAGINATED;
            Menu.BuildingAnimation = MenuBuildingAnimation.NONE;
            Menu.MaxItemsOnScreen = 15;
            this.items = items;
            

            Menu.OnItemSelect += (sender, item, index) =>
            {
                OnItemSelected?.Invoke(this, this.Index, SelectedItem);
            };

            Menu.OnIndexChange += (sender, index) =>
            {
                OnItemHovered?.Invoke(this, this.Index, SelectedItem);
                if(ShowSelection)
                    Item.Label = $"{headerText}: {SelectedItem.ToString()}";
            };

            Menu.OnMenuOpen += (menu, data) =>
            {
                // Repopulate items
                foreach (T item in items)
                {
                    Menu.AddItem(new UIMenuItem(item.ToString()));
                }
                // Remove "list is empty" if we have items
                if (items.Count > 0)
                {
                    Menu.RemoveItemAt(0);
                    // If the list has shrunk and selection no longer exists, select the last item
                    if (_lastIndex >= items.Count)
                    {
                        _lastIndex = items.Count - 1;
                    }
                }
                else
                {
                    _lastIndex = 0;
                }
                Debug.WriteLine($"List OnMenuOpen count={{items.Count}} lastIndex={{_lastIndex}} currentSel={Menu.CurrentSelection}");
                Menu.CurrentSelection = _lastIndex;
                OnMenuOpen?.Invoke(menu, data);
                if (items.Count > 0)
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
                Menu.AddItem(new UIMenuItem("List is empty") { Enabled = false });
            };
            Menu.AddItem(new UIMenuItem("List is empty") { Enabled = false });
        }

        public event SelectedItemEvent OnItemSelected;
        public event SelectedItemEvent OnItemHovered;
        public event MenuOpenedEvent OnMenuOpen;
        public event MenuClosedEvent OnMenuClose;

        public void AddItem(T item)
        {
            items.Add(item);
        }

        public void RemoveItem(T item)
        {
            items.Remove(item);
        }

        public void RemoveItem(int index)
        {
            items.RemoveAt(index);
        }

        public T GetItem(int index)
        {
            return items[index];
        }

        public bool HasItem(T item)
        {
            return items.Contains(item);
        }

    }
    
}