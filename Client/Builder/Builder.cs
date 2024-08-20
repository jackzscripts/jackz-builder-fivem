using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace jackz_builder.Client.Builder
{
    public class Builder
    {
        private Vector3 Position;
        public float StepSize = 0.1f;
        private BuilderEntity ParentEntity = null;

        private readonly List<BuilderEntity> _entities = new List<BuilderEntity>();
        public List<BuilderEntity> Entities => _entities;
        public List<BuilderEntity> Props
        {
            get {
                return _entities.FindAll(entity => entity.Type == BuilderEntity.EntityType.Prop);
            }
        }
        public List<BuilderEntity> Vehicles
        {
            get {
                return _entities.FindAll(entity => entity.Type == BuilderEntity.EntityType.Vehicle);
            }
        }
        public List<BuilderEntity> Peds
        {
            get {
                return _entities.FindAll(entity => entity.Type == BuilderEntity.EntityType.Ped);
            }
        }

        private int _nextId = 0;
        
        [JsonProperty("name")]
        private string Name = "Untitled Build";
        [JsonProperty("author")]
        private string Author;

        private BuilderEntity EditedEntity;
        private Entity _previewEntity;
        private string _previewName;

        public Entity PreviewEntityRef => _previewEntity;

        public int GetNextId()
        {
            _nextId++;
            return _nextId;
        }
        
        public async Task<uint?> RequestModel(string modelId, CancellationToken token)
        {
            int hash = CitizenFX.Core.Native.API.GetHashKey(modelId);
            if (!CitizenFX.Core.Native.API.IsModelValid((uint) hash))
            {
                return null;
            }

            CitizenFX.Core.Native.API.RequestModel((uint)hash);
            while (!CitizenFX.Core.Native.API.HasModelLoaded((uint) hash) && !token.IsCancellationRequested)
            {
                await BaseScript.Delay(40);
            }

            if (token.IsCancellationRequested)
            {
                CitizenFX.Core.Native.API.SetModelAsNoLongerNeeded((uint)hash);
                return null;
            }

            return (uint)hash;
        }

        private CancellationTokenSource _previewCancelToken;
        
        /// <summary>
        /// Start previewing a prop. Will load the model and start previewing
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns>Entity, or if cancelled (by another Preview call), null</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Entity> PreviewProp(string modelId)
        {
            _previewCancelToken?.Cancel();
            _previewCancelToken = new CancellationTokenSource();
            uint? hash = await RequestModel(modelId, _previewCancelToken.Token);
            if (hash == null)
            {
                return null;
            }
            Vector3 pos = Game.PlayerPed.GetOffsetPosition(new Vector3(0, 5, 0));
            Entity entity = await World.CreateProp(modelId, pos, false, true);
            PreviewEntity(entity, modelId);
            CitizenFX.Core.Native.API.SetModelAsNoLongerNeeded((uint)hash);
            _previewCancelToken = null;
            return entity;
        }

        public void PreviewEntity(Entity entity, string name = "")
        {
            ClearPreview();
            _previewEntity = entity;
            _previewEntity.Opacity = 180;
            _previewEntity.HasGravity = false;
            _previewEntity.IsCollisionEnabled = false;
            _previewEntity.IsPositionFrozen = true;
            _previewEntity.IsCollisionProof = true;
            _previewEntity.IsInvincible = true;
            _previewName = string.IsNullOrEmpty(name) ? entity.Handle.ToString() : name;
        }

        public void ClearPreview()
        {
            if (_previewEntity != null)
            {
                BuilderUtil.RemoveAllAttachments(_previewEntity);
                _previewEntity.Delete();
            }

            _previewName = null;
        }

        public void AddEntity(Entity entity, string name = "")
        {
            var entry = new BuilderEntity(entity, name);
            _entities.Add(entry);
        }

        public void Reset()
        {
            ClearPreview();
            EditedEntity = null;
            foreach (var entry in _entities)
            {
                entry.Entity.Delete();
            }
            _entities.Clear();
        }

        public void Save()
        {
            string content = JsonConvert.SerializeObject(this);
        }

        private int _tick;
        public void OnTick()
        {
            if (_previewEntity != null)
            {
                _previewEntity.Velocity = Vector3.Zero;
                _previewEntity.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0, 5, 0));
                Util.DrawText3D(_previewEntity, _previewName, 0.2f, Color.White);
                Util.DrawEntityBox(_previewEntity, Color.Red);
                if (_tick > 2)
                {
                    _tick = 0;
                    _previewEntity.Rotation = new Vector3(_previewEntity.Rotation.X, _previewEntity.Rotation.Y,
                        _previewEntity.Rotation.Z + 1);
                }

                _tick++;
            }
        }
    }
}