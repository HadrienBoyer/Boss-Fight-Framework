using System;
using System.Collections.Generic;
using UnityEngine;

namespace TappyTale.BossFight.Attacks
{
    public enum AttackTimelineAction
    {
        Telegraph,
        OpenParryWindow,
        OpenPerfectDodgeWindow,
        MeleeHit,
        SpawnProjectile,
        Dash,
        PlayAudio,
        PlayVfx,
        Custom
    }

    [Serializable]
    public struct AttackTimelineEntry
    {
        [SerializeField, Min(0f)] private float time;
        [SerializeField] private AttackTimelineAction action;
        [SerializeField] private string payloadId;
        [SerializeField] private float value;

        public float Time => time;
        public AttackTimelineAction Action => action;
        public string PayloadId => payloadId;
        public float Value => value;
    }

    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Attack Timeline", fileName = "AttackTimeline_")]
    public sealed class AttackTimelineDefinition : ScriptableObject
    {
        [SerializeField, Min(0f)] private float duration = 1f;
        [SerializeField] private List<AttackTimelineEntry> entries = new();

        public float Duration => duration;
        public IReadOnlyList<AttackTimelineEntry> Entries => entries;
    }
}
