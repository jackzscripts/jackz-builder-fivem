using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using jackz_builder.Client.JackzBuilder;
using Mono.CSharp;
using Newtonsoft.Json;

namespace jackz_builder.Client
{
    public class BuildManager
    {
        private const string KvpSaveName = "jackzbuilder:builds";
        
        private static Dictionary<string, string> builds;
        
        
        static BuildManager()
        {
            // API.DeleteResourceKvp(KvpSaveName);
            LoadBuilds();
        }

        public static void LoadBuilds()
        {
            var data = API.GetResourceKvpString(KvpSaveName);
            if (data == null) builds = new Dictionary<string, string>();
            else builds = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            
            foreach(var builddata in builds)
            {
                CitizenFX.Core.Debug.WriteLine(builddata.ToString());
            }
        }

        public static void SaveBuilds()
        {
            API.SetResourceKvp(KvpSaveName, JsonConvert.SerializeObject(builds));
        }

        public static void SaveBuild(Build build)
        {
            if (!build.HasName) throw new Exception("Build is untitled");
            // TODO: Check for duplicate and confirm user wants to replace the already saved build
            // Remove any existing build with same id:
            Debug.WriteLine("Removing duplicate");
            builds.Remove(build.Id);
            Debug.WriteLine("Addiong build");
            builds.Add(build.Id, build.Export());
            Debug.WriteLine("Saving all builds");
            SaveBuilds();
        }

        public static bool DeleteBuild(string id)
        {
            var res = builds.Remove(id);
            SaveBuilds();
            return res;
        }

        public static async Task<Build> GetBuild(string id, bool isPreview)
        {
            if (!builds.ContainsKey(id)) return null;
            return await Build.Import(builds[id], isPreview);
        }

        public static bool HasBuild(string id)
        {
            return builds.ContainsKey(id);
        }
        
        public static IEnumerable<BuildMetaData> Builds =>
            builds
                .Select(kv => Build.ImportMeta(kv.Value));
        
        public static IEnumerable<string> BuildIds =>
            builds
                .Select(kv => kv.Key);
    }
}