using System;

namespace TappyTale.BossFight.Core
{
    public sealed class BossRuntime : IDisposable
    {
        private bool _isDisposed;

        public BossRuntime(BossContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            TickSystem = new BossTickSystem();
        }

        public BossContext Context { get; }
        public BossTickSystem TickSystem { get; }
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
            Started?.Invoke();
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

            IsRunning = false;
            Context.Blackboard.IsFightActive = false;
            Stopped?.Invoke();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Stop();
            TickSystem.Clear();
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
