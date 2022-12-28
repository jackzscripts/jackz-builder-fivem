namespace jackz_builder.Client.lib
{
    public enum SemverResult
    {
        SmallerThan = -1,
        Equal,
        GreaterThan = 1
    }
    public class Semver
    {
        public int Major { get; } = 0;
        public int Minor { get; } = 0;
        public int Patch { get; } = 0;

        public Semver(string versionText)
        {
            var bits = versionText.Split('.');
            if (bits.Length > 0) Major = int.Parse(bits[0]);
            if (bits.Length > 1) Minor = int.Parse(bits[1]);
            if (bits.Length > 2) Patch = int.Parse(bits[2]);
        }

        /// <summary>
        /// Compares two semver versions, showing which one is greater.
        /// </summary>
        /// <param name="b">Other semver version</param>
        /// <returns></returns>
        public SemverResult Compare(Semver b)
        {
            if (Major > b.Major) return SemverResult.GreaterThan;
            else if (Major < b.Major) return SemverResult.SmallerThan;
            else if (Minor > b.Minor) return SemverResult.GreaterThan;
            else if (Minor < b.Minor) return SemverResult.SmallerThan;
            else if (Patch > b.Patch) return SemverResult.GreaterThan;
            else if (Patch < b.Patch) return SemverResult.SmallerThan;
            return SemverResult.Equal;
        }

        public SemverResult Compare(string versionText)
        {
            return Compare(new Semver(versionText));
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}