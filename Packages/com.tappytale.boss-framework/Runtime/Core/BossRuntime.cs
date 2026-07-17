using System;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Combat;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Events;
using TappyTale.BossFight.Phases;
using TappyTale.BossFight.Projectiles;
using TappyTale.BossFight.Telegraphs;

namespace TappyTale.BossFight.Core
{
    public sealed class BossRuntime : IDisposable
    {
        private bool _isDisposed;
        private readonly BossEventHub _events;
        private readonly BossHealthService _health;
        private readonly BossPhaseController _phases;
        private readonly BossTimelineRunner _timeline;
        private readonly BossAttackScheduler _scheduler;
        private readonly BossReactionWindowService _reactionWindows;
        private readonly BossTelegraphService _telegraphs;
        private readonly BossProjectilePool _projectiles;

        public BossRuntime(BossContext context, BossDefinition definition = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TickSystem = new BossTickSystem();

            _events = RegisterService(new BossEventHub());
            _health = RegisterService(new BossHealthService());

            if (definition != null && definition.Phases.Count > 0)
            {
                _phases = RegisterService(new BossPhaseController(definition));
                TickSystem.Register(_phases);
            }

            _timeline = RegisterService(new BossTimelineRunner());
            TickSystem.Register(_timeline);

            _reactionWindows = RegisterService(new BossReactionWindowService());
            TickSystem.Register(_reactionWindows);

            _telegraphs = RegisterService(new BossTelegraphService());
            TickSystem.Register(_telegraphs);

            _projectiles = RegisterService(new BossProjectilePool());

            if (_phases != null)
            {
                _scheduler = RegisterService(new BossAttackScheduler());
                TickSystem.Register(_scheduler);
            }
        }

        public BossContext Context { get; }
        public BossTickSystem TickSystem { get; }
        public BossEventHub Events => _events;
        public BossHealthService Health => _health;
        public BossPhaseController Phases => _phases;
        public BossTimelineRunner Timeline => _timeline;
        public BossAttackScheduler Scheduler => _scheduler;
        public BossReactionWindowService ReactionWindows => _reactionWindows;
        public BossTelegraphService Telegraphs => _telegraphs;
        public BossProjectilePool Projectiles => _projectiles;
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

            _timeline.Cancel();
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
            _scheduler?.Shutdown();
            _projectiles.Shutdown();
            _telegraphs.Shutdown();
            _reactionWindows.Shutdown();
            _timeline.Shutdown();
            _phases?.Shutdown();
            _health.Shutdown();
            _events.Shutdown();
            Context.Services.Clear();
            Started = null;
            Stopped = null;
            _isDisposed = true;
        }

        private TService RegisterService<TService>(TService service) where TService : class, IBossService
        {
            Context.Services.Register(service);
            service.Initialize(Context);
            return service;
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
