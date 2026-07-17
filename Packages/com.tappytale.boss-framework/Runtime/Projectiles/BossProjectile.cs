using UnityEngine;

namespace TappyTale.BossFight.Projectiles
{
    [DisallowMultipleComponent]
    public sealed class BossProjectile : MonoBehaviour
    {
        private BossProjectilePool _owner;
        private ProjectileDefinition _definition;
        private Vector3 _velocity;
        private float _remainingLifetime;

        public ProjectileDefinition Definition => _definition;
        public float Damage => _definition != null ? _definition.Damage : 0f;
        public bool CanBeParried => _definition != null && _definition.CanBeParried;
        public bool IsActive { get; private set; }

        internal void Launch(BossProjectilePool owner, ProjectileDefinition definition, Vector3 position, Vector3 direction)
        {
            _owner = owner;
            _definition = definition;
            transform.SetPositionAndRotation(position, direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction) : Quaternion.identity);
            _velocity = direction.normalized * definition.Speed;
            _remainingLifetime = definition.Lifetime;
            IsActive = true;
            gameObject.SetActive(true);
        }

        internal void ResetForPool()
        {
            IsActive = false;
            _definition = null;
            _velocity = default;
            _remainingLifetime = 0f;
            gameObject.SetActive(false);
        }

        public void Despawn()
        {
            if (IsActive)
            {
                _owner?.Despawn(this);
            }
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            transform.position += _velocity * Time.deltaTime;
            _remainingLifetime -= Time.deltaTime;
            if (_remainingLifetime <= 0f)
            {
                Despawn();
            }
        }
    }
}
