using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using jackz_builder.Client.lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jackz_builder.Client.JackzBuilder
{
    public class SerializedVector3
    {
        [JsonProperty("x")] public float X;
        [JsonProperty("y")] public float Y;
        [JsonProperty("z")] public float Z;
        
        public SerializedVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public static implicit operator SerializedVector3(Vector3 vec) => new SerializedVector3(vec.X, vec.Y, vec.Z);
        public static implicit operator Vector3(SerializedVector3 vec) => new Vector3(vec.X, vec.Y, vec.Z);

    }
    public class InvalidBuildData : Exception
    {
        public InvalidBuildData(string message) : base($"Build data is invalid:\n{message}")
        {
            
        }
    }
    public enum EntityType
    {
        Vehicle,
        Prop,
        Ped
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BuildMetaData
    {
        public string Id => $"{Author}/{Name}";

        [JsonProperty("name")] [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
        private string? _name;
        public string Name => _name ?? "Unnamed Build";
        [JsonProperty("created")] public int? Created;
        [JsonProperty("version")] [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")] 
        public string? Version;
        [JsonProperty("author")] [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")] 
        private string? _author;
        public string Author => _author ?? "Anonymous";
        public float? Rating;

        public string GetDescriptionText(bool showRatings)
        {
            if (Version == null)
            {
                return "-Invalid Version-";
            }

            var lines = new List<string>();

            var versionText = $"Format Version: {Version} ";
            var result = new Semver(Version).Compare(BuilderMain.BuilderVersion);
            if (result == SemverResult.SmallerThan)
                versionText += $"(Older version, latest {BuilderMain.BuilderVersion}";
            else if (result == SemverResult.GreaterThan)
                versionText += $"(Unsupported version, latest {BuilderMain.BuilderVersion}";
            else
                versionText += "(Latest)";
            lines.Add(versionText);

            if (Created != null)
            {
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(Created.Value).ToLocalTime();
                lines.Add("Created: " + dtDateTime.ToString("yyyy/M/d h:mm:ss tt UTC"));
            }

            if (Author != null)
            {
                lines.Add($"Build Author: {Author}");
            }

            if (showRatings)
            {
                if (Rating > 0.0)
                {
                    lines.Add($"Rating: {Rating} / 5 stars");
                }
                else
                {
                    lines.Add("No user ratings");
                }     
            }
            return string.Join("\n", lines);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class Build
    {
        [JsonProperty("base")]
        public Attachment Base;

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

        [JsonProperty("blipIcon")] public int BlipSprite { get; set; }
        [JsonProperty("spawnLocation")] public SerializedVector3? SpawnLocation { get; set; } = null;
        [JsonProperty("spawnInBuild")] public bool SpawnInBuild { get; set; }
        [JsonProperty("created")] public long _created;
        [JsonProperty("version")] public string Version { get; private set; }

        public DateTimeOffset Created
        {
            get
            {
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(_created).ToLocalTime();
                return dtDateTime;
            }
            set
            {
                TimeSpan timeSpan = value - new DateTime(1970, 1, 1, 0, 0, 0);
                _created = (long)timeSpan.TotalSeconds;
            }
        }

        public Dictionary<Entity, Attachment> Attachments = new Dictionary<Entity, Attachment>();

        private Blip blip;
        private int _nextId;

        /// <summary>
        /// The next ID that should given to a new attachment
        /// </summary>
        // Increment id AFTER return
        public int NextId => _nextId++;

        private bool _showBlip;

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

        /// <summary>
        /// Create a new build
        /// </summary>
        /// <param name="baseEntity"></param>
        public Build(Entity baseEntity)
        {
            Version = BuilderMain.BuilderVersion;
            Created = DateTimeOffset.Now;
            Base = new Attachment(-1, null, baseEntity, "_base");
            ChangeBase(baseEntity);
        }

        [JsonConstructor]
        public Build()
        {
        }

        // Should only import metadata
        public static BuildMetaData ImportMeta(string saveData)
        {
            return JsonConvert.DeserializeObject<BuildMetaData>(saveData);
        }
        
        /// <summary>
        /// Will import a build and spawn its attachments
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="isPreview"></param>
        /// <returns></returns>
        public static async Task<Build> Import(string saveData, bool isPreview = false)
        {
            var obj = JsonConvert.DeserializeObject<JObject>(saveData);
            var attachments = new List<Attachment>();
            
            var build = obj.ToObject<Build>();

            await build.Spawn(isPreview, obj);

            return build;
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
            Base.ComputeSize(ref rSize, ref hSize);
            foreach (var attachment in Attachments.Values)
            {
                attachment.ComputeSize(ref rSize, ref hSize);
            }

            return new Vector2(rSize + 10f, hSize);
        }

        public string Export()
        {
            JObject jo = JObject.FromObject(this);
            Debug.WriteLine("Exporting vehicles");
            var vehicleAttachments = GetAttachments(EntityType.Vehicle);
            jo.Add("vehicles", JToken.FromObject(vehicleAttachments));
            Debug.WriteLine("Exporting peds");
            var pedAttachments = GetAttachments(EntityType.Ped);
            jo.Add("peds", JToken.FromObject(pedAttachments));
            Debug.WriteLine("Exporting props");
            var propAttachments = GetAttachments(EntityType.Prop);
            jo.Add("objects", JToken.FromObject(propAttachments));
            var str = JsonConvert.SerializeObject(jo);
            Debug.WriteLine($"Build Export: \"{str}\"");
            return str;
        }
        
        private List<Attachment> GetAttachments(EntityType type)
        {
            return Attachments.Values.Where(attach => attach.Type == type).ToList();
        }

        /// <summary>
        /// Spawns a build with a blip. Throws errors if build data is invalid
        /// </summary>
        /// <returns></returns>
        public async Task Spawn(bool isPreview, JObject obj)
        {
            if (Base == null) throw new InvalidBuildData("Missing 'base' entry");
            BuilderMain.ClearPreview();
            var size = ComputeSize();
            var pos = SpawnLocation ?? Game.PlayerPed.GetOffsetPosition(new Vector3(0f, size.X, 0f));
            var baseHandle = await Base.Spawn(isPreview);
            baseHandle.Position = pos;
            if (baseHandle == null)
            {
                throw new InvalidBuildData(
                    "Failed to spawn base entity for build, possibly due to invalid model. Check logs for details");
            }
            ChangeBase(baseHandle);
            Debug.WriteLine($"Spawned base handle {baseHandle.Handle} [Preview:{isPreview}]");
            
            // Spawn the attachments
            await SpawnAttachments(isPreview, obj);
            
            // Call this after we spawn all the attachments: The attachments can call SpawnPreview overwriting this value
            if (isPreview)
            {
                BuilderMain.SetPreview(baseHandle, "_base", size.X, null, null, size.Y);
            }
            else
            {
                ShowBlip = true;
            }
        }

        private async Task SpawnAttachments(bool isPreview, JObject obj)
        {
            Debug.WriteLine("Spawning attachments for build");
            var handles = new List<int>();
            var idMap = new Dictionary<int, int>(); // <Id, Handle>
            var parentQueue = new Queue<dynamic>();
            
            await LoadAttachment<VehicleAttachment>(obj["vehicles"], isPreview);

            await LoadAttachment<Attachment>(obj["peds"], isPreview);
            
            await LoadAttachment<Attachment>(obj["objects"], isPreview);
        }
        
        private async Task LoadAttachment<T>(JToken property, bool isPreview) where T : Attachment
        {
            if (property != null)
            {
                foreach (var jToken in property.Children())
                {
                    var attach = jToken.ToObject<T>();
                    Debug.WriteLine($"Spawning attachment {attach.Id} of {typeof(T)}");
                    attach.SetParent(Base);
                    var entity = await attach.Spawn(isPreview);
                    Attachments.Add(entity, attach);
                }
            }
            else
            {
                Debug.WriteLine("[Debug] Skipping null property");
            }
        }

        public void ChangeBase(Entity newBase)
        {
            var oldBase = Base.Entity;

            Base.Entity = newBase;
            
            CreateBlip();

            BuilderMain.HighlightedEntity = newBase;
                
            Debug.WriteLine($"Reassigned base {oldBase?.Handle.ToString() ?? "-none"} --> {newBase.Handle}");
        }

        private void CreateBlip()
        {
            if(blip != null)
            {
                blip.Delete();
            }

            if (ShowBlip && Base.Entity != null)
            {
                blip = Util.CreateBlipForEntity(Base.Entity, (BlipSprite)BlipSprite, Name);
            }
        }

        /// <summary>
        /// Finds an attachment with the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The attachment or null if none found</returns>
        public Attachment GetAttachment(int id)
        {
            if (id == -1) return Base;
            return (from kv in Attachments where kv.Value.Id == id select kv.Value)
                .FirstOrDefault();
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                var _build = (Build)obj;
                return _build.Name == Name && _build.Author == Author && Attachments.Count == _build.Attachments.Count;
            }
        }
        
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Author.GetHashCode() ^ Created.GetHashCode();
        }
    }
}