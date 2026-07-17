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
        [SerializeField, Min(0f)] private float activeDuration = 0.1f;
        private readonly HashSet<IBossDamageReceiver> _hitReceivers = new();
        private BossTimelineActionExecutor _executor;
        private float _activeDamage;
        private float _remaining;

        private void Awake()
        {
            if (director == null) director = GetComponentInParent<BossDirector>();
            if (hitbox == null) hitbox = GetComponent<Collider>();
            if (hitbox != null) hitbox.enabled = false;
        }

        private void OnEnable() => TryBind();

        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Deactivate();
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
            _remaining = activeDuration;
            _hitReceivers.Clear();
            if (hitbox != null) hitbox.enabled = true;
        }

        public void Deactivate()
        {
            _remaining = 0f;
            if (hitbox != null) hitbox.enabled = false;
            _hitReceivers.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_remaining <= 0f) return;
            MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IBossDamageReceiver receiver || !_hitReceivers.Add(receiver)) continue;
                receiver.ReceiveDamage(_activeDamage, gameObject);
                break;
            }
        }
    }
}