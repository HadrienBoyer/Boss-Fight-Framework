using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Core;
using UnityEngine;

namespace TappyTale.BossFight.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossDashExecutor : MonoBehaviour
    {
        [SerializeField] private BossDirector director;
        [SerializeField] private CharacterController characterController;
        [SerializeField, Min(0f)] private float defaultDistance = 4f;
        private BossTimelineActionExecutor _executor;

        private void Awake()
        {
            if (director == null) director = GetComponentInParent<BossDirector>();
            if (characterController == null) characterController = GetComponent<CharacterController>();
        }

        private void OnEnable() => TryBind();

        private void OnDisable()
        {
            if (_executor != null) _executor.DashRequested -= Dash;
            _executor = null;
        }

        private void TryBind()
        {
            _executor = director != null ? director.Runtime?.ActionExecutor : null;
            if (_executor != null) _executor.DashRequested += Dash;
        }

        private void Dash(AttackTimelineEntry entry)
        {
            Vector3 direction = director != null && director.Blackboard != null
                ? director.Blackboard.DirectionToTarget
                : transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = transform.forward;
            float distance = entry.Value > 0f ? entry.Value : defaultDistance;
            Vector3 motion = direction.normalized * distance;
            if (characterController != null && characterController.enabled) characterController.Move(motion);
            else transform.position += motion;
        }
    }
}