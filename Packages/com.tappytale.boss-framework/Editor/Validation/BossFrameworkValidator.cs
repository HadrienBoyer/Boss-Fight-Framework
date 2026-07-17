using System;
using System.Collections.Generic;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Phases;
using UnityEditor;
using UnityEngine;

namespace TappyTale.BossFight.Editor.Validation
{
    public static class BossFrameworkValidator
    {
        public static BossValidationReport ValidateProject()
        {
            BossValidationReport report = new();
            ValidateBosses(report);
            ValidatePhases(report);
            ValidateAttacks(report);
            return report;
        }

        private static void ValidateBosses(BossValidationReport report)
        {
            foreach (BossDefinition boss in LoadAssets<BossDefinition>())
            {
                string path = AssetDatabase.GetAssetPath(boss);
                if (string.IsNullOrWhiteSpace(boss.Id))
                {
                    report.Add(BossValidationSeverity.Error, "Boss id is empty.", path);
                }

                if (boss.MaxHealth <= 0f)
                {
                    report.Add(BossValidationSeverity.Error, "Boss max health must be greater than zero.", path);
                }

                if (boss.Phases == null || boss.Phases.Count == 0)
                {
                    report.Add(BossValidationSeverity.Warning, "Boss has no phases.", path);
                    continue;
                }

                HashSet<string> phaseIds = new(StringComparer.Ordinal);
                for (int index = 0; index < boss.Phases.Count; index++)
                {
                    BossPhaseDefinition phase = boss.Phases[index];
                    if (phase == null)
                    {
                        report.Add(BossValidationSeverity.Error, $"Boss phase slot {index} is null.", path);
                        continue;
                    }

                    if (!phaseIds.Add(phase.Id))
                    {
                        report.Add(BossValidationSeverity.Error, $"Duplicate phase id '{phase.Id}'.", path);
                    }
                }
            }
        }

        private static void ValidatePhases(BossValidationReport report)
        {
            foreach (BossPhaseDefinition phase in LoadAssets<BossPhaseDefinition>())
            {
                string path = AssetDatabase.GetAssetPath(phase);
                if (string.IsNullOrWhiteSpace(phase.Id))
                {
                    report.Add(BossValidationSeverity.Error, "Phase id is empty.", path);
                }

                if (phase.EnterAtNormalizedHealth < 0f || phase.EnterAtNormalizedHealth > 1f)
                {
                    report.Add(BossValidationSeverity.Error, "Phase threshold must be between 0 and 1.", path);
                }

                if (phase.Attacks == null || phase.Attacks.Count == 0)
                {
                    report.Add(BossValidationSeverity.Warning, "Phase has no attacks.", path);
                }
            }
        }

        private static void ValidateAttacks(BossValidationReport report)
        {
            foreach (BossAttackDefinition attack in LoadAssets<BossAttackDefinition>())
            {
                string path = AssetDatabase.GetAssetPath(attack);
                if (string.IsNullOrWhiteSpace(attack.Id))
                {
                    report.Add(BossValidationSeverity.Error, "Attack id is empty.", path);
                }

                if (attack.Cooldown < 0f)
                {
                    report.Add(BossValidationSeverity.Error, "Attack cooldown cannot be negative.", path);
                }

                if (attack.Timeline == null)
                {
                    report.Add(BossValidationSeverity.Warning, "Attack has no timeline.", path);
                }
            }
        }

        private static IEnumerable<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }
    }
}
