using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using CitizenFX.Core;
using ScaleformUI;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public delegate void ItemClicked(UIMenuItem item, int index);
    public class BuilderMenu
    {
        private UIMenuItem _item;
        private Dictionary<UIMenuItem, ItemClicked> _menuHandlers = new Dictionary<UIMenuItem, ItemClicked>();
        private List<BuilderMenu> _children = new List<BuilderMenu>();
        public BuilderMenu(string title, string header, string description, PointF offset)
        {
            Menu = new UIMenu("", header, offset)
            {
                EnableAnimation = false,
                AnimationType = MenuAnimationType.LINEAR,
                BuildingAnimation = MenuBuildingAnimation.NONE,
                Enabled3DAnimations = false
            };
            _item = new UIMenuItem(title, description);
            Menu.OnItemSelect += (sender, item, index) =>
            {
                Debug.WriteLine($"item clicked at {index}");
                Debug.WriteLine($"{sender.Subtitle}/{item.Label} clicked");
                ItemClicked callback;
                if (_menuHandlers.TryGetValue(item, out callback))
                {
                    Debug.WriteLine($"Got callback for {item}");
                    callback(item, index);
                }
                else
                {
                    BuilderMenu child = _children.Find(c => c.Item == item);
                    if (child != null)
                    {
                        Menu.SwitchTo(child.Menu);
                        Menu.Visible = false;
                    }
                }
            };
            Menu.OnMenuClose += (menu) =>
            {
                if (ParentMenu != null)
                {
                    menu.SwitchTo(ParentMenu);
                }
            };
        }

        public BuilderMenu(UIMenu parent, string title, string header, string description, PointF offset) : this(title, header, description, offset)
        {
            ParentMenu = parent;
        }
        
        public UIMenuItem Item => _item;

        public UIMenu Menu { get; }

        public UIMenu ParentMenu { get; set; }
        public BuilderMenu ParentBuilderMenu { get; set; }
        
        public bool Visible
        {
            get => Menu.Visible; 
            set => Menu.Visible = value;
        }
        
        public void AddBuilderItem(BuilderMenuItem item)
        {
            this.Menu.AddItem(item.Item);
        }

        public void AddItem(UIMenuItem item)
        {
            this.Menu.AddItem(item);
        }
        
        public void AddItemHandler(UIMenuItem item, ItemClicked callback)
        {
            this.Menu.AddItem(item);
            this._menuHandlers.Add(item, callback);
        }

        public UIMenuItem CreateItem(string title, string description = "")
        {
            UIMenuItem item = new UIMenuItem(title)
            {
                Description = description,
            };
            Menu.AddItem(item);
            return item;
        }

        public UIMenuItem CreateItemHandler(string title, ItemClicked callback, string description = "")
        {
            UIMenuItem item = this.CreateItem(title, description);
            this._menuHandlers.Add(item, callback);
            return item;
        }

        public void AddChildMenu(BuilderMenu menu, string label = "")
        {
            menu.ParentMenu = Menu;
            menu.ParentBuilderMenu = this;
            
            this._children.Add(menu);
            if(!string.IsNullOrEmpty(label))
                menu.Item.Label = label;
            this.AddItem(menu.Item);
        }
        
    }
}