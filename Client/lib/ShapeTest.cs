using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace jackz_builder.Client.lib
{
    

    public enum RayResultType
    {
        InvalidHandle,
        Pending,
        Complete
    }
    
    public class RayResult
    {
        public bool DidHit { get; protected set; }
        public Vector3 Coords { get; }
        public Vector3 SurfaceNormal { get; }
        public Entity Entity { get; }

        public Vehicle Vehicle
        {
            get
            {
                if (Entity != null && Entity.Model.IsVehicle)
                {
                    return (Vehicle)Entity;
                }

                return null;
            }
        }
        
        public Ped Ped
        {
            get
            {
                if (Entity != null && Entity.Model.IsPed)
                {
                    return (Ped)Entity;
                }

                return null;
            }
        }
        
        public Prop Object
        {
            get
            {
                if (Entity != null && Entity.Model.IsProp)
                {
                    return (Prop)Entity;
                }

                return null;
            }
        }

        public RayResult(bool didHit, Vector3 coords, Vector3 surfaceNormal, int entity)
        {
            DidHit = didHit;
            Coords = coords;
            SurfaceNormal = surfaceNormal;
            Entity = Entity.FromHandle(entity);
        }
    }
    
    public class RayHandle
    {
        
        public int Handle { get; }

        public RayResult Result { get; private set; }
        public RayResultType ResultType { get; private set; } 

        /// <summary>
        /// Checks if the shape test is done. True if done, false if waiting (or failed, check ResultType!)
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            bool hit = false;
            Vector3 endCoords = Vector3.Zero;
            Vector3 surfaceNormal = Vector3.Zero;
            int entityHit = 0;
            var result = GetShapeTestResult(Handle, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);
            if (result == 0)
            {
                // Handle is invalid
                ResultType = RayResultType.InvalidHandle;
            } else if (result == 2)
            {
                ResultType = RayResultType.Complete;
                Result = new RayResult(hit, endCoords, surfaceNormal, entityHit);
                return true;
            }

            return false;
        }

        public RayResult GetResult()
        {
            return Check() ? Result : null;
        }

        public RayHandle(int handle)
        {
            ResultType = RayResultType.Pending;
            Handle = handle;
        }
        
    }

    public static class ShapeTest
    {

        public static RayHandle Capsule(Vector3 from, Vector3 to, float radius, TraceFlags flags, int ignoreEntity = 0)
        {
            var handle = StartShapeTestCapsule(from.X, from.Y, from.Z, to.X, to.Y, to.Z, radius, (int) flags,
                ignoreEntity, 4);
            return new RayHandle(handle);
        }
        
        public static RayHandle Capsule(Vector3 from, Vector3 to, float radius, TraceFlags flags, Entity ignoreEntity)
        {
            return Capsule(from, to, radius, flags, ignoreEntity.Handle);
        }
        
        public static RayHandle Capsule(Entity entity, float radius, TraceFlags flags, float distForward, float distSideways = 0f, float distVertical = 0f)
        {
            var endPos = GetOffsetFromEntityInWorldCoords(entity.Handle, distSideways, distForward, distVertical);
            return Capsule(entity.Position, endPos, radius, flags, entity.Handle);
        }

        public static RayHandle ExpensiveLosProbe(Vector3 from, Vector3 to, TraceFlags flags, int ignoreEntity)
        {
            var handle = StartExpensiveSynchronousShapeTestLosProbe(from.X, from.Y, from.Z, to.X, to.Y, to.Z, (int) flags,
                ignoreEntity, 4);
            return new RayHandle(handle);
        }
        
        public static RayHandle ExpensiveLosProbe(Vector3 from, Vector3 to, TraceFlags flags, Entity ignoreEntity)
        {
            return ExpensiveLosProbe(from, to, flags, ignoreEntity.Handle);
        }
        
        public static RayHandle ExpensiveLosProbe(Entity entity, TraceFlags flags, float distForward, float distSideways = 0f, float distVertical = 0f)
        {
            var endPos = GetOffsetFromEntityInWorldCoords(entity.Handle, distSideways, distForward, distVertical);
            return ExpensiveLosProbe(entity.Position, endPos, flags, entity.Handle);
        }
        
        public static RayHandle LosProbe(Vector3 from, Vector3 to, TraceFlags flags, int ignoreEntity)
        {
            var handle = StartShapeTestLosProbe(from.X, from.Y, from.Z, to.X, to.Y, to.Z, (int) flags,
                ignoreEntity, 4);
            return new RayHandle(handle);
        }
        
        public static RayHandle LosProbe(Vector3 from, Vector3 to, TraceFlags flags, Entity ignoreEntity)
        {
            return LosProbe(from, to, flags, ignoreEntity.Handle);
        }
        
        public static RayHandle LosProbe(Entity entity, TraceFlags flags, float distForward, float distSideways = 0f, float distVertical = 0f)
        {
            var endPos = GetOffsetFromEntityInWorldCoords(entity.Handle, distSideways, distForward, distVertical);
            return LosProbe(entity.Position, endPos, flags, entity.Handle);
        }
    }
    
    [Flags]
    public enum TraceFlags : uint
    {
        None = 0,
        IntersectWorld = 1,
        IntersectVehicles = 2,
        IntersectPedsSimpleCollision = 4,
        IntersectPeds = 8,
        IntersectObjects = 16,
        IntersectWater = 32,
        Unknown = 128,
        IntersectFoliage = 256,
        IntersectEverything = 4294967295
    }
}