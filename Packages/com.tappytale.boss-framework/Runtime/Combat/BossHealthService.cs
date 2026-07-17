using System;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Events;
using UnityEngine;

namespace TappyTale.BossFight.Combat
{
    public sealed class BossHealthService : IBossService
    {
        private BossContext _context;
        private BossEventHub _events;

        public bool IsInvulnerable { get; set; }
        public bool IsDead => _context != null && _context.Blackboard.CurrentHealth <= 0f;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _events);
        }

        public bool ApplyDamage(BossDamageRequest request)
        {
            if (_context == null || IsDead || request.Amount <= 0f)
            {
                return false;
            }

            if (IsInvulnerable && !request.IgnoreInvulnerability)
            {
                return false;
            }

            BossBlackboard blackboard = _context.Blackboard;
            float previous = blackboard.CurrentHealth;
            blackboard.CurrentHealth = Mathf.Max(0f, previous - request.Amount);
            float applied = previous - blackboard.CurrentHealth;

            HealthChanged?.Invoke(blackboard.CurrentHealth, blackboard.MaxHealth);
            _events?.Raise(new BossEvent(BossEventType.Damaged, request.SourceId, applied));

            if (blackboard.CurrentHealth <= 0f)
            {
                Died?.Invoke();
                _events?.Raise(new BossEvent(BossEventType.Died));
            }

            return applied > 0f;
        }

        public void RestoreFullHealth()
        {
            if (_context == null)
            {
                return;
            }

            _context.Blackboard.CurrentHealth = _context.Blackboard.MaxHealth;
            HealthChanged?.Invoke(_context.Blackboard.CurrentHealth, _context.Blackboard.MaxHealth);
        }

        public void Shutdown()
        {
            HealthChanged = null;
            Died = null;
            _events = null;
            _context = null;
        }
    }
}
