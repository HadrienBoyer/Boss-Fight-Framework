using UnityEngine;

namespace TappyTale.BossFight.Feedback
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Audio Cue", fileName = "AudioCue_")]
    public sealed class AudioCueDefinition : ScriptableObject
    {
        [SerializeField] private string id = "audio";
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private Vector2 pitchRange = Vector2.one;
        [SerializeField] private bool spatial = true;

        public string Id => id;
        public AudioClip[] Clips => clips;
        public float Volume => volume;
        public Vector2 PitchRange => pitchRange;
        public bool Spatial => spatial;
    }
}
