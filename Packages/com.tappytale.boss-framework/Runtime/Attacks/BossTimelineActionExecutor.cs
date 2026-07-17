using System;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Events;
using TappyTale.BossFight.Projectiles;
using UnityEngine;

namespace TappyTale.BossFight.Attacks
{
    public sealed class BossTimelineActionExecutor : IBossService
    {
        private BossContext _context;
        private BossTimelineRunner _runner;
        private BossPayloadResolver _resolver;
        private BossProjectilePool _projectiles;
        private BossEventHub _events;

        public event Action<AttackTimelineEntry> MeleeRequested;
        public event Action<AttackTimelineEntry> DashRequested;
        public event Action<AttackTimelineEntry> AudioRequested;
        public event Action<AttackTimelineEntry> VfxRequested;
        public event Action<AttackTimelineEntry> CustomRequested;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _runner);
            context.Services.TryResolve(out _resolver);
            context.Services.TryResolve(out _projectiles);
            context.Services.TryResolve(out _events);
            if (_runner != null) _runner.EntryTriggered += Execute;
        }

        public void Shutdown()
        {
            if (_runner != null) _runner.EntryTriggered -= Execute;
            MeleeRequested = null;
            DashRequested = null;
            AudioRequested = null;
            VfxRequested = null;
            CustomRequested = null;
            _context = null;
            _runner = null;
            _resolver = null;
            _projectiles = null;
            _events = null;
        }

        private void Execute(AttackTimelineEntry entry)
        {
            switch (entry.Action)
            {
                case AttackTimelineAction.SpawnProjectile:
                    SpawnProjectile(entry);
                    break;
                case AttackTimelineAction.MeleeHit:
                    MeleeRequested?.Invoke(entry);
                    break;
                case AttackTimelineAction.Dash:
                    DashRequested?.Invoke(entry);
                    break;
                case AttackTimelineAction.PlayAudio:
                    if (_resolver != null && _resolver.TryGetAudio(entry.PayloadId, out _)) AudioRequested?.Invoke(entry);
                    break;
                case AttackTimelineAction.PlayVfx:
                    if (_resolver != null && _resolver.TryGetVfx(entry.PayloadId, out _)) VfxRequested?.Invoke(entry);
                    break;
                case AttackTimelineAction.Custom:
                    CustomRequested?.Invoke(entry);
                    break;
            }
        }

        private void SpawnProjectile(AttackTimelineEntry entry)
        {
            if (_resolver == null || _projectiles == null || !_resolver.TryGetProjectile(entry.PayloadId, out ProjectileDefinition definition)) return;
            Vector3 direction = _context.Blackboard.DirectionToTarget;
            if (direction.sqrMagnitude < 0.0001f) direction = _context.BossTransform.forward;
            _projectiles.Spawn(definition, _context.BossTransform.position, direction);
            _events?.Raise(new BossEvent(BossEventType.TimelineAction, definition.Id, entry.Value, (int)entry.Action));
        }
    }
}