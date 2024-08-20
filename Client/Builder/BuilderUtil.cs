using CitizenFX.Core;

namespace jackz_builder.Client.Builder
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
        
        public static void RemoveAllAttachments(Entity entity)
        {
            recurseRemoveAttachments(entity, World.GetAllProps());
            recurseRemoveAttachments(entity, World.GetAllVehicles());
            recurseRemoveAttachments(entity, World.GetAllPeds());
        }
        
        private static void recurseRemoveAttachments(Entity parent, Entity[] entities)
        {
            foreach (var entity in entities)
            {
                if (entity == parent) continue;
                foreach (var subEntity in entities)
                {
                    if (subEntity != entity && subEntity != parent && subEntity.IsAttachedTo(entity))
                    {
                        subEntity.Delete();
                    }
                }

                if (entity.IsAttachedTo(parent))
                {
                    entity.Delete();
                }
            }
        }
    }
}