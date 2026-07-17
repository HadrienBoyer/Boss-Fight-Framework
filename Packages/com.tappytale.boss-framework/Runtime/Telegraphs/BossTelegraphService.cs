using System;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Events;

namespace TappyTale.BossFight.Telegraphs
{
    public sealed class BossTelegraphService : IBossService, IBossTickable
    {
        private BossEventHub _events;
        private BossTimelineRunner _runner;
        private float _remaining;
        private string _activeId = string.Empty;

        public int TickOrder => 40;
        public bool IsActive => _remaining > 0f;
        public string ActiveId => _activeId;

        public event Action<string, float> Started;
        public event Action<string> Ended;

        public void Initialize(BossContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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
            if (!IsActive)
            {
                return;
            }

            _remaining -= deltaTime;
            if (_remaining <= 0f)
            {
                StopTelegraph();
            }
        }

        public void Shutdown()
        {
            if (_runner != null)
            {
                _runner.EntryTriggered -= OnTimelineEntry;
                _runner.Completed -= OnAttackCompleted;
            }

            StopTelegraph();
            Started = null;
            Ended = null;
            _runner = null;
            _events = null;
        }

        private void OnTimelineEntry(AttackTimelineEntry entry)
        {
            if (entry.Action != AttackTimelineAction.Telegraph)
            {
                return;
            }

            StopTelegraph();
            _activeId = entry.PayloadId;
            _remaining = Math.Max(0.01f, entry.Value);
            Started?.Invoke(_activeId, _remaining);
            _events?.Raise(new BossEvent(BossEventType.TelegraphStarted, _activeId, _remaining));
        }

        private void OnAttackCompleted(BossAttackDefinition attack)
        {
            StopTelegraph();
        }

        private void StopTelegraph()
        {
            if (string.IsNullOrEmpty(_activeId))
            {
                _remaining = 0f;
                return;
            }

            string endedId = _activeId;
            _activeId = string.Empty;
            _remaining = 0f;
            Ended?.Invoke(endedId);
            _events?.Raise(new BossEvent(BossEventType.TelegraphEnded, endedId));
        }
    }
}
