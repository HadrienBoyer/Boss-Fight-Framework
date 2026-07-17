using System;
using TappyTale.BossFight.Combat;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Events;
using TappyTale.BossFight.Phases;

namespace TappyTale.BossFight.Core
{
    public sealed class BossRuntime : IDisposable
    {
        private bool _isDisposed;
        private readonly BossEventHub _events;
        private readonly BossHealthService _health;
        private readonly BossPhaseController _phases;

        public BossRuntime(BossContext context, BossDefinition definition = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TickSystem = new BossTickSystem();

            _events = new BossEventHub();
            Context.Services.Register(_events);
            _events.Initialize(Context);

            _health = new BossHealthService();
            Context.Services.Register(_health);
            _health.Initialize(Context);

            if (definition != null && definition.Phases.Count > 0)
            {
                _phases = new BossPhaseController(definition);
                Context.Services.Register(_phases);
                _phases.Initialize(Context);
                TickSystem.Register(_phases);
            }
        }

        public BossContext Context { get; }
        public BossTickSystem TickSystem { get; }
        public BossEventHub Events => _events;
        public BossHealthService Health => _health;
        public BossPhaseController Phases => _phases;
        public bool IsRunning { get; private set; }

        public event Action Started;
        public event Action Stopped;

        public void Start()
        {
            ThrowIfDisposed();
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            Context.Blackboard.IsFightActive = true;
            _phases?.Start();
            Started?.Invoke();
            _events.Raise(new BossEvent(BossEventType.FightStarted));
        }

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();
            if (!IsRunning)
            {
                return;
            }

            UpdateTargetData();
            TickSystem.Tick(deltaTime);
        }

        public void Stop()
        {
            if (_isDisposed || !IsRunning)
            {
                return;
            }

            _phases?.Stop();
            IsRunning = false;
            Context.Blackboard.IsFightActive = false;
            Stopped?.Invoke();
            _events.Raise(new BossEvent(BossEventType.FightStopped));
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Stop();
            TickSystem.Clear();
            _phases?.Shutdown();
            _health.Shutdown();
            _events.Shutdown();
            Context.Services.Clear();
            Started = null;
            Stopped = null;
            _isDisposed = true;
        }

        private void UpdateTargetData()
        {
            if (!Context.HasTarget)
            {
                Context.Blackboard.DistanceToTarget = float.PositiveInfinity;
                Context.Blackboard.DirectionToTarget = default;
                return;
            }

            UnityEngine.Vector3 offset = Context.TargetTransform.position - Context.BossTransform.position;
            Context.Blackboard.DistanceToTarget = offset.magnitude;
            Context.Blackboard.DirectionToTarget = offset.sqrMagnitude > 0.0001f ? offset.normalized : default;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(BossRuntime));
            }
        }
    }
}
