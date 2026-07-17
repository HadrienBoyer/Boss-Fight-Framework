using TappyTale.BossFight.Core;
using UnityEngine;

namespace TappyTale.BossFight.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossDamageReceiver : MonoBehaviour, IBossDamageReceiver
    {
        [SerializeField] private BossDirector director;

        private void Reset() => director = GetComponentInParent<BossDirector>();

        public bool ReceiveDamage(float amount, GameObject source)
        {
            if (director == null) director = GetComponentInParent<BossDirector>();
            return director != null && director.ApplyDamage(amount, source != null ? source.name : null);
        }
    }
}