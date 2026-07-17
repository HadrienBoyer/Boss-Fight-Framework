using System.IO;
using TappyTale.BossFight.Combat;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Data;
using TappyTale.BossFight.Demo;
using TappyTale.BossFight.Feedback;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TappyTale.BossFight.Editor.Demo
{
    public static class BossDemoSceneBuilder
    {
        private const string DemoFolder = "Assets/TappyTale/BossFightFramework/Demo";
        private const string ScenePath = DemoFolder + "/BossFightDemo.unity";

        [MenuItem("Tools/Tappy Tale/Boss Fight/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            EnsureFolder(DemoFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena";
            ground.transform.localScale = Vector3.one * 3f;

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Demo Player";
            player.transform.position = new Vector3(0f, 1f, 7f);
            player.AddComponent<BossDemoHealthReceiver>();

            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "Demo Boss";
            boss.transform.position = new Vector3(0f, 1f, 0f);
            BossDirector director = boss.AddComponent<BossDirector>();
            boss.AddComponent<BossDamageReceiver>();
            boss.AddComponent<BossDashExecutor>();
            boss.AddComponent<AudioSource>();
            boss.AddComponent<BossTimelineFeedbackAdapter>();

            GameObject hitboxObject = new("Melee Hitbox");
            hitboxObject.transform.SetParent(boss.transform, false);
            hitboxObject.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            BoxCollider hitbox = hitboxObject.AddComponent<BoxCollider>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector3(2f, 2f, 3f);
            Rigidbody body = hitboxObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            hitboxObject.AddComponent<BossMeleeHitbox>();

            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 15f, -16f), Quaternion.Euler(35f, 0f, 0f));
            camera.fieldOfView = 55f;

            GameObject lightObject = new("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            SerializedObject directorObject = new(director);
            directorObject.FindProperty("target").objectReferenceValue = player.transform;
            directorObject.FindProperty("definition").objectReferenceValue = FindFirstAsset<BossDefinition>();
            directorObject.FindProperty("payloadLibrary").objectReferenceValue = FindFirstAsset<BossPayloadLibrary>();
            directorObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = boss;
            Debug.Log($"Boss Fight demo scene created at {ScenePath}.");
        }

        private static T FindFirstAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void EnsureFolder(string fullPath)
        {
            string[] parts = fullPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}