using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace jackz_builder.Client.lib
{
    [Serializable]
    public class VehicleInfo
    {
        public int livery;
        public uint model;
        public string name;
        public bool neonBack;
        public bool neonFront;
        public bool neonLeft;
        public bool neonRight;
        public string plateText;
        public int plateStyle;
        public bool turbo;
        public bool tyreSmoke;
        public int version;
        public int wheelType;
        public int windowTint;
        public bool xenonHeadlights;
        public bool bulletProofTires;
        public int headlightColor;
        public float enveffScale;
        public Dictionary<int, int> mods;
        public Dictionary<int, bool> extras;
        public bool customWheels;
        public Dictionary<string, int> colors;
        public float? fuelLevel;
    }
    
    public class VehicleManager
    {
        public static string SerializeVehicleToJson(Vehicle vehicle)
        {
            var info = SerializeVehicle(vehicle);
            return JsonConvert.SerializeObject(info);
        }
        public static VehicleInfo SerializeVehicle(Vehicle vehicle)
        {
            var mods = vehicle.Mods.GetAllMods()
                .ToDictionary(mod => (int)mod.ModType, mod => mod.Index);

            #region colors
            var colors = new Dictionary<string, int>();
            int primaryColor = 0;
            int secondaryColor = 0;
            int pearlescentColor = 0;
            int wheelColor = 0;
            int dashColor = 0;
            int trimColor = 0;
            GetVehicleExtraColours(vehicle.Handle, ref pearlescentColor, ref wheelColor);
            GetVehicleColours(vehicle.Handle, ref primaryColor, ref secondaryColor);
            GetVehicleDashboardColour(vehicle.Handle, ref dashColor);
            GetVehicleInteriorColour(vehicle.Handle, ref trimColor);
            colors.Add("primary", primaryColor);
            colors.Add("secondary", secondaryColor);
            colors.Add("pearlescent", pearlescentColor);
            colors.Add("wheels", wheelColor);
            colors.Add("dash", dashColor);
            colors.Add("trim", trimColor);
            int neonR = 255;
            int neonG = 255;
            int neonB = 255;
            if (vehicle.Mods.HasNeonLights)
            {
                GetVehicleNeonLightsColour(vehicle.Handle, ref neonR, ref neonG, ref neonB);
            }
            colors.Add("neonR", neonR);
            colors.Add("neonG", neonG);
            colors.Add("neonB", neonB);
            int tyresmokeR = 0;
            int tyresmokeG = 0;
            int tyresmokeB = 0;
            GetVehicleTyreSmokeColor(vehicle.Handle, ref tyresmokeR, ref tyresmokeG, ref tyresmokeB);
            colors.Add("tyresmokeR", tyresmokeR);
            colors.Add("tyresmokeG", tyresmokeG);
            colors.Add("tyresmokeB", tyresmokeB);
            #endregion

            var extras = new Dictionary<int, bool>();
            for (int i = 0; i < 20; i++)
            {
                if (vehicle.ExtraExists(i))
                {
                    extras.Add(i, vehicle.IsExtraOn(i));
                }
            }

            return new VehicleInfo()
            {
                colors = colors,
                customWheels = GetVehicleModVariation(vehicle.Handle, 23),
                extras = extras,
                livery = GetVehicleLivery(vehicle.Handle),
                model = (uint) GetEntityModel(vehicle.Handle),
                mods = mods,
                name = GetLabelText(GetDisplayNameFromVehicleModel((uint)GetEntityModel(vehicle.Handle))),
                neonBack = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Back),
                neonFront = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Front),
                neonLeft = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Left),
                neonRight = vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Right),
                plateText = vehicle.Mods.LicensePlate,
                plateStyle = (int)vehicle.Mods.LicensePlateStyle,
                turbo = IsToggleModOn(vehicle.Handle, 18),
                tyreSmoke = IsToggleModOn(vehicle.Handle, 20),
                version = 1,
                wheelType = GetVehicleWheelType(vehicle.Handle),
                windowTint = (int)vehicle.Mods.WindowTint,
                xenonHeadlights = IsToggleModOn(vehicle.Handle, 22),
                bulletProofTires = !vehicle.CanTiresBurst,
                headlightColor =  GetVehicleHeadlightsColour(vehicle.Handle),
                enveffScale = GetVehicleEnveffScale(vehicle.Handle),
                fuelLevel = GetVehicleFuelLevel(vehicle.Handle)
            };
        }

        public static void ApplyToVehicle(Vehicle vehicle, VehicleInfo info)
        {
            Debug.WriteLine("Applying vehicle data");
            SetModelAsNoLongerNeeded(info.model);
            SetVehicleLivery(vehicle.Handle, info.livery);
            SetVehicleNeonLightEnabled(vehicle.Handle, (int)VehicleNeonLight.Left, info.neonLeft);
            SetVehicleNeonLightEnabled(vehicle.Handle, (int)VehicleNeonLight.Right, info.neonRight);
            SetVehicleNeonLightEnabled(vehicle.Handle, (int)VehicleNeonLight.Front, info.neonFront);
            SetVehicleNeonLightEnabled(vehicle.Handle, (int)VehicleNeonLight.Back, info.neonBack);
            SetVehicleNumberPlateText(vehicle.Handle, info.plateText);
            SetVehicleNumberPlateTextIndex(vehicle.Handle, info.plateStyle);
            SetVehicleXenonLightsColor(vehicle.Handle, info.headlightColor);
            SetVehicleWindowTint(vehicle.Handle, info.windowTint);
            SetVehicleEnveffScale(vehicle.Handle, info.enveffScale);
            vehicle.CanTiresBurst = !info.bulletProofTires;
            foreach (var keyValuePair in info.extras)
            {
                SetVehicleExtra(vehicle.Handle, keyValuePair.Key, keyValuePair.Value);
            }
            foreach (var keyValuePair in info.mods)
            {
                SetVehicleMod(vehicle.Handle, keyValuePair.Key, keyValuePair.Value, false);
            }

            SetVehicleExtraColours(vehicle.Handle, info.colors["pearlescent"], info.colors["wheels"]);
            SetVehicleColours(vehicle.Handle, info.colors["primary"], info.colors["secondary"]);
            SetVehicleDashboardColour(vehicle.Handle, info.colors["dash"]);
            SetVehicleInteriorColour(vehicle.Handle, info.colors["trim"]);
            SetVehicleTyreSmokeColor(vehicle.Handle, info.colors["tyresmokeR"], info.colors["tyresmokeG"], info.colors["tyresmokeB"]);
            if (info.fuelLevel != null)
            {
                SetVehicleFuelLevel(vehicle.Handle, info.fuelLevel.Value);
            }
        }
        public static async Task<Vehicle> SpawnVehicle(VehicleInfo info, Vector3 position, float heading)
        {
            Debug.WriteLine($"Requesting model {info.model}");
            await Util.RequestModel(info.model);
            Debug.WriteLine("Spawning vehicle");
            var vehicle = await World.CreateVehicle((int)info.model, position, heading);
            ApplyToVehicle(vehicle, info);
            return vehicle;
        }
    }
}