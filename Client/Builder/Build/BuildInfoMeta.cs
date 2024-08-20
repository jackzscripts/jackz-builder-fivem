using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace jackz_builder.Client.Builder.Build
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BuildMetaData
    {
        public string Id => $"{Author}/{Name}";

        [JsonProperty("name")] [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
        private string? _name;
        public string Name => _name ?? "Unnamed Build";
        [JsonProperty("created")] public int? Created;
        [JsonProperty("version")] [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")] 
        public string Version;
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
            // if (Version != null)
            // {
            //     var version = new Semver(Version.Split(' ').Last());
            //     var result = version.Compare(BuilderMain.BuilderVersion);
            //     if (result == SemverResult.SmallerThan)
            //         versionText += $"(Older version, latest {BuilderMain.BuilderVersion}";
            //     else if (result == SemverResult.GreaterThan)
            //         versionText += $"(Unsupported version, latest {BuilderMain.BuilderVersion}";
            //     else
            //         versionText += "(Latest)";
            //     lines.Add(versionText);
            // }
            

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
}