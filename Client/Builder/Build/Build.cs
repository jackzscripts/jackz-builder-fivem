using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jackz_builder.Client.Builder.Build
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Build
    {
        // [JsonProperty("base")]
        // public Attachment Base;

        public string Id => $"{Author}/{Name}";

        public bool HasName => _name != null;
        public bool HasAuthor => _author != null;

        [JsonProperty("name")] private string _name;

        public string Name
        {
            get => _name ?? "Unnamed Build";
            set => _name = value;
        }

        [JsonProperty("author")] private string _author;

        public string Author
        {
            get => _author ?? "Anonymous";
            set => _author = value;
        }

        [JsonProperty("blipIcon")] public int _blipSprite;
        public int BlipSprite
        {
            get => _blipSprite;
            set
            {
                _blipSprite = value;
                CreateBlip();
            }
        }

        // [JsonProperty("spawnLocation")] private SerializedVector3 _spawnLocation;

        // public Vector3? SpawnLocation
        // {
        //     get => _spawnLocation != null ? new Vector3(_spawnLocation.X, _spawnLocation.Y, _spawnLocation.Z) : null;
        //     set => _spawnLocation = value.HasValue ? new SerializedVector3(value.Value.X, value.Value.Y, value.Value.Z) : null;
        // }
        // [JsonProperty("spawnInBuild")] public bool SpawnInBuild { get; set; }
        // [JsonProperty("created")] public long _created;
        // public Semver Version;

        // public DateTimeOffset Created
        // {
        //     get
        //     {
        //         DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //         dtDateTime = dtDateTime.AddSeconds(_created).ToLocalTime();
        //         return dtDateTime;
        //     }
        //     set
        //     {
        //         TimeSpan timeSpan = value - new DateTime(1970, 1, 1, 0, 0, 0);
        //         _created = (long)timeSpan.TotalSeconds;
        //     }
        // }

        // public Dictionary<Entity, Attachment> Attachments = new Dictionary<Entity, Attachment>();

        private Blip blip;
        private int _nextId;

        /// <summary>
        /// The next ID that should given to a new attachment
        /// </summary>
        // Increment id AFTER return
        public int NextId => _nextId++;

        private bool _showBlip = true;

        /// <summary>
        /// Should a blip be shown for the base entity of this build?
        /// Setting this to false will clear any existing blip
        /// Setting this to true will create a new blip
        /// </summary>
        public bool ShowBlip
        {
            get => _showBlip;
            set
            {
                _showBlip = value;
                CreateBlip();
            }
        }

        // Should only import metadata
        public static BuildMetaData ImportMeta(string saveData)
        {
            return JsonConvert.DeserializeObject<BuildMetaData>(saveData);
        }
        
        /// <summary>
        /// Computes the size of the build, taking into account the dimensions of all its attachments.
        /// This helps prevent the previews from being close for large builds and too far for small builds
        /// </summary>
        /// <returns>The radius size (l,w) as X and the height as Y</returns>
        public Vector2 ComputeSize()
        {
            float rSize = 0f;
            float hSize = 0f;
            // Base.ComputeSize(ref rSize, ref hSize);
            // foreach (var attachment in Attachments.Values)
            // {
            //     attachment.ComputeSize(ref rSize, ref hSize);
            // }

            return new Vector2(rSize + 10f, hSize);
        }
        
        private void CreateBlip()
        {
            if(blip != null)
            {
                blip.Delete();
            }

            // if (ShowBlip && Base.Entity != null)
            // {
            //     blip = Util.CreateBlipForEntity(Base.Entity, (BlipSprite)BlipSprite, Name);
            // }
        }
    }
}