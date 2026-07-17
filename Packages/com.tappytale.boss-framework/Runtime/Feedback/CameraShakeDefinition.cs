using UnityEngine;

namespace TappyTale.BossFight.Feedback
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Camera Shake", fileName = "CameraShake_")]
    public sealed class CameraShakeDefinition : ScriptableObject
    {
        [SerializeField] private string id = "shake";
        [SerializeField, Min(0f)] private float duration = 0.2f;
        [SerializeField, Min(0f)] private float amplitude = 1f;
        [SerializeField, Min(0f)] private float frequency = 10f;
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        public string Id => id;
        public float Duration => duration;
        public float Amplitude => amplitude;
        public float Frequency => frequency;
        public AnimationCurve Falloff => falloff;
    }
}
