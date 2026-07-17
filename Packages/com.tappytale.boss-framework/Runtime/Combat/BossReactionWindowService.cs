using System;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Events;

namespace TappyTale.BossFight.Combat
{
    public sealed class BossReactionWindowService : IBossService, IBossTickable
    {
        private BossContext _context;
        private BossEventHub _events;
        private BossTimelineRunner _runner;
        private float _parryRemaining;
        private float _dodgeRemaining;

        public int TickOrder => 50;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _events);
            context.Services.TryResolve(out _runner);
            if (_runner != null)
            {
                _runner.EntryTriggered += OnTimelineEntry;
                _runner.Completed += OnAttackCompleted;
            }
        }

        public void Tick(float deltaTime)
        {
            TickWindow(ref _parryRemaining, deltaTime, true);
            TickWindow(ref _dodgeRemaining, deltaTime, false);
        }

        public bool TryParry()
        {
            if (_context == null || !_context.Blackboard.IsParryWindowOpen)
            {
                return false;
            }

            CloseParryWindow();
            _events?.Raise(new BossEvent(BossEventType.PlayerParried));
            return true;
        }

        public bool TryPerfectDodge()
        {
            if (_context == null || !_context.Blackboard.IsPerfectDodgeWindowOpen)
            {
                return false;
            }

            ClosePerfectDodgeWindow();
            _events?.Raise(new BossEvent(BossEventType.PlayerPerfectDodged));
            return true;
        }

        public void Shutdown()
        {
            if (_runner != null)
            {
                _runner.EntryTriggered -= OnTimelineEntry;
                _runner.Completed -= OnAttackCompleted;
            }

            CloseAll();
            _runner = null;
            _events = null;
            _context = null;
        }

        private void OnTimelineEntry(AttackTimelineEntry entry)
        {
            switch (entry.Action)
            {
                case AttackTimelineAction.OpenParryWindow:
                    OpenParryWindow(entry.Value);
                    break;
                case AttackTimelineAction.OpenPerfectDodgeWindow:
                    OpenPerfectDodgeWindow(entry.Value);
                    break;
            }
        }

        private void OnAttackCompleted(BossAttackDefinition attack)
        {
            CloseAll();
        }

        private void OpenParryWindow(float duration)
        {
            _parryRemaining = Math.Max(0.01f, duration);
            _context.Blackboard.IsParryWindowOpen = true;
            _events?.Raise(new BossEvent(BossEventType.ParryWindowOpened, value: _parryRemaining));
        }

        private void OpenPerfectDodgeWindow(float duration)
        {
            _dodgeRemaining = Math.Max(0.01f, duration);
            _context.Blackboard.IsPerfectDodgeWindowOpen = true;
            _events?.Raise(new BossEvent(BossEventType.PerfectDodgeWindowOpened, value: _dodgeRemaining));
        }

        private void TickWindow(ref float remaining, float deltaTime, bool parry)
        {
            if (remaining <= 0f)
            {
                return;
            }

            remaining -= deltaTime;
            if (remaining <= 0f)
            {
                if (parry)
                {
                    CloseParryWindow();
                }
                else
                {
                    ClosePerfectDodgeWindow();
                }
            }
        }

        private void CloseParryWindow()
        {
            _parryRemaining = 0f;
            if (_context != null && _context.Blackboard.IsParryWindowOpen)
            {
                _context.Blackboard.IsParryWindowOpen = false;
                _events?.Raise(new BossEvent(BossEventType.ParryWindowClosed));
            }
        }

        private void ClosePerfectDodgeWindow()
        {
            _dodgeRemaining = 0f;
            if (_context != null && _context.Blackboard.IsPerfectDodgeWindowOpen)
            {
                _context.Blackboard.IsPerfectDodgeWindowOpen = false;
                _events?.Raise(new BossEvent(BossEventType.PerfectDodgeWindowClosed));
            }
        }

        private void CloseAll()
        {
            CloseParryWindow();
            ClosePerfectDodgeWindow();
        }
    }
}
