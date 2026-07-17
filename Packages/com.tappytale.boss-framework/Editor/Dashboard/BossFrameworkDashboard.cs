using TappyTale.BossFight.Editor.Install;
using TappyTale.BossFight.Editor.Validation;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace TappyTale.BossFight.Editor.Dashboard
{
#if ODIN_INSPECTOR
    public sealed class BossFrameworkDashboard : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Tappy Tale/Boss Fight/Dashboard")]
        private static void OpenWindow() => GetWindow<BossFrameworkDashboard>("Boss Framework");

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new();
            tree.Add("Overview", new DashboardOverview());
            tree.AddAllAssetsAtPath("Bosses", "Assets", typeof(Data.BossDefinition), true, true);
            tree.AddAllAssetsAtPath("Phases", "Assets", typeof(Phases.BossPhaseDefinition), true, true);
            tree.AddAllAssetsAtPath("Attacks", "Assets", typeof(Attacks.BossAttackDefinition), true, true);
            tree.Add("Validation", new DashboardValidation());
            tree.Add("Installer", new DashboardInstaller());
            return tree;
        }
    }
#else
    public sealed class BossFrameworkDashboard : EditorWindow
    {
        private BossValidationReport _report;
        private Vector2 _scroll;

        [MenuItem("Tools/Tappy Tale/Boss Fight/Dashboard")]
        private static void OpenWindow() => GetWindow<BossFrameworkDashboard>("Boss Framework");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Boss Fight Framework", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Odin Inspector was not detected. The fallback dashboard remains available; install Odin to unlock asset navigation and richer tooling.", MessageType.Info);

            if (GUILayout.Button("Install or Repair"))
            {
                BossFrameworkInstaller.InstallOrRepair();
            }

            if (GUILayout.Button("Validate Project"))
            {
                _report = BossFrameworkValidator.ValidateProject();
            }

            if (_report == null)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Errors: {_report.ErrorCount}   Warnings: {_report.WarningCount}");
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (BossValidationIssue issue in _report.Issues)
            {
                MessageType type = issue.Severity switch
                {
                    BossValidationSeverity.Error => MessageType.Error,
                    BossValidationSeverity.Warning => MessageType.Warning,
                    _ => MessageType.Info
                };
                EditorGUILayout.HelpBox($"{issue.Message}\n{issue.AssetPath}", type);
            }
            EditorGUILayout.EndScrollView();
        }
    }
#endif

    internal sealed class DashboardOverview
    {
        public string Status => "Sprint 1.3 editor tools installed";
        public string RuntimeArchitecture => "ScriptableObject configuration + runtime-owned mutable state";
    }

    internal sealed class DashboardValidation
    {
        public BossValidationReport LastReport { get; private set; }
        public void Validate() => LastReport = BossFrameworkValidator.ValidateProject();
    }

    internal sealed class DashboardInstaller
    {
        public void InstallOrRepair() => BossFrameworkInstaller.InstallOrRepair();
    }
}
