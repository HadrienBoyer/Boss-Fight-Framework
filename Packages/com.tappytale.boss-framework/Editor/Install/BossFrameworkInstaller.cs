using System.IO;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Phases;
using UnityEditor;
using UnityEngine;

namespace TappyTale.BossFight.Editor.Install
{
    public static class BossFrameworkInstaller
    {
        public const string RootFolder = "Assets/TappyTale/BossFightFramework";
        public const string DataFolder = RootFolder + "/Data";

        [MenuItem("Tools/Tappy Tale/Boss Fight/Install or Repair")]
        public static void InstallOrRepair()
        {
            EnsureFolder("Assets", "TappyTale");
            EnsureFolder("Assets/TappyTale", "BossFightFramework");
            EnsureFolder(RootFolder, "Data");
            EnsureFolder(RootFolder, "Demo");
            EnsureFolder(RootFolder, "Prefabs");

            CreateStarterAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Boss Fight Framework installation completed.");
        }

        private static void CreateStarterAssets()
        {
            string bossPath = DataFolder + "/Boss_Starter.asset";
            if (AssetDatabase.LoadAssetAtPath<BossDefinition>(bossPath) != null)
            {
                return;
            }

            BossAttackDefinition melee = CreateAsset<BossAttackDefinition>(DataFolder + "/Attack_Melee.asset");
            Configure(melee, "id", "melee");
            Configure(melee, "kind", BossAttackKind.Melee);
            Configure(melee, "cooldown", 1.5f);
            Configure(melee, "maximumRange", 3f);
            Configure(melee, "canBeParried", true);

            BossAttackDefinition projectile = CreateAsset<BossAttackDefinition>(DataFolder + "/Attack_Projectile.asset");
            Configure(projectile, "id", "projectile");
            Configure(projectile, "kind", BossAttackKind.Projectile);
            Configure(projectile, "cooldown", 2.5f);
            Configure(projectile, "minimumRange", 2f);
            Configure(projectile, "maximumRange", 20f);

            BossPhaseDefinition phase = CreateAsset<BossPhaseDefinition>(DataFolder + "/Phase_One.asset");
            Configure(phase, "id", "phase_one");
            Configure(phase, "enterAtNormalizedHealth", 1f);
            SerializedObject phaseObject = new(phase);
            SerializedProperty attacks = phaseObject.FindProperty("attacks");
            attacks.arraySize = 2;
            attacks.GetArrayElementAtIndex(0).objectReferenceValue = melee;
            attacks.GetArrayElementAtIndex(1).objectReferenceValue = projectile;
            phaseObject.ApplyModifiedPropertiesWithoutUndo();

            BossDefinition boss = CreateAsset<BossDefinition>(bossPath);
            Configure(boss, "id", "starter_boss");
            Configure(boss, "displayName", "Starter Boss");
            Configure(boss, "maxHealth", 500f);
            SerializedObject bossObject = new(boss);
            SerializedProperty phases = bossObject.FindProperty("phases");
            phases.arraySize = 1;
            phases.GetArrayElementAtIndex(0).objectReferenceValue = phase;
            bossObject.ApplyModifiedPropertiesWithoutUndo();

            BossDatabase database = CreateAsset<BossDatabase>(DataFolder + "/BossDatabase.asset");
            SerializedObject databaseObject = new(database);
            SerializedProperty bosses = databaseObject.FindProperty("bosses");
            bosses.arraySize = 1;
            bosses.GetArrayElementAtIndex(0).objectReferenceValue = boss;
            databaseObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Configure(Object target, string propertyName, object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            switch (value)
            {
                case string stringValue:
                    property.stringValue = stringValue;
                    break;
                case float floatValue:
                    property.floatValue = floatValue;
                    break;
                case bool boolValue:
                    property.boolValue = boolValue;
                    break;
                case System.Enum enumValue:
                    property.enumValueIndex = System.Convert.ToInt32(enumValue);
                    break;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string name)
        {
            string fullPath = Path.Combine(parent, name).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
