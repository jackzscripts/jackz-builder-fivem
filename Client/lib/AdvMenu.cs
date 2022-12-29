using System.Collections.Generic;
using System.Linq;
using MenuAPI;

namespace jackz_builder.Client.lib
{
    public delegate void ItemSelectedCallback(int itemIndex);
    public delegate void SliderValueChangedCallback(int itemIndex, int oldValue, int newValue);
    public delegate void SliderClickedCallback(int itemIndex);
    public delegate void CheckboxValueChangedCallback(int itemIndex, bool active);
    public delegate void ListMenuItemSelectedCallback(int itemIndex, int selectedIndex);
    public delegate void ListMenuItemIndexChangedCallback(int itemIndex, int oldSelectedIndex, int newSelectedIndex);

    public delegate void DynamicListMenuItemSelectedCallback(MenuDynamicListItem item, string currentItem);
    public delegate void DynamicListMenuItemIndexChangedCallback(MenuDynamicListItem item, string oldValue, string newValue);
    
    internal class ListMenuCallbacks
    {
        public ListMenuItemSelectedCallback SelectCallback;
        public ListMenuItemIndexChangedCallback IndexChangedCallback;
    }
    
    internal class DynamicListMenuCallbacks
    {
        public DynamicListMenuItemSelectedCallback SelectCallback;
        public DynamicListMenuItemIndexChangedCallback IndexChangedCallback;
    }
    
    public class AdvMenu : Menu
    {
        public new Menu ParentMenu;
        public AdvMenu AdvParentMenu;

        /// <summary>
        /// The <see cref="MenuItem"/> that links from its <see cref="Menu.ParentMenu" /> links to this menu
        /// </summary>
        public MenuItem SubmenuEntry;
        
        public new int CurrentIndex { get; set; }

        public AdvMenu(string title, string description = null) : base(title, description)
        {
            OnCheckboxChange += _OnCheckboxValueChanged;
            OnItemSelect += _OnItemSelect;
            OnSliderPositionChange += _OnSliderValueChanged;
            OnListItemSelect += _OnListItemSelected;
            OnListIndexChange += _OnListIndexChange;
            OnDynamicListItemSelect += _OnDynamicListItemSelected;
            OnDynamicListItemCurrentItemChange += _OnDynamicListItemChanged;
        }

        public new void ClearMenuItems()
        {
            itemMenus.Clear();
            sliderMenus.Clear();
            checkboxMenus.Clear();
            listMenus.Clear();
            dynamicListMenus.Clear();
            base.ClearMenuItems();
        }

        public void SetParent(Menu menu)
        {
            ParentMenu = menu;
        }
        public void SetParent(AdvMenu menu)
        {
            AdvParentMenu = menu;
            ParentMenu = menu;
        }

        public void CreateParentEntry(Menu parent)
        {
            MenuController.AddSubmenu(parent, this);
            SubmenuEntry = new MenuItem(MenuTitle, MenuSubtitle);
            MenuController.BindMenuItem(parent, this, SubmenuEntry);
            parent.AddMenuItem(SubmenuEntry);
            this.SetParent(parent);
        }
        
        private Dictionary<MenuItem, ItemSelectedCallback> itemMenus = new Dictionary<MenuItem, ItemSelectedCallback>();
        private Dictionary<MenuItem, SliderValueChangedCallback> sliderMenus = new Dictionary<MenuItem, SliderValueChangedCallback>();
        private Dictionary<MenuItem, CheckboxValueChangedCallback> checkboxMenus = new Dictionary<MenuItem, CheckboxValueChangedCallback>();
        private Dictionary<MenuItem, ListMenuCallbacks> listMenus = new Dictionary<MenuItem, ListMenuCallbacks>();
        private Dictionary<MenuItem, DynamicListMenuCallbacks> dynamicListMenus = new Dictionary<MenuItem, DynamicListMenuCallbacks>();

        /// <summary>
        /// Creates a new submenu with it's entry in the parent
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public Menu CreateSubMenu(string title, string description = null)
        {
            var submenu = new Menu(this.MenuTitle, title);
            MenuController.AddSubmenu(this, submenu);
            var entry = new MenuItem(title, description);
            MenuController.BindMenuItem(this, submenu, entry);
            this.AddMenuItem(entry);
            return submenu;
        }
        /// <summary>
        /// Creates a new submenu with it's entry in the parent, and returns a new AdvMenu
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public AdvMenu CreateAdvSubMenu(string title, string description = null)
        {
            var submenu = new AdvMenu(MenuTitle, title);
            submenu.AdvParentMenu = this;
            MenuController.AddSubmenu(this, submenu);
            var entry = new MenuItem(title, description);
            submenu.SubmenuEntry = entry;
            MenuController.BindMenuItem(this, submenu, entry);
            this.AddMenuItem(entry);
            return submenu;
        }

