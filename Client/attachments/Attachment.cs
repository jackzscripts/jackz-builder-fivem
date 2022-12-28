using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using ExtensionMethods;
using jackz_builder.Client.JackzBuilder.Spawners;
using jackz_builder.Client.lib;
using MenuAPI;
using Newtonsoft.Json;

namespace jackz_builder.Client.JackzBuilder
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Attachment
    {
        public AdvMenu Root;
        public Entity ParentEntity;
        private Entity _entity;

        public Entity Entity
        {
            get => _entity;
            set
            {
                _entity = value;
                Type = BuilderMain.GetType(value);
                Model = value.Model;

            }
        }
        public List<Menu> ListMenus;


        /// <summary>
        /// Determines if the attachment is editable (has an entity tied to it) or is just data (a saved attachment)
        /// </summary>
        public bool IsEditable => Entity != null;

        /// <summary>
        /// Is the entity attached to any vehicle (false) or to the world (true)
        /// </summary>
        public bool IsAttachedToWorld => ParentEntity == null;

        [JsonProperty("id")] public int Id;
        public EntityType Type { get; private set; }
        [JsonProperty("type")] private string _type
        {
            get
            {
                switch (Type)
                {
                    case EntityType.Ped: return "ped";
                    case EntityType.Prop: return "object";
                    case EntityType.Vehicle: return "vehicle";
                    default: return "entity";
                }
            }
            set
            {
                switch (value)
                {
                    case "ped": 
                        Type = EntityType.Ped;
                        break;
                    case "object":
                        Type = EntityType.Prop;
                        break;
                    case "vehicle":
                        Type = EntityType.Vehicle;
                        break;
                }
            }
        }

        [JsonProperty("model")] public int Model;
        [JsonProperty("offset")] public SerializedVector3 PositionOffset = Vector3.Zero;
        [JsonProperty("rotation")] public SerializedVector3 Rotation = Vector3.Zero;
        [JsonProperty("visible")] public bool Visible = true;
        [JsonProperty("godmode")] public bool Godmode = true;

        [JsonProperty("collision")] public bool Collision;
        [JsonProperty("boneIndex")] public int BoneIndex;

        [JsonProperty("name")] private string _name;

        [JsonProperty("savedata")] private string _saveData;
        public VehicleInfo GetSavedSaveData()
        {
            return _saveData == null ? null : JsonConvert.DeserializeObject<VehicleInfo>(_saveData);
        }

        public void Export()
        {
            _saveData = JsonConvert.SerializeObject(VehicleManager.SerializeVehicle((Vehicle)Entity));
        }

        public string Name
        {
            set
            {
                _name = value;
                if (Root != null)
                {
                    Root.MenuSubtitle = value;
                    Root.SubmenuEntry.Text = Name;
                }
            }
            get => _name;
        }

        private MenuItem RenameMenu;
        private MenuSliderItem SliderPosX;
        private MenuSliderItem SliderPosY;
        private MenuSliderItem SliderPosZ;
        private MenuSliderItem SliderRotX;
        private MenuSliderItem SliderRotY;
        private MenuSliderItem SliderRotZ;

        /// <summary>
        /// Spawns an entity for this attachment, based on this attachment data, used for loading a build.
        /// Will populate the 'Entity' field
        /// </summary>
        public virtual async Task<Entity> Spawn(bool isPreview)
        {
            if (!API.IsModelValid((uint)Model))
            {
                Debug.WriteLine($"{GetType()}.Spawn: Attachment(#{Id}, Name={Name}) model \"{Model}\" is invalid, skipping");
                return null;
            }
            await Util.RequestModel((uint)Model);
            Debug.WriteLine($"Spawning attachment(#{Id}, Name={Name}) Preview={isPreview} with Type={Type.ToString()}");
            switch (Type)
            {
                case EntityType.Ped:
                    Entity = await PedSpawner.CreatePed((uint)Model, Vector3.Zero, Rotation, isPreview);
                    break;
                case EntityType.Prop:
                    Entity = await PropSpawner.CreateProp((uint)Model, Vector3.Zero, Rotation, isPreview);
                    break;
                case EntityType.Vehicle:
                    var saveData = GetSavedSaveData();
                    var vehicle = await VehicleSpawner.CreateVehicle((uint)Model, Vector3.Zero, Rotation, isPreview);
                    if (vehicle == null)
                    {
                        Debug.WriteLine("<Attachment>.Spawn for Vehicle is null");
                    }
                    // TODO: Figure out why entity null
                    if(saveData != null)
                        VehicleManager.ApplyToVehicle(vehicle, saveData);
                    Entity = (Entity) vehicle;
                    break;
            }

            if (Entity == null)
            {
                throw new Exception(
                    $"<{GetType()}>.Spawn() called on attachment of an unsupported type ({Type.ToString()}). Possibly <{GetType()}> should be an instance of a child class instead, or entity type is not supported");
            }
            else
            {
                Attach();
                SetupEntity();
            }
            return Entity;
        }

        protected void SetupEntity()
        {
            Entity.IsVisible = Visible;
            Entity.IsInvincible = Godmode;
        }
        
        /// <summary>
        /// Creates a new attachment - tied to an pre-existing entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentEntity"></param>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        public Attachment(int id, Entity parentEntity, Entity entity, string name)
        {
            Id = id;
            ParentEntity = parentEntity;
            Entity = entity;
            Model = entity.Model;
            Type = BuilderMain.GetType(entity);
            Name = name;
        }

        /// <summary>
        /// Loads a new attachment, which may be invalid (Entity null)
        /// </summary>
        [JsonConstructor]
        public Attachment()
        {
        }

        public void ComputeSize(ref float rSize, ref float hSize)
        {
            var size = ((Model)Model).GetDimensions();
            size.X += PositionOffset.X;
            size.Y += PositionOffset.Y;
            if (PositionOffset.Z < 0f)
            {
                size.Z += Math.Abs(PositionOffset.Z);
            }
            else
            {
                PositionOffset.Z = 0f;
            }

            if (size.X > rSize)
            {
                rSize = size.X;
            }
            if (size.Y > rSize)
            {
                rSize = size.Y;
            }
            if (size.Z > hSize)
            {
                hSize = size.Z;
            }

        }

        /// <summary>
        /// (Re-)attach the entity to it's parent/the world according to the offset or rotation
        /// Call this when the offset, rotation, parent, or the bone index changes
        /// </summary>
        public void Attach()
        {
            // If entity is the base entity for the build:
            if (Id == -1 || ParentEntity == null)
            {
                Entity.Rotation = Rotation;
            } else if (IsAttachedToWorld)
            {
                Entity.Detach();
                var worldPos = CurrentBuildMenu.Build.Base.Entity.Position.Clone() + PositionOffset;
                Entity.Position = worldPos;
            }
            else
            {
                API.AttachEntityToEntity(Entity.Handle, ParentEntity.Handle, BoneIndex, PositionOffset.X, PositionOffset.Y, PositionOffset.Z, Rotation.X, Rotation.Y, Rotation.Z, false, false, Collision, false, 2, true);
            }
        }

        public AdvMenu CreateEditMenu(AdvMenu parentMenu)
        {
            Root = parentMenu.CreateAdvSubMenu(Name ?? Type.ToString(), $"Edit this {Type.ToString()} {Entity.Handle}");
            Root.OnMenuOpen += OnMenuOpen;
            Root.OnMenuClose += OnMenuClose;
            Root.SubmenuEntry.Text = Name;
            Root.SubmenuEntry.LeftIcon = GetIcon();
            return Root;
        }
        
        /// <summary>
        /// Determines the icon that shows on the left side of the AttachmentsMenu
        /// </summary>
        /// <returns></returns>
        private MenuItem.Icon GetIcon()
        {
            switch (Type)
            {
                case EntityType.Ped:
                    return API.IsPedMale(Entity.Handle) ? MenuItem.Icon.MALE : MenuItem.Icon.FEMALE;
                case EntityType.Prop:
                    return MenuItem.Icon.AMMO;
                case EntityType.Vehicle:
                    return MenuItem.Icon.CAR;
                default:
                    return MenuItem.Icon.INV_QUESTIONMARK;
            }
        }
        
        /// <summary>
        /// Load the attachment's edit menu - with all it's settings
        /// </summary>
        /// <param name="menu"></param>
        private void OnMenuOpen(Menu menu)
        {
            BuilderMain.HighlightedEntity = Entity;
            #region Position
            if (Entity != CurrentBuildMenu.Build.Base.Entity)
            {
                Root.AddDivider("Position");
                SliderPosX = Root.AddMenuItem(new MenuSliderItem("Front / Back", $"Sets the X offset from the base entity\n{PositionOffset.X}",
                    -20000,
                    20000, (int)(PositionOffset.Y * BuilderMain.EditorSensitivity)), ((itemIndex, value, newValue) =>
                {
                    PositionOffset.X = newValue / BuilderMain.EditorSensitivity;
                    SliderPosX.Description = $"Sets the X offset from the base entity\n{PositionOffset.X}";
                    Attach();
                }));
                SliderPosY = Root.AddMenuItem(new MenuSliderItem("Left / Right", $"Sets the Y offset from the base entity\n{PositionOffset.Y}",
                    -20000,
                    20000, (int)(PositionOffset.X * BuilderMain.EditorSensitivity)), ((itemIndex, value, newValue) =>
                {
                    PositionOffset.Y = newValue / BuilderMain.EditorSensitivity;
                    SliderPosY.Description = $"Sets the Y offset from the base entity\n{PositionOffset.Y}";
                    Attach();
                }));
                SliderPosZ = Root.AddMenuItem(new MenuSliderItem("Up / Down", $"Sets the Z offset from the base entity\n{PositionOffset.Z}", -20000,
                    20000, (int)(PositionOffset.Z * BuilderMain.EditorSensitivity)), ((itemIndex, value, newValue) =>
                {
                    PositionOffset.Z = newValue / BuilderMain.EditorSensitivity;
                    SliderPosZ.Description = $"Sets the Z offset from the base entity\n{PositionOffset.Z}";
                    Attach();
                }));
            }
            #endregion
            
            #region Rotation
            Root.AddDivider("Rotation");
            SliderRotX = Root.AddMenuItem(new MenuSliderItem("Pitch", $"Sets the X-axis rotation\n{Rotation.X}",
                -175,
                180, (int)Rotation.X), ((itemIndex, value, newValue) =>
            {
                Rotation.X = newValue;
                SliderRotX.Description = $"Sets the X-axis rotation\n{Rotation.X}";
                Attach();
            }));
            SliderRotY = Root.AddMenuItem(new MenuSliderItem("Roll", $"Sets theY-axis rotation\n{Rotation.Y}",
                -175,
                180, (int)Rotation.Y), ((itemIndex, value, newValue) =>
            {
                Rotation.Y = newValue;
                SliderRotY.Description = $"Sets the X-axis rotation\n{Rotation.Y}";
                Attach();
            }));
            SliderRotZ = Root.AddMenuItem(new MenuSliderItem("Yaw", $"Sets the Z-axis rotation\n{Rotation.Z}", 
                -175,
                180, (int)Rotation.Z), ((itemIndex, value, newValue) =>
            {
                Rotation.Z = newValue;
                SliderRotZ.Description = $"Sets the X-axis rotation\n{Rotation.Z}";
                Attach();
            }));
            #endregion

            Root.AddDivider("Misc");
            Root.AddMenuItem(new MenuItem("Attach To:")
            {
                Enabled = false,
                Label = "Base"
            });
            RenameMenu = Root.AddMenuItem(new MenuItem("Rename")
            {
                Label = Name
            }, async (index) =>
            {
                Name = await Util.GetUserInput($"Rename Entity {Entity.Handle.ToString()}", Name, 30);
                RenameMenu.Label = Name;
            });
            Root.AddMenuItem(
                new MenuCheckboxItem("Collision", "Toggles if this entity will have collision, default is enabled", Collision),
                (index, active) =>
                {
                    Collision = active;
                    Attach();
                });
            Root.AddMenuItem(
                new MenuCheckboxItem("Visibile", "Toggles if this entity is visible", Visible),
                (index, active) =>
                {
                    Visible = active;
                    Entity.IsVisible = active;
                });
            if (Type == EntityType.Vehicle)
            {
                Root.AddMenuItem(
                    new MenuCheckboxItem("Godmode", "Toggles if this entity will have godmode", Godmode),
                    (index, active) =>
                    {
                        Godmode = active;
                        API.SetEntityInvincible(Entity.Handle, active);
                    });
                Root.AddMenuItem(new MenuItem("Enter Vehicle"), index =>
                {
                    Game.PlayerPed.SetIntoVehicle((Vehicle) Entity, VehicleSeat.Any);
                });
            }

            CreateCloneMenu();

            #region Danger Zone
            if (Entity != CurrentBuildMenu.Build.Base.Entity)
            {

                Root.AddDivider("Danger Zone", MenuItem.Icon.WARNING);

                var DeleteMenu = Root.AddMenuItem(new MenuItem("Delete")
                {
                    LeftIcon = MenuItem.Icon.DEATHMATCH,
                }, async index =>
                {
                    // TODO: Better delete confirm
                    var confirm = await Util.GetUserInput("Confirm Deletion: Enter \"delete\"", "", 10);
                    if (confirm.ToLower() == "delete")
                    {
                        this.Root.CloseMenu();
                        BuilderMain.CurrentBuildMenu.DeleteAttachment(this);
                    }
                });
            }
            #endregion
        }

        private void CreateCloneMenu()
        {
            var cloneMenu = Root.CreateSubMenu("Clone", "Create a copy of this build");
            cloneMenu.OnItemSelect += async (menu, item, index) =>
            {
                await Clone(index);
            };
            
            cloneMenu.AddMenuItem(new MenuItem("Clone In-place"));
            cloneMenu.AddMenuItem(new MenuItem("Mirror (X, Left/Right)", "Clones the entity, mirrored on the x-axis"));
            cloneMenu.AddMenuItem(new MenuItem("Mirror (Y, Forward/Back)", "Clones the entity, mirrored on the y-axis"));
            cloneMenu.AddMenuItem(new MenuItem("Mirror (Z, Up/Down)", "Clones the entity, mirrored on the z-axis"));
        }

        /// <summary>
        /// Clone the attachment to the parent
        /// </summary>
        /// <param name="mirrorMode">If non zero, determines the axis (X, Y, Z) to clone off</param>
        /// <returns></returns>
        /// <exception cref="Exception">If attachment is an orphan</exception>
        private async Task<Attachment> Clone(int mirrorMode)
        {
            Vector3 pos;
            if (mirrorMode > 0)
            {
                if (!CurrentBuildMenu.Build.Attachments.ContainsKey(Entity))
                {
                    Debug.WriteLine("Error: CloneEntity with mirrorMode set on non-builder entity");
                    return null;
                }

                pos = new Vector3(PositionOffset.X, PositionOffset.Y, PositionOffset.Z);
                switch (mirrorMode)
                {
                    case 1: 
                        pos.X = -pos.X;
                        break;
                    case 2:
                        pos.Y = -pos.Y;
                        break;
                    case 3: 
                        pos.Z = -pos.Z;
                        break;
                }
            }
            else
            {
                pos = Entity.Position;
            }

            Entity entity;
            if (Type == EntityType.Ped)
            {
                entity = await PedSpawner.CreatePed(Entity.Model, pos, Rotation);
            } else if (Type == EntityType.Vehicle)
            {
                entity = await VehicleSpawner.CreateVehicle(Entity.Model, pos, Rotation);
            } else if (Type == EntityType.Prop)
            {
                entity = await PropSpawner.CreateProp(Entity.Model, pos, Rotation);
            }
            else
            {
                throw new Exception($"Unknown entity type: {Type.ToString()}");
            }

            var attach = new Attachment(CurrentBuildMenu.Build.NextId, ParentEntity, entity, $"{Name} (Clone)");
            BuilderMain.CurrentBuildMenu.AddAttachment(attach);
            return attach;
        }


        private void OnMenuClose(Menu menu)
        {
            // TODO: Possibly find a solution to not nuke the items when a submenu is being opened.
            Root.ClearMenuItems();
            // Don't clear highlighted handle: AttachmentsMenu will switch it when it's returned to
        }
    }
}