using System.IO;
using TappyTale.BossFight.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TappyTale.BossFight.Editor.Demo
{
    public static class PlayerCoreDemoBuilder
    {
        private const string RootFolder = "Assets/TappyTale/BossFightFramework";
        private const string DataFolder = RootFolder + "/Data";
        private const string DemoFolder = RootFolder + "/Demo";
        private const string DefinitionPath = DataFolder + "/Player_Starter.asset";
        private const string ScenePath = DemoFolder + "/PlayerCoreDemo.unity";

        [MenuItem("Tools/Tappy Tale/Boss Fight/Create Player Core Demo")]
        public static void CreateDemo()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(DemoFolder);
            PlayerDefinition definition = LoadOrCreateDefinition();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Player Test Arena";
            ground.transform.localScale = Vector3.one * 4f;

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player Core";
            player.transform.position = new Vector3(0f, 1.05f, 0f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController character = player.AddComponent<CharacterController>();
            character.height = 2f;
            character.radius = 0.45f;
            character.center = Vector3.zero;
            player.AddComponent<PlayerMotor>();
            player.AddComponent<LegacyPlayerInputSource>();
            PlayerController controller = player.AddComponent<PlayerController>();

            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0f, 10f, -12f),
                Quaternion.Euler(35f, 0f, 0f));

            GameObject lightObject = new("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            SerializedObject serialized = new(controller);
            serialized.FindProperty("definition").objectReferenceValue = definition;
            serialized.FindProperty("cameraTransform").objectReferenceValue = cameraObject.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = player;
            Debug.Log($"Player Core demo created at {ScenePath}. Move with WASD, jump with Space, dash with Left Shift.");
        }

        private static PlayerDefinition LoadOrCreateDefinition()
        {
            PlayerDefinition existing = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(DefinitionPath);
            if (existing != null)
            {
                return existing;
            }

            PlayerDefinition definition = ScriptableObject.CreateInstance<PlayerDefinition>();
            AssetDatabase.CreateAsset(definition, DefinitionPath);
            AssetDatabase.SaveAssets();
            return definition;
        }

        private static void EnsureFolder(string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                EnsureFolder(directory);
            }

            if (AssetDatabase.IsValidFolder(fullPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');
            string name = Path.GetFileName(fullPath);
            if (!string.IsNullOrEmpty(parent))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