        public MenuItem AddDivider(string title = "")
        {
            return AddDivider(title, MenuItem.Icon.INFO);
        } 
        public MenuItem AddDivider(string title, MenuItem.Icon icon)
        {
            var entry = new MenuItem("")
            {
                Enabled = false,
                Label = title,
                LeftIcon = icon,
            };
            AddMenuItem(entry);
            return entry;
        } 
        
        /// <summary>
        /// Adds a <see cref="MenuItem"/> to this <see cref="Menu"/>.
        /// </summary>
        /// <param name="item"></param>
        public new MenuItem AddMenuItem(MenuItem item)
        {
            base.AddMenuItem(item);
            return item;
        }
        public MenuItem AddMenuItem(MenuItem item, ItemSelectedCallback callback)
        {
            itemMenus.Add(item, callback);
            base.AddMenuItem(item);
            return item;
        }
        public MenuSliderItem AddMenuItem(MenuSliderItem item, SliderValueChangedCallback callback)
        {
            sliderMenus.Add(item, callback);
            base.AddMenuItem(item);
            return item;
        }
        
        public MenuCheckboxItem AddMenuItem(MenuCheckboxItem item, CheckboxValueChangedCallback callback)
        {
            checkboxMenus.Add(item, callback);
            base.AddMenuItem(item);
            return item;
        }
        
        public MenuListItem AddMenuItem(MenuListItem item, ListMenuItemSelectedCallback callback, ListMenuItemIndexChangedCallback indexChangedCallback = null)
        {
            listMenus.Add(item, new ListMenuCallbacks
            {
                IndexChangedCallback = indexChangedCallback,
                SelectCallback = callback
            });
            base.AddMenuItem(item);
            return item;
        }
        
        public MenuDynamicListItem AddMenuItem(MenuDynamicListItem item, DynamicListMenuItemSelectedCallback callback, DynamicListMenuItemIndexChangedCallback indexChangedCallback = null)
        {
            dynamicListMenus.Add(item, new DynamicListMenuCallbacks
            {
                IndexChangedCallback = indexChangedCallback,
                SelectCallback = callback
            });
            base.AddMenuItem(item);
            return item;
        }

        private void _OnItemSelect(Menu _menu, MenuItem menuItem, int itemIndex)
        {
            foreach (var kv in itemMenus.Where(kv => menuItem == kv.Key))
            {
                kv.Value(itemIndex);
                break;
            }
        }

        private void _OnSliderValueChanged(Menu _menu, MenuSliderItem sliderItem, int oldValue, int newValue, int itemIndex)
        {
            foreach (var kv in sliderMenus.Where(kv => sliderItem == kv.Key))
            {
                kv.Value(itemIndex, oldValue, newValue);
                break;
            }
        }
        private void _OnCheckboxValueChanged(Menu menu, MenuItem item, int itemIndex, bool active)
        {
            foreach (var kv in checkboxMenus.Where(kv => item == kv.Key))
            {
                kv.Value(itemIndex, active);
                break;
            }
        }
        
        
        private void _OnListItemSelected(Menu _menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            foreach (var kv in listMenus.Where(kv => listItem == kv.Key))
            {
                kv.Value.SelectCallback(itemIndex, selectedIndex);
                break;
            }
        }
        
        private void _OnListIndexChange(Menu _menu, MenuListItem listItem, int oldSelectIndex, int newSelectIndex, int itemIndex)
        {
            foreach (var kv in listMenus.Where(kv => listItem == kv.Key))
            {
                kv.Value.IndexChangedCallback.Invoke(itemIndex, oldSelectIndex, newSelectIndex);
                break;
            }
        }
        
        private void _OnDynamicListItemSelected(Menu _menu, MenuDynamicListItem listItem, string currentItem)
        {
            foreach (var kv in dynamicListMenus.Where(kv => listItem == kv.Key))
            {
                kv.Value.SelectCallback.Invoke(listItem, currentItem);
                break;
            }
        }

        private void _OnDynamicListItemChanged(Menu _menu, MenuDynamicListItem listItem, string oldValue, string newValue)
        {
            foreach (var kv in dynamicListMenus.Where(kv => listItem == kv.Key))
            {
                kv.Value.IndexChangedCallback.Invoke(listItem, oldValue, newValue);
                break;
            }
        }
    }
}