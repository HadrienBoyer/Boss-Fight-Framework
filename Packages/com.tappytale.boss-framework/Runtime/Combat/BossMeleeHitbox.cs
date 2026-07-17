using System.Collections.Generic;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Core;
using UnityEngine;

namespace TappyTale.BossFight.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossMeleeHitbox : MonoBehaviour
    {
        [SerializeField] private BossDirector director;
        [SerializeField] private Collider hitbox;
        [SerializeField, Min(0f)] private float defaultDamage = 10f;
        private readonly HashSet<IBossDamageReceiver> _hitReceivers = new();
        private BossTimelineActionExecutor _executor;
        private float _activeDamage;
        private bool _isActive;

        private void Awake()
        {
            if (director == null) director = GetComponentInParent<BossDirector>();
            if (hitbox == null) hitbox = GetComponent<Collider>();
            if (hitbox != null) hitbox.enabled = false;
        }

        private void OnEnable()
        {
            TryBind();
        }

        private void OnDisable()
        {
            if (_executor != null) _executor.MeleeRequested -= Activate;
            _executor = null;
            Deactivate();
        }

        private void TryBind()
        {
            _executor = director != null ? director.Runtime?.ActionExecutor : null;
            if (_executor != null) _executor.MeleeRequested += Activate;
        }

        private void Activate(AttackTimelineEntry entry)
        {
            _activeDamage = entry.Value > 0f ? entry.Value : defaultDamage;
            _hitReceivers.Clear();
            _isActive = true;
            if (hitbox != null) hitbox.enabled = true;
        }

        public void Deactivate()
        {
            _isActive = false;
            if (hitbox != null) hitbox.enabled = false;
            _hitReceivers.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            IBossDamageReceiver receiver = other.GetComponentInParent<IBossDamageReceiver>();
            if (receiver != null && _hitReceivers.Add(receiver)) receiver.ReceiveDamage(_activeDamage, gameObject);
        }
    }
}