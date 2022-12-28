using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using jackz_builder.Client.JackzBuilder.Spawners;
using jackz_builder.Client.lib;
using Newtonsoft.Json;

namespace jackz_builder.Client.JackzBuilder
{
    public class VehicleAttachment : Attachment
    {
        [JsonProperty("type")] public new EntityType Type = EntityType.Vehicle;

        [JsonProperty("savedata")]
        public VehicleInfo SaveData
        {
            get => VehicleManager.SerializeVehicle((Vehicle)Entity);
        }

        
        public VehicleAttachment(int id, Entity parentEntity, Entity entity, string name) : base(id, parentEntity, entity, name)
        {
            
        }

        public VehicleAttachment() : base()
        {
        }

        public override async Task<Entity> Spawn(bool isPreview)
        {
            if (!API.IsModelValid((uint)Model))
            {
                Debug.WriteLine($"{GetType()}.Spawn: Attachment(#{Id}, Name={Name}) model \"{Model}\" is invalid, skipping");
                return null;
            }
            await Util.RequestModel((uint)Model);
            if (isPreview)
            {
                var vehicle = (Vehicle) await VehicleSpawner.CreateVehicle((uint)Model, Vector3.Zero, Rotation, true);
                VehicleManager.ApplyToVehicle(vehicle, SaveData);
                Entity = vehicle;
            }
            else
            {
                Entity = await VehicleManager.SpawnVehicle(SaveData, PositionOffset, Rotation.Z);
            }
            Attach();
            SetupEntity();
            return Entity;
        }
    }
}