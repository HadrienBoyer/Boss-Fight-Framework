using UnityEngine;

namespace TappyTale.BossFight.Core
{
    [DisallowMultipleComponent]
    public sealed class BossDirector : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private bool startOnEnable = true;

        public BossRuntime Runtime { get; private set; }
        public BossBlackboard Blackboard => Runtime?.Context.Blackboard;

        private void Awake()
        {
            BuildRuntime();
        }

        private void OnEnable()
        {
            if (startOnEnable)
            {
                StartFight();
            }
        }

        private void Update()
        {
            Runtime?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            Runtime?.Stop();
        }

        private void OnDestroy()
        {
            Runtime?.Dispose();
            Runtime = null;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (Runtime != null)
            {
                Runtime.Context.TargetTransform = newTarget;
            }
        }

        public void StartFight()
        {
            if (Runtime == null)
            {
                BuildRuntime();
            }

            Runtime.Start();
        }

        public void StopFight()
        {
            Runtime?.Stop();
        }

        private void BuildRuntime()
        {
            Runtime?.Dispose();

            BossBlackboard blackboard = new()
            {
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth,
                CurrentPhaseIndex = 0
            };

            BossServiceRegistry services = new();
            BossContext context = new(gameObject, transform, target, blackboard, services);
            Runtime = new BossRuntime(context);
        }
    }
}
