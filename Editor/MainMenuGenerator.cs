using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace MCPForUnity.Editor.Helpers
{
    public static class MainMenuGenerator
    {
        public static void Generate(string preset, bool is2D, int saveSlots, string outputPath, string menuAlignment)
        {
            Debug.Log($"[MainMenuGenerator] Starting generation in path: {outputPath}");

            // 1. Create directories
            string scenesDir = Path.Combine(outputPath, "Scenes");
            string uiUxmlDir = Path.Combine(outputPath, "UI", "UXML");
            string uiUssDir = Path.Combine(outputPath, "UI", "USS");
            string settingsDir = Path.Combine(outputPath, "Settings");

            Directory.CreateDirectory(scenesDir);
            Directory.CreateDirectory(uiUxmlDir);
            Directory.CreateDirectory(uiUssDir);
            Directory.CreateDirectory(settingsDir);

            // 3. Write UI Files
            string alignClass = "menu-root--" + menuAlignment.ToLower().Replace(" ", "-");
            string uxmlContent = MainMenuTemplateRegistry.GetUXMLMainMenu()
                .Replace("class=\"menu-root\"", $"class=\"menu-root {alignClass}\"");
            
            // Rewrite the USS reference path dynamically to match our output path
            string relativeUssPath = Path.Combine(outputPath, "UI", "USS", "MainMenu.uss").Replace("\\", "/");
            uxmlContent = uxmlContent.Replace("project://database/Assets/MainMenu1/UI/USS/MainMenu.uss", "project://database/" + relativeUssPath);

            File.WriteAllText(Path.Combine(uiUxmlDir, "MainMenu.uxml"), uxmlContent);
            File.WriteAllText(Path.Combine(uiUssDir, "MainMenu.uss"), MainMenuTemplateRegistry.GetUSSMainMenu());

            AssetDatabase.Refresh();

            // 4. Create Main Menu Scene
            string scenePath = Path.Combine(scenesDir, "MainMenu.unity");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create MainCamera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            cameraObj.AddComponent<AudioListener>();

            // Create Cinemachine Brain
            var brainType = System.Type.GetType("Cinemachine.CinemachineBrain, Unity.Cinemachine")
                            ?? System.Type.GetType("Cinemachine.CinemachineBrain, Cinemachine");
            if (brainType != null)
            {
                cameraObj.AddComponent(brainType);
            }

            // Create Light
            if (is2D)
            {
#if UNITY_2022_1_OR_NEWER
                GameObject lightObj = new GameObject("Global Light 2D");
                var light2d = lightObj.AddComponent<Light2D>();
                light2d.lightType = Light2D.LightType.Global;
#endif
            }
            else
            {
                GameObject lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // Create Cinemachine Camera
            GameObject vcamObj = new GameObject("CinemachineCamera");
            var vcamType = System.Type.GetType("Cinemachine.CinemachineCamera, Unity.Cinemachine")
                           ?? System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (vcamType != null)
            {
                var vcamComponent = vcamObj.AddComponent(vcamType);
                var priorityProp = vcamType.GetProperty("Priority") ?? vcamType.GetProperty("priority");
                if (priorityProp != null)
                {
                    priorityProp.SetValue(vcamComponent, 10);
                }
                else
                {
                    var priorityField = vcamType.GetField("m_Priority", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (priorityField != null)
                    {
                        priorityField.SetValue(vcamComponent, 10);
                    }
                }
            }

            // Load UXML asset
            string relativeUxmlPath = Path.Combine(outputPath, "UI", "UXML", "MainMenu.uxml").Replace("\\", "/");
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(relativeUxmlPath);

            // Create or Load PanelSettings asset
            string panelSettingsPath = Path.Combine(outputPath, "UI", "MainMenuPanelSettings.asset").Replace("\\", "/");
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;

                string uiDir = Path.Combine(outputPath, "UI");
                if (!Directory.Exists(uiDir)) Directory.CreateDirectory(uiDir);

                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
                AssetDatabase.SaveAssets();
            }

            // Create UI Root
            GameObject uiRootObj = new GameObject("MainMenuRoot");
            var uiDoc = uiRootObj.AddComponent<UIDocument>();
            if (uxmlAsset != null)
            {
                uiDoc.visualTreeAsset = uxmlAsset;
            }
            if (panelSettings != null)
            {
                uiDoc.panelSettings = panelSettings;
            }
            var mainMenuMgr = uiRootObj.AddComponent<MainMenuManager>();
            mainMenuMgr.menuAlignment = menuAlignment;
            uiRootObj.AddComponent<PanelManager>();
            uiRootObj.AddComponent<AudioManager>();
            uiRootObj.AddComponent<SettingsManager>();
            
            if (preset == "Single Player" || preset == "Local Multiplayer" || preset == "Mobile")
            {
                uiRootObj.AddComponent<SaveSlotManager>();
                uiRootObj.AddComponent<SaveSlotUI>();
            }

            // Create SceneTransitionManager GameObject and attach component
            GameObject transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.AddComponent<SceneTransitionManager>();

            // Create EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                eventSystemObj.AddComponent(inputModuleType);
            }
            else
            {
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MainMenuGenerator] Scene saved to: {scenePath}");

            // 5. Add scene to Build Settings
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool alreadyInBuild = false;
            foreach (var s in scenes)
            {
                if (s.path.Replace("\\", "/") == scenePath.Replace("\\", "/"))
                {
                    alreadyInBuild = true;
                    break;
                }
            }
            if (!alreadyInBuild)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[MainMenuGenerator] Scene added to build settings: {scenePath}");
            }

            AssetDatabase.Refresh();
            Debug.Log("[MainMenuGenerator] Generation complete!");
        }
    }
}
