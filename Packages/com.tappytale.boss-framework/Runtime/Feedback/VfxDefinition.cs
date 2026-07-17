using UnityEngine;

namespace TappyTale.BossFight.Feedback
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/VFX Definition", fileName = "VFX_")]
    public sealed class VfxDefinition : ScriptableObject
    {
        [SerializeField] private string id = "vfx";
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0f)] private float lifetime = 2f;
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField] private bool followSource;

        public string Id => id;
        public GameObject Prefab => prefab;
        public float Lifetime => lifetime;
        public Vector3 LocalOffset => localOffset;
        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);
        public bool FollowSource => followSource;
    }
}
