using System.IO;
using TappyTale.BossFight.Attacks;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Phases;
using TappyTale.BossFight.Projectiles;
using TappyTale.BossFight.Telegraphs;
using UnityEditor;
using UnityEngine;

namespace TappyTale.BossFight.Editor.Install
{
    public static class BossFrameworkInstaller
    {
        public const string RootFolder = "Assets/TappyTale/BossFightFramework";
        public const string DataFolder = RootFolder + "/Data";
        public const string PrefabFolder = RootFolder + "/Prefabs";

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
            TelegraphDefinition telegraph = CreateAsset<TelegraphDefinition>(DataFolder + "/Telegraph_Default.asset");
            Configure(telegraph, "id", "default_telegraph");
            Configure(telegraph, "duration", 0.5f);
            Configure(telegraph, "radius", 2f);

            GameObject projectilePrefab = CreateProjectilePrefab();
            ProjectileDefinition projectileDefinition = CreateAsset<ProjectileDefinition>(DataFolder + "/Projectile_Default.asset");
            Configure(projectileDefinition, "id", "default_projectile");
            Configure(projectileDefinition, "prefab", projectilePrefab);
            Configure(projectileDefinition, "speed", 12f);
            Configure(projectileDefinition, "lifetime", 5f);
            Configure(projectileDefinition, "damage", 12f);
            Configure(projectileDefinition, "canBeParried", true);

            AttackTimelineDefinition meleeTimeline = CreateAsset<AttackTimelineDefinition>(DataFolder + "/Timeline_Melee.asset");
            ConfigureTimeline(meleeTimeline, 1.2f,
                (0f, AttackTimelineAction.Telegraph, telegraph.Id, 0.5f),
                (0.25f, AttackTimelineAction.OpenParryWindow, string.Empty, 0.35f),
                (0.6f, AttackTimelineAction.MeleeHit, string.Empty, 18f));

            AttackTimelineDefinition projectileTimeline = CreateAsset<AttackTimelineDefinition>(DataFolder + "/Timeline_Projectile.asset");
            ConfigureTimeline(projectileTimeline, 1.4f,
                (0f, AttackTimelineAction.Telegraph, telegraph.Id, 0.6f),
                (0.35f, AttackTimelineAction.OpenPerfectDodgeWindow, string.Empty, 0.3f),
                (0.7f, AttackTimelineAction.SpawnProjectile, projectileDefinition.Id, 0f));

            BossAttackDefinition melee = CreateAsset<BossAttackDefinition>(DataFolder + "/Attack_Melee.asset");
            Configure(melee, "id", "melee");
            Configure(melee, "kind", BossAttackKind.Melee);
            Configure(melee, "timeline", meleeTimeline);
            Configure(melee, "cooldown", 1.5f);
            Configure(melee, "maximumRange", 3f);
            Configure(melee, "canBeParried", true);

            BossAttackDefinition projectile = CreateAsset<BossAttackDefinition>(DataFolder + "/Attack_Projectile.asset");
            Configure(projectile, "id", "projectile");
            Configure(projectile, "kind", BossAttackKind.Projectile);
            Configure(projectile, "timeline", projectileTimeline);
            Configure(projectile, "cooldown", 2.5f);
            Configure(projectile, "minimumRange", 2f);
            Configure(projectile, "maximumRange", 20f);

            BossPhaseDefinition phase = CreateAsset<BossPhaseDefinition>(DataFolder + "/Phase_One.asset");
            Configure(phase, "id", "phase_one");
            Configure(phase, "enterAtNormalizedHealth", 1f);
            SetObjectArray(phase, "attacks", melee, projectile);

            BossDefinition boss = CreateAsset<BossDefinition>(DataFolder + "/Boss_Starter.asset");
            Configure(boss, "id", "starter_boss");
            Configure(boss, "displayName", "Starter Boss");
            Configure(boss, "maxHealth", 500f);
            SetObjectArray(boss, "phases", phase);

            BossDatabase database = CreateAsset<BossDatabase>(DataFolder + "/BossDatabase.asset");
            SetObjectArray(database, "bosses", boss);

            BossPayloadLibrary payloads = CreateAsset<BossPayloadLibrary>(DataFolder + "/BossPayloadLibrary.asset");
            SetObjectArray(payloads, "projectiles", projectileDefinition);
            SetObjectArray(payloads, "telegraphs", telegraph);
        }

        private static GameObject CreateProjectilePrefab()
        {
            string path = PrefabFolder + "/BossProjectile_Default.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            instance.name = "BossProjectile_Default";
            instance.transform.localScale = Vector3.one * 0.35f;
            SphereCollider collider = instance.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            Rigidbody body = instance.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            instance.AddComponent<BossProjectile>();
            instance.AddComponent<BossProjectileImpact>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static void ConfigureTimeline(AttackTimelineDefinition timeline, float duration,
            params (float time, AttackTimelineAction action, string payloadId, float value)[] entries)
        {
            SerializedObject serialized = new(timeline);
            serialized.FindProperty("duration").floatValue = duration;
            SerializedProperty list = serialized.FindProperty("entries");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("time").floatValue = entries[i].time;
                entry.FindPropertyRelative("action").enumValueIndex = (int)entries[i].action;
                entry.FindPropertyRelative("payloadId").stringValue = entries[i].payloadId;
                entry.FindPropertyRelative("value").floatValue = entries[i].value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(Object target, string propertyName, params Object[] values)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
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
                case string stringValue: property.stringValue = stringValue; break;
                case float floatValue: property.floatValue = floatValue; break;
                case bool boolValue: property.boolValue = boolValue; break;
                case System.Enum enumValue: property.enumValueIndex = System.Convert.ToInt32(enumValue); break;
                case Object objectValue: property.objectReferenceValue = objectValue; break;
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string parent, string name)
        {
            string fullPath = Path.Combine(parent, name).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(fullPath)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}