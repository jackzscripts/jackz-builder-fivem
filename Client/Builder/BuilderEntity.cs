using System;
using CitizenFX.Core;
using ScaleformUI.Menu;
using test_project.Client.MenuAPI;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using test_project.Client.ExtensionMethods;

namespace test_project.Client
{
    public class BuilderEntity
    {
        public enum EntityType
        {
            /// <summary>
            /// Unknown, base entity
            /// </summary>
            Entity,
            Prop,
            Vehicle,
            Ped
        }

        [JsonProperty("id")] public int Id { get; }
        
        [JsonProperty("offset")]
        public Vector3 Offset;
        
        [JsonProperty("parentId")]
        public int? ParentId => ParentEntity?.Id; 
        public BuilderEntity ParentEntity = null;
        
        public EntityType Type = EntityType.Entity;
        public Entity Entity { get; private set; }
        public MenuAPI.BuilderMenu Menu { get; private set; }
        public UIMenuItem MenuItem => Menu.Item;
        
        public BuilderEntity(Entity entity, string name = "")
        {
            Id = ClientMain.Builder.GetNextId();
            Offset = entity.Position.Clone();
            Entity = entity;
            name = string.IsNullOrEmpty(name) ? entity.Handle.ToString() : name;
            Menu = new MenuAPI.BuilderMenu(name, name, "", ClientMain.MenuPosition);

            if (API.IsModelAVehicle(Entity.Model))
            {
                Type = EntityType.Vehicle;
            } else if (API.IsModelAPed(Entity.Model))
            {
                Type = EntityType.Ped;
            } else if (API.IsEntityAnObject(Entity.Handle))
            {
                Type = EntityType.Prop;
            }
            
            Menu.Menu.OnMenuOpen += (menu, data) =>
            {
                PopulateDefaultItems();
                PopulateItems();
                if(menu.MenuItems.Count > 0)
                    menu.RemoveItemAt(0);
            };
            Menu.Menu.OnMenuClose += (menu) =>
            {
                menu.Clear();
                Menu.AddItem(new UIMenuItem("List is empty") { Enabled = false });
            };
            Menu.AddItem(new UIMenuItem("List is empty") { Enabled = false });
        }

        private void PopulateDefaultItems()
        {
            // TODO: populate default/shared items
            Menu.AddItem(new UIMenuSeparatorItem("Position", false));
            Vector3MenuItem positionItem = new Vector3MenuItem(Offset, Menu, "Position", ClientMain.Builder.StepSize);
            positionItem.OnVectorChanged += value =>
            {
                Offset = value;
                Attach();
            };
            
            Menu.AddItem(new UIMenuSeparatorItem("Rotation", false));
            Vector3MenuItem rotationItem = new Vector3MenuItem(Entity.Rotation, Menu, "Rotation", 1f);
            rotationItem.OnVectorChanged += value =>
            {
                Entity.Rotation = value;
                Attach();
            };
            
            
        }

        protected virtual void PopulateItems()
        {
            
        }

        public override string ToString()
        {
            return Menu.Item.Label;
        }

        public void Attach()
        {
            if (ParentEntity == null)
            {
                Entity.Position = Offset;
            }
            else
            {
                Entity.AttachTo(ParentEntity.Entity, Offset, Entity.Rotation);
            }
        }
    }
}