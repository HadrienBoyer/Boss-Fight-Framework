using System;
using System.Collections.Generic;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Phases;

namespace TappyTale.BossFight.Attacks
{
    public sealed class BossAttackScheduler : IBossService, IBossTickable
    {
        private readonly List<BossAttackRuntime> _attacks = new();
        private BossContext _context;
        private BossPhaseController _phases;
        private BossTimelineRunner _runner;
        private int _roundRobinIndex;

        public int TickOrder => 0;
        public bool Enabled { get; set; } = true;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _phases);
            context.Services.TryResolve(out _runner);
            if (_phases != null)
            {
                _phases.PhaseStarted += OnPhaseStarted;
                if (_phases.CurrentPhase != null)
                {
                    Rebuild(_phases.CurrentPhase.Definition.Attacks);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _attacks.Count; i++)
            {
                _attacks[i].Tick(deltaTime);
            }

            if (!Enabled || _runner == null || _runner.IsRunning || _attacks.Count == 0)
            {
                return;
            }

            BossAttackRuntime selected = SelectNext();
            if (selected != null && _runner.Play(selected.Definition))
            {
                selected.CommitCooldown();
            }
        }

        public void Shutdown()
        {
            if (_phases != null)
            {
                _phases.PhaseStarted -= OnPhaseStarted;
            }

            _attacks.Clear();
            _phases = null;
            _runner = null;
            _context = null;
        }

        private BossAttackRuntime SelectNext()
        {
            float distance = _context.Blackboard.DistanceToTarget;
            for (int offset = 0; offset < _attacks.Count; offset++)
            {
                int index = (_roundRobinIndex + offset) % _attacks.Count;
                BossAttackRuntime attack = _attacks[index];
                if (attack.IsReady && attack.Definition.IsInRange(distance) && attack.Definition.Timeline != null)
                {
                    _roundRobinIndex = (index + 1) % _attacks.Count;
                    return attack;
                }
            }

            return null;
        }

        private void OnPhaseStarted(BossPhaseRuntime phase)
        {
            Rebuild(phase.Definition.Attacks);
        }

        private void Rebuild(IReadOnlyList<BossAttackDefinition> definitions)
        {
            _attacks.Clear();
            _roundRobinIndex = 0;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null)
                {
                    _attacks.Add(new BossAttackRuntime(definitions[i]));
                }
            }

            _attacks.Sort(static (left, right) => right.Definition.Priority.CompareTo(left.Definition.Priority));
        }
    }
}
