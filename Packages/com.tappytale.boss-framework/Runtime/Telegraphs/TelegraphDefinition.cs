using UnityEngine;

namespace TappyTale.BossFight.Telegraphs
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Telegraph Definition", fileName = "Telegraph_")]
    public sealed class TelegraphDefinition : ScriptableObject
    {
        [SerializeField] private string id = "telegraph";
        [SerializeField, Min(0f)] private float duration = 0.5f;
        [SerializeField] private Color color = Color.red;
        [SerializeField, Min(0f)] private float radius = 1f;
        [SerializeField] private AnimationCurve intensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public string Id => id;
        public float Duration => duration;
        public Color Color => color;
        public float Radius => radius;
        public AnimationCurve Intensity => intensity;
    }
}
