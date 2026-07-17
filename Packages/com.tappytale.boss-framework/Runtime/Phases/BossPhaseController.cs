using System;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Events;

namespace TappyTale.BossFight.Phases
{
    public sealed class BossPhaseController : IBossService, IBossTickable
    {
        private readonly BossDefinition _definition;
        private BossContext _context;
        private BossEventHub _events;

        public BossPhaseController(BossDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public int TickOrder => -100;
        public BossPhaseRuntime CurrentPhase { get; private set; }

        public event Action<BossPhaseRuntime> PhaseStarted;
        public event Action<BossPhaseRuntime> PhaseEnded;

        public void Initialize(BossContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            context.Services.TryResolve(out _events);
        }

        public void Start()
        {
            if (_definition.Phases.Count > 0)
            {
                EnterPhase(0);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_context == null || CurrentPhase == null)
            {
                return;
            }

            CurrentPhase.Tick(deltaTime);
            int desiredIndex = ResolvePhaseIndex(_context.Blackboard.NormalizedHealth);
            if (desiredIndex != CurrentPhase.Index)
            {
                EnterPhase(desiredIndex);
            }
        }

        public void Stop()
        {
            ExitCurrentPhase();
        }

        public void Shutdown()
        {
            Stop();
            PhaseStarted = null;
            PhaseEnded = null;
            _events = null;
            _context = null;
        }

        private int ResolvePhaseIndex(float normalizedHealth)
        {
            int desiredIndex = 0;
            for (int i = _definition.Phases.Count - 1; i >= 0; i--)
            {
                BossPhaseDefinition phase = _definition.Phases[i];
                if (phase != null && normalizedHealth <= phase.EnterAtNormalizedHealth)
                {
                    desiredIndex = i;
                    break;
                }
            }

            return desiredIndex;
        }

        private void EnterPhase(int index)
        {
            if (index < 0 || index >= _definition.Phases.Count || _definition.Phases[index] == null)
            {
                return;
            }

            ExitCurrentPhase();
            CurrentPhase = new BossPhaseRuntime(_definition.Phases[index], index);
            CurrentPhase.Enter();
            _context.Blackboard.CurrentPhaseIndex = index;
            PhaseStarted?.Invoke(CurrentPhase);
            _events?.Raise(new BossEvent(BossEventType.PhaseStarted, CurrentPhase.Definition.Id, index: index));
        }

        private void ExitCurrentPhase()
        {
            if (CurrentPhase == null)
            {
                return;
            }

            BossPhaseRuntime previous = CurrentPhase;
            previous.Exit();
            PhaseEnded?.Invoke(previous);
            _events?.Raise(new BossEvent(BossEventType.PhaseEnded, previous.Definition.Id, index: previous.Index));
            CurrentPhase = null;
        }
    }
}
