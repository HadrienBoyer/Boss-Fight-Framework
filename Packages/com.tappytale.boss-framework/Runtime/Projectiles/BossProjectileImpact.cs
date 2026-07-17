using TappyTale.BossFight.Combat;
using UnityEngine;

namespace TappyTale.BossFight.Projectiles
{
    [DisallowMultipleComponent]
    public sealed class BossProjectileImpact : MonoBehaviour
    {
        [SerializeField] private BossProjectile projectile;
        [SerializeField] private LayerMask hitMask = ~0;

        private void Awake()
        {
            if (projectile == null) projectile = GetComponent<BossProjectile>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (projectile == null || !projectile.IsActive) return;
            if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

            MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IBossDamageReceiver receiver) continue;
                receiver.ReceiveDamage(projectile.Damage, gameObject);
                projectile.Despawn();
                return;
            }

            projectile.Despawn();
        }
    }
}