using System.Collections.Generic;
using TappyTale.BossFight.Phases;
using UnityEngine;

namespace TappyTale.BossFight.Data
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Boss Definition", fileName = "Boss_")]
    public sealed class BossDefinition : ScriptableObject
    {
        [SerializeField] private string id = "boss";
        [SerializeField] private string displayName = "Boss";
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private List<BossPhaseDefinition> phases = new();

        public string Id => id;
        public string DisplayName => displayName;
        public float MaxHealth => maxHealth;
        public IReadOnlyList<BossPhaseDefinition> Phases => phases;
    }
}
