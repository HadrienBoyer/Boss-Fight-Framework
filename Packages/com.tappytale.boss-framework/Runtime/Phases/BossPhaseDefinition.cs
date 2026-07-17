using System.Collections.Generic;
using TappyTale.BossFight.Attacks;
using UnityEngine;

namespace TappyTale.BossFight.Phases
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Phase Definition", fileName = "BossPhase_")]
    public sealed class BossPhaseDefinition : ScriptableObject
    {
        [SerializeField] private string id = "phase";
        [SerializeField, Range(0f, 1f)] private float enterAtNormalizedHealth = 1f;
        [SerializeField] private List<BossAttackDefinition> attacks = new();

        public string Id => id;
        public float EnterAtNormalizedHealth => enterAtNormalizedHealth;
        public IReadOnlyList<BossAttackDefinition> Attacks => attacks;
    }
}
