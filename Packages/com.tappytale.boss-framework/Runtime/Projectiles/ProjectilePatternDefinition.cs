using UnityEngine;

namespace TappyTale.BossFight.Projectiles
{
    public enum ProjectilePatternKind
    {
        Single,
        Burst,
        Spread,
        Radial,
        Spiral,
        Custom
    }

    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Projectile Pattern", fileName = "ProjectilePattern_")]
    public sealed class ProjectilePatternDefinition : ScriptableObject
    {
        [SerializeField] private string id = "pattern";
        [SerializeField] private ProjectilePatternKind kind;
        [SerializeField] private ProjectileDefinition projectile;
        [SerializeField, Min(1)] private int count = 1;
        [SerializeField, Min(0f)] private float interval = 0.1f;
        [SerializeField, Range(0f, 360f)] private float arc = 360f;
        [SerializeField] private float angularOffset;

        public string Id => id;
        public ProjectilePatternKind Kind => kind;
        public ProjectileDefinition Projectile => projectile;
        public int Count => count;
        public float Interval => interval;
        public float Arc => arc;
        public float AngularOffset => angularOffset;
    }
}
