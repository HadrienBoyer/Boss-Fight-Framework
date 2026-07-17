using UnityEngine;

namespace TappyTale.BossFight.Attacks
{
    public enum BossAttackKind
    {
        Melee,
        Dash,
        Projectile,
        Laser,
        Summon,
        Custom
    }

    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Attack Definition", fileName = "BossAttack_")]
    public sealed class BossAttackDefinition : ScriptableObject
    {
        [SerializeField] private string id = "attack";
        [SerializeField] private BossAttackKind kind;
        [SerializeField, Min(0f)] private float cooldown = 1f;
        [SerializeField, Min(0f)] private float minimumRange;
        [SerializeField, Min(0f)] private float maximumRange = 20f;
        [SerializeField, Min(0)] private int priority;
        [SerializeField] private bool canBeParried;
        [SerializeField] private bool canBePerfectDodged = true;

        public string Id => id;
        public BossAttackKind Kind => kind;
        public float Cooldown => cooldown;
        public float MinimumRange => minimumRange;
        public float MaximumRange => maximumRange;
        public int Priority => priority;
        public bool CanBeParried => canBeParried;
        public bool CanBePerfectDodged => canBePerfectDodged;

        public bool IsInRange(float distance)
        {
            return distance >= minimumRange && distance <= maximumRange;
        }

        private void OnValidate()
        {
            if (maximumRange < minimumRange)
            {
                maximumRange = minimumRange;
            }
        }
    }
}
