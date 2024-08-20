using System.Linq;

namespace test_project.Client
{
    public static class BuilderUtil
    {
        public static string GetBreadcrumbsList(string[] pieces)
        {
            return "~q~Builder~s~ > " + string.Join(" > ", pieces);
        }
        public static string GetBreadcrumbs(params string[] pieces)
        {
            return GetBreadcrumbsList(pieces);
        }
    }
}