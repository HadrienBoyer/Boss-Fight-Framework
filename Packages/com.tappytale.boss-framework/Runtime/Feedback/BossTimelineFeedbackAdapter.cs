using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Data;
using UnityEngine;

namespace TappyTale.BossFight.Feedback
{
    [DisallowMultipleComponent]
    public sealed class BossTimelineFeedbackAdapter : MonoBehaviour
    {
        [SerializeField] private BossDirector director;
        [SerializeField] private AudioSource audioSource;
        private BossTimelineActionExecutor _executor;

        private void Awake()
        {
            if (director == null) director = GetComponentInParent<BossDirector>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable() => Bind();

        private void OnDisable()
        {
            if (_executor != null)
            {
                _executor.AudioRequested -= PlayAudio;
                _executor.VfxRequested -= PlayVfx;
            }
            _executor = null;
        }

        private void Bind()
        {
            _executor = director != null ? director.Runtime?.ActionExecutor : null;
            if (_executor == null) return;
            _executor.AudioRequested += PlayAudio;
            _executor.VfxRequested += PlayVfx;
        }

        private void PlayAudio(AttackTimelineEntry entry)
        {
            BossPayloadResolver resolver = director?.Runtime?.Payloads;
            if (resolver == null || !resolver.TryGetAudio(entry.PayloadId, out AudioCueDefinition cue) || cue.Clips == null || cue.Clips.Length == 0) return;
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            AudioClip clip = cue.Clips[Random.Range(0, cue.Clips.Length)];
            if (clip == null) return;
            audioSource.pitch = Random.Range(cue.PitchRange.x, cue.PitchRange.y);
            audioSource.spatialBlend = cue.Spatial ? 1f : 0f;
            audioSource.PlayOneShot(clip, cue.Volume);
        }

        private void PlayVfx(AttackTimelineEntry entry)
        {
            BossPayloadResolver resolver = director?.Runtime?.Payloads;
            if (resolver == null || !resolver.TryGetVfx(entry.PayloadId, out VfxDefinition definition) || definition.Prefab == null) return;
            Transform parent = definition.FollowSource ? transform : null;
            GameObject instance = Instantiate(definition.Prefab, parent);
            instance.transform.SetPositionAndRotation(transform.TransformPoint(definition.LocalOffset), transform.rotation * definition.LocalRotation);
            if (definition.Lifetime > 0f) Destroy(instance, definition.Lifetime);
        }
    }
}