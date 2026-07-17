using UnityEngine;

namespace TappyTale.BossFight.Projectiles
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Projectile Definition", fileName = "Projectile_")]
    public sealed class ProjectileDefinition : ScriptableObject
    {
        [SerializeField] private string id = "projectile";
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0f)] private float speed = 10f;
        [SerializeField, Min(0f)] private float lifetime = 5f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float radius = 0.1f;
        [SerializeField] private bool canBeParried;

        public string Id => id;
        public GameObject Prefab => prefab;
        public float Speed => speed;
        public float Lifetime => lifetime;
        public float Damage => damage;
        public float Radius => radius;
        public bool CanBeParried => canBeParried;
    }
}
