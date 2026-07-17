using System;
using System.Collections.Generic;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Events;
using UnityEngine;

namespace TappyTale.BossFight.Projectiles
{
    public sealed class BossProjectilePool : IBossService
    {
        private readonly Dictionary<ProjectileDefinition, Queue<BossProjectile>> _available = new();
        private readonly HashSet<BossProjectile> _active = new();
        private BossContext _context;
        private BossEventHub _events;
        private Transform _root;

        public int ActiveCount => _active.Count;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _events);
            GameObject rootObject = new("Boss Projectile Pool");
            _root = rootObject.transform;
            _root.SetParent(context.BossTransform, false);
        }

        public BossProjectile Spawn(ProjectileDefinition definition, Vector3 position, Vector3 direction)
        {
            if (definition == null || definition.Prefab == null)
            {
                return null;
            }

            if (!_available.TryGetValue(definition, out Queue<BossProjectile> queue))
            {
                queue = new Queue<BossProjectile>();
                _available.Add(definition, queue);
            }

            BossProjectile projectile = queue.Count > 0 ? queue.Dequeue() : CreateProjectile(definition);
            _active.Add(projectile);
            projectile.Launch(this, definition, position, direction);
            _events?.Raise(new BossEvent(BossEventType.ProjectileSpawned, definition.Id));
            return projectile;
        }

        public void Despawn(BossProjectile projectile)
        {
            if (projectile == null || !_active.Remove(projectile))
            {
                return;
            }

            ProjectileDefinition definition = projectile.Definition;
            projectile.ResetForPool();
            if (definition != null)
            {
                if (!_available.TryGetValue(definition, out Queue<BossProjectile> queue))
                {
                    queue = new Queue<BossProjectile>();
                    _available.Add(definition, queue);
                }

                queue.Enqueue(projectile);
                _events?.Raise(new BossEvent(BossEventType.ProjectileDespawned, definition.Id));
            }
        }

        public void Shutdown()
        {
            foreach (BossProjectile projectile in _active)
            {
                if (projectile != null)
                {
                    UnityEngine.Object.Destroy(projectile.gameObject);
                }
            }

            foreach (Queue<BossProjectile> queue in _available.Values)
            {
                while (queue.Count > 0)
                {
                    BossProjectile projectile = queue.Dequeue();
                    if (projectile != null)
                    {
                        UnityEngine.Object.Destroy(projectile.gameObject);
                    }
                }
            }

            _active.Clear();
            _available.Clear();
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root.gameObject);
            }

            _events = null;
            _context = null;
            _root = null;
        }

        private BossProjectile CreateProjectile(ProjectileDefinition definition)
        {
            GameObject instance = UnityEngine.Object.Instantiate(definition.Prefab, _root);
            BossProjectile projectile = instance.GetComponent<BossProjectile>();
            if (projectile == null)
            {
                projectile = instance.AddComponent<BossProjectile>();
            }

            instance.SetActive(false);
            return projectile;
        }
    }
}
