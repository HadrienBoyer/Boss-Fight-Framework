using TappyTale.BossFight.Combat;
using TappyTale.BossFight.Data;
using UnityEngine;

namespace TappyTale.BossFight.Core
{
    [DisallowMultipleComponent]
    public sealed class BossDirector : MonoBehaviour
    {
        [SerializeField] private BossDefinition definition;
        [SerializeField] private BossPayloadLibrary payloadLibrary;
        [SerializeField] private Transform target;
        [SerializeField, Min(1f)] private float fallbackMaxHealth = 100f;
        [SerializeField] private bool startOnEnable = true;

        public BossRuntime Runtime { get; private set; }
        public BossBlackboard Blackboard => Runtime?.Context.Blackboard;
        public BossDefinition Definition => definition;
        public BossPayloadLibrary PayloadLibrary => payloadLibrary;

        private void Awake() => BuildRuntime();

        private void OnEnable()
        {
            if (startOnEnable) StartFight();
        }

        private void Update() => Runtime?.Tick(Time.deltaTime);
        private void OnDisable() => Runtime?.Stop();

        private void OnDestroy()
        {
            Runtime?.Dispose();
            Runtime = null;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (Runtime != null) Runtime.Context.TargetTransform = newTarget;
        }

        public void SetDefinition(BossDefinition newDefinition, bool rebuildRuntime = true)
        {
            definition = newDefinition;
            if (!rebuildRuntime) return;
            bool restart = Runtime != null && Runtime.IsRunning;
            BuildRuntime();
            if (restart) Runtime.Start();
        }

        public void SetPayloadLibrary(BossPayloadLibrary newLibrary, bool rebuildRuntime = true)
        {
            payloadLibrary = newLibrary;
            if (!rebuildRuntime) return;
            bool restart = Runtime != null && Runtime.IsRunning;
            BuildRuntime();
            if (restart) Runtime.Start();
        }

        public bool ApplyDamage(float amount, string sourceId = null, bool ignoreInvulnerability = false)
        {
            return Runtime != null && Runtime.Health.ApplyDamage(new BossDamageRequest(amount, sourceId, ignoreInvulnerability));
        }

        public bool TryParry() => Runtime != null && Runtime.ReactionWindows.TryParry();
        public bool TryPerfectDodge() => Runtime != null && Runtime.ReactionWindows.TryPerfectDodge();

        public void StartFight()
        {
            if (Runtime == null) BuildRuntime();
            Runtime.Start();
        }

        public void StopFight() => Runtime?.Stop();

        private void BuildRuntime()
        {
            Runtime?.Dispose();
            float maxHealth = definition != null ? definition.MaxHealth : fallbackMaxHealth;
            BossBlackboard blackboard = new()
            {
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth,
                CurrentPhaseIndex = 0
            };

            BossServiceRegistry services = new();
            BossContext context = new(gameObject, transform, target, blackboard, services);
            Runtime = new BossRuntime(context, definition, payloadLibrary);
        }
    }
}