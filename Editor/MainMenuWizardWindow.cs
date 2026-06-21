using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;

namespace MCPForUnity.Editor.Helpers
{
    public class MainMenuWizardWindow : EditorWindow
    {
        [MenuItem("Tools/Main Menu Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<MainMenuWizardWindow>();
            window.titleContent = new GUIContent("Main Menu Wizard");
            window.minSize = new Vector2(500, 600);
            window.maxSize = new Vector2(500, 600);
        }

        private int _currentStep = 1;
        private string _selectedPreset = "Single Player";
        private bool _is2D = true;
        private int _saveSlotsCount = 3;
        private bool _pixelPerfect = true;
        private bool _cameraConfiner = true;
        private string _outputPath = "Assets/MainMenu1";
        private string _menuAlignment = "Middle Center";

        // Dashboard config targets
        private SceneAsset _tempSceneAsset;
        private UnityEngine.InputSystem.InputActionAsset _tempInputAsset;
        private string _lastScannedScenePath;
        private int _dashboardTab = 0;
        private bool _controlsLoaded = false;
        private List<ExposedControl> _tempKeyboardControls = new List<ExposedControl>();
        private List<ExposedControl> _tempControllerControls = new List<ExposedControl>();

        // Sound config temporary values
        private bool _soundsLoaded = false;
        private List<AudioClip> _tempMainMenuMusic = new List<AudioClip>();
        private bool _tempLoopMainMenuMusic = true;
        private float _tempMainMenuMusicVolume = 0.5f;
        private AudioClip _tempClickSFX;
        private float _tempClickSFXVolume = 0.8f;
        private AudioClip _tempToggleSFX;
        private float _tempToggleSFXVolume = 0.8f;
        private AudioClip _tempPanelChangeSFX;
        private float _tempPanelChangeSFXVolume = 0.8f;
        private AudioClip _tempNavigateSFX;
        private float _tempNavigateSFXVolume = 0.6f;

        private VisualElement _contentRoot;
        private Label _stepTitle;
        private Label _stepDescription;

        private void OnEnable()
        {
            _currentStep = EditorPrefs.GetInt("MainMenuWizard.CurrentStep", 1);
            _selectedPreset = EditorPrefs.GetString("MainMenuWizard.SelectedPreset", "Single Player");
            _is2D = EditorPrefs.GetBool("MainMenuWizard.Is2D", true);
            _saveSlotsCount = EditorPrefs.GetInt("MainMenuWizard.SaveSlotsCount", 3);
            _outputPath = EditorPrefs.GetString("MainMenuWizard.OutputPath", "Assets/MainMenu1");
            _menuAlignment = EditorPrefs.GetString("MainMenuWizard.MenuAlignment", "Middle Center");

            string scenePath = Path.Combine(_outputPath, "Scenes", "MainMenu.unity").Replace("\\", "/");
            if (File.Exists(scenePath) && _currentStep == 1)
            {
                _currentStep = 6;
                SaveState();
            }

            var root = rootVisualElement;
            root.style.backgroundColor = new Color(0.13f, 0.16f, 0.19f); // Dark background
            root.style.paddingLeft = 20;
            root.style.paddingRight = 20;
            root.style.paddingTop = 15;
            root.style.paddingBottom = 20;

            // Undo / Redo Header Row
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.FlexEnd;
            headerRow.style.marginBottom = 10;

            var undoBtn = new Button(() => {
                Undo.PerformUndo();
                DrawStep();
            }) { text = "↶ Undo" };
            undoBtn.style.marginRight = 5;
            undoBtn.style.height = 20;
            undoBtn.style.fontSize = 11;
            headerRow.Add(undoBtn);

            var redoBtn = new Button(() => {
                Undo.PerformRedo();
                DrawStep();
            }) { text = "↷ Redo" };
            redoBtn.style.height = 20;
            redoBtn.style.fontSize = 11;
            headerRow.Add(redoBtn);

            root.Add(headerRow);

            // Title Header
            var header = new Label("MAIN MENU SETUP WIZARD");
            header.style.fontSize = 24;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new Color(0f, 0.68f, 0.71f); // Teal Accent
            header.style.marginBottom = 10;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(header);

            // Subtitle Description
            _stepTitle = new Label("Step 1 — Choose Preset");
            _stepTitle.style.fontSize = 16;
            _stepTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _stepTitle.style.color = Color.white;
            _stepTitle.style.marginBottom = 5;
            root.Add(_stepTitle);

            _stepDescription = new Label("Select the target gameplay configuration for your main menu.");
            _stepDescription.style.fontSize = 12;
            _stepDescription.style.color = new Color(0.7f, 0.7f, 0.7f);
            _stepDescription.style.marginBottom = 20;
            root.Add(_stepDescription);

            // Dynamic Content Area
            _contentRoot = new VisualElement();
            _contentRoot.style.flexGrow = 1;
            _contentRoot.style.marginBottom = 20;
            _contentRoot.style.backgroundColor = new Color(0.18f, 0.22f, 0.26f);
            _contentRoot.style.paddingLeft = 15;
            _contentRoot.style.paddingRight = 15;
            _contentRoot.style.paddingTop = 15;
            _contentRoot.style.paddingBottom = 15;
            
            _contentRoot.style.borderTopLeftRadius = 8;
            _contentRoot.style.borderTopRightRadius = 8;
            _contentRoot.style.borderBottomLeftRadius = 8;
            _contentRoot.style.borderBottomRightRadius = 8;
            root.Add(_contentRoot);

            // Bottom Navigation Panel
            var navPanel = new VisualElement();
            navPanel.style.flexDirection = FlexDirection.Row;
            navPanel.style.justifyContent = Justify.SpaceBetween;

            var backBtn = new Button(OnBackClicked) { text = "Back" };
            backBtn.name = "back-btn";
            backBtn.style.width = 100;
            backBtn.style.height = 30;
            backBtn.style.backgroundColor = new Color(0.25f, 0.3f, 0.35f);
            backBtn.style.color = Color.white;
            navPanel.Add(backBtn);

            var nextBtn = new Button(OnNextClicked) { text = "Next" };
            nextBtn.name = "next-btn";
            nextBtn.style.width = 100;
            nextBtn.style.height = 30;
            nextBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            nextBtn.style.color = Color.white;
            navPanel.Add(nextBtn);

            root.Add(navPanel);

            DrawStep();
        }

        private void SaveState()
        {
            EditorPrefs.SetInt("MainMenuWizard.CurrentStep", _currentStep);
            EditorPrefs.SetString("MainMenuWizard.SelectedPreset", _selectedPreset);
            EditorPrefs.SetBool("MainMenuWizard.Is2D", _is2D);
            EditorPrefs.SetInt("MainMenuWizard.SaveSlotsCount", _saveSlotsCount);
            EditorPrefs.SetString("MainMenuWizard.OutputPath", _outputPath);
            EditorPrefs.SetString("MainMenuWizard.MenuAlignment", _menuAlignment);
        }

        private void DrawStep()
        {
            _contentRoot.Clear();

            var backBtn = rootVisualElement.Q<Button>("back-btn");
            var nextBtn = rootVisualElement.Q<Button>("next-btn");

            if (_currentStep == 6)
            {
                if (backBtn != null)
                {
                    backBtn.text = "Start Wizard";
                    backBtn.style.display = DisplayStyle.Flex;
                }
                if (nextBtn != null)
                {
                    nextBtn.style.display = DisplayStyle.None;
                }
            }
            else
            {
                if (backBtn != null)
                {
                    backBtn.text = "Back";
                    backBtn.style.display = _currentStep == 1 || _currentStep == 5 ? DisplayStyle.None : DisplayStyle.Flex;
                }
                if (nextBtn != null)
                {
                    if (_currentStep == 4) nextBtn.text = "Generate";
                    else if (_currentStep == 5) nextBtn.text = "Done";
                    else nextBtn.text = "Next";

                    nextBtn.style.display = _currentStep == 5 ? DisplayStyle.None : DisplayStyle.Flex;
                    nextBtn.SetEnabled(true);
                }
            }

            switch (_currentStep)
            {
                case 1:
                    DrawStep1();
                    break;
                case 2:
                    DrawStep2();
                    break;
                case 3:
                    DrawStep3();
                    break;
                case 4:
                    DrawStep4();
                    break;
                case 5:
                    DrawStep5();
                    break;
                case 6:
                    DrawStep6();
                    break;
            }
        }

        private void DrawStep1()
        {
            _stepTitle.text = "Step 1 — Choose Preset";
            _stepDescription.text = "Select the default template preset that fits your project type.";

            string[] presets = {
                "Single Player",
                "Local Multiplayer",
                "Online Multiplayer",
                "Mobile",
                "VR / XR",
                "Game Jam / Prototype"
            };

            foreach (var preset in presets)
            {
                var btn = new Button(() => {
                    _selectedPreset = preset;
                    SaveState();
                    DrawStep();
                }) { text = preset };

                btn.style.height = 40;
                btn.style.marginBottom = 10;
                btn.style.fontSize = 14;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;

                if (_selectedPreset == preset)
                {
                    btn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
                    btn.style.color = new Color(0.13f, 0.16f, 0.19f);
                }
                else
                {
                    btn.style.backgroundColor = new Color(0.22f, 0.27f, 0.32f);
                    btn.style.color = Color.white;
                }

                _contentRoot.Add(btn);
            }
        }

        private void DrawStep2()
        {
            _stepTitle.text = "Step 1b — Choose Dimension";
            _stepDescription.text = "Configure rendering settings and camera controllers.";

            var label = new Label($"Preset: {_selectedPreset}");
            label.style.color = new Color(0.7f, 0.7f, 0.7f);
            label.style.marginBottom = 20;
            _contentRoot.Add(label);

            var btn2D = new Button(() => {
                _is2D = true;
                SaveState();
                DrawStep();
            }) { text = "2D Dimension\nURP 2D Renderer, Cinemachine 2D" };
            btn2D.style.height = 80;
            btn2D.style.marginBottom = 15;
            btn2D.style.fontSize = 14;

            var btn3D = new Button(() => {
                _is2D = false;
                SaveState();
                DrawStep();
            }) { text = "3D Dimension\nURP Lit Renderer, Cinemachine Brain & Orbit" };
            btn3D.style.height = 80;
            btn3D.style.fontSize = 14;

            if (_is2D)
            {
                btn2D.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
                btn2D.style.color = new Color(0.13f, 0.16f, 0.19f);
                btn3D.style.backgroundColor = new Color(0.22f, 0.27f, 0.32f);
                btn3D.style.color = Color.white;
            }
            else
            {
                btn2D.style.backgroundColor = new Color(0.22f, 0.27f, 0.32f);
                btn2D.style.color = Color.white;
                btn3D.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
                btn3D.style.color = new Color(0.13f, 0.16f, 0.19f);
            }

            _contentRoot.Add(btn2D);
            _contentRoot.Add(btn3D);
        }

        private void DrawStep3()
        {
            _stepTitle.text = "Step 2 — Required Packages";
            _stepDescription.text = "Verify that all required Unity packages are available in the project.";

            var requiredPackages = GetRequiredPackages();
            var missingPackages = GetMissingPackages();

            var listLabel = new Label("Package Installation Status:");
            listLabel.style.color = Color.white;
            listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            listLabel.style.marginBottom = 10;
            _contentRoot.Add(listLabel);

            foreach (var pkg in requiredPackages)
            {
                bool isMissing = missingPackages.Contains(pkg);
                var pkgLabel = new Label();
                pkgLabel.style.fontSize = 13;
                pkgLabel.style.marginBottom = 5;

                if (isMissing)
                {
                    pkgLabel.text = $"✗ {pkg} (Not Installed)";
                    pkgLabel.style.color = new Color(0.9f, 0.6f, 0.2f);
                }
                else
                {
                    pkgLabel.text = $"✓ {pkg} (Installation satisfies)";
                    pkgLabel.style.color = Color.green;
                }

                _contentRoot.Add(pkgLabel);
            }

            if (missingPackages.Count == 0)
            {
                var successLabel = new Label("✓ All required packages are already installed!");
                successLabel.style.color = Color.green;
                successLabel.style.fontSize = 14;
                successLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                successLabel.style.marginTop = 20;
                successLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _contentRoot.Add(successLabel);
            }
            else
            {
                var installBtn = new Button(() => {
                    InstallMissingPackages(missingPackages);
                }) { text = "Install Missing Packages" };
                installBtn.style.height = 40;
                installBtn.style.marginTop = 20;
                installBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
                installBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
                _contentRoot.Add(installBtn);

                var nextBtn = rootVisualElement.Q<Button>("next-btn");
                if (nextBtn != null) nextBtn.SetEnabled(false);
            }
        }

        private void DrawStep4()
        {
            _stepTitle.text = "Step 3 — Tweak & Review Options";
            _stepDescription.text = "Adjust final settings and output directory before generating.";

            if (_selectedPreset == "Single Player" || _selectedPreset == "Local Multiplayer" || _selectedPreset == "Mobile")
            {
                var saveSlotsContainer = new VisualElement();
                saveSlotsContainer.style.flexDirection = FlexDirection.Row;
                saveSlotsContainer.style.justifyContent = Justify.SpaceBetween;
                saveSlotsContainer.style.alignItems = Align.Center;
                saveSlotsContainer.style.marginBottom = 15;

                saveSlotsContainer.Add(new Label("Number of Save Slots:") { style = { color = Color.white, fontSize = 13 } });

                var slotsField = new DropdownField(new List<string> { "1", "3", "5" }, _saveSlotsCount.ToString());
                slotsField.style.width = 120;
                slotsField.RegisterValueChangedCallback(evt => {
                    _saveSlotsCount = int.Parse(evt.newValue);
                    SaveState();
                });
                saveSlotsContainer.Add(slotsField);
                _contentRoot.Add(saveSlotsContainer);
            }

            var alignmentContainer = new VisualElement();
            alignmentContainer.style.flexDirection = FlexDirection.Row;
            alignmentContainer.style.justifyContent = Justify.SpaceBetween;
            alignmentContainer.style.alignItems = Align.Center;
            alignmentContainer.style.marginBottom = 15;

            alignmentContainer.Add(new Label("Menu Alignment:") { style = { color = Color.white, fontSize = 13 } });

            var alignField = new DropdownField(new List<string> { "Left", "Right", "Middle Center", "Top", "Bottom" }, _menuAlignment);
            alignField.style.width = 120;
            alignField.RegisterValueChangedCallback(evt => {
                _menuAlignment = evt.newValue;
                SaveState();
            });
            alignmentContainer.Add(alignField);
            _contentRoot.Add(alignmentContainer);

            var folderContainer = new VisualElement();
            folderContainer.style.flexDirection = FlexDirection.Row;
            folderContainer.style.justifyContent = Justify.SpaceBetween;
            folderContainer.style.alignItems = Align.Center;
            folderContainer.style.marginBottom = 20;

            folderContainer.Add(new Label("Output Folder:") { style = { color = Color.white, fontSize = 13 } });

            var pathTxt = new TextField() { value = _outputPath };
            pathTxt.style.flexGrow = 1;
            pathTxt.style.marginLeft = 10;
            pathTxt.style.marginRight = 10;
            pathTxt.RegisterValueChangedCallback(evt => {
                _outputPath = evt.newValue;
                SaveState();
            });
            folderContainer.Add(pathTxt);

            var browseBtn = new Button(() => {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Directory", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.Contains(Application.dataPath))
                    {
                        _outputPath = "Assets" + selectedPath.Replace(Application.dataPath, "");
                        pathTxt.value = _outputPath;
                        SaveState();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Folder", "Output folder must be inside Assets/.", "OK");
                    }
                }
            }) { text = "Browse" };
            folderContainer.Add(browseBtn);
            _contentRoot.Add(folderContainer);

            var summaryTitle = new Label("SUMMARY");
            summaryTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            summaryTitle.style.color = new Color(0f, 0.68f, 0.71f);
            summaryTitle.style.marginTop = 15;
            summaryTitle.style.marginBottom = 8;
            _contentRoot.Add(summaryTitle);

            _contentRoot.Add(new Label($"• Selected Preset: {_selectedPreset}") { style = { color = Color.white, marginBottom = 4 } });
            _contentRoot.Add(new Label($"• Render Dimension: {(_is2D ? "2D (URP 2D)" : "3D (URP Lit)")}") { style = { color = Color.white, marginBottom = 4 } });
            _contentRoot.Add(new Label($"• Target Directory: {_outputPath}") { style = { color = Color.white, marginBottom = 4 } });
        }

        private void DrawStep5()
        {
            _stepTitle.text = "Step 4 — Generation Complete!";
            _stepDescription.text = "All assets were generated successfully and added to the build settings.";

            var successIcon = new Label("✓ SUCCESS");
            successIcon.style.color = Color.green;
            successIcon.style.fontSize = 20;
            successIcon.style.unityFontStyleAndWeight = FontStyle.Bold;
            successIcon.style.marginTop = 30;
            successIcon.style.unityTextAlign = TextAnchor.MiddleCenter;
            _contentRoot.Add(successIcon);

            var infoLabel = new Label($"Scene created: {_outputPath}/Scenes/MainMenu.unity");
            infoLabel.style.color = Color.white;
            infoLabel.style.marginTop = 20;
            infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _contentRoot.Add(infoLabel);

            var openBtn = new Button(() => {
                string scenePath = Path.Combine(_outputPath, "Scenes", "MainMenu.unity");
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                _currentStep = 6;
                SaveState();
                DrawStep();
            }) { text = "Open MainMenu Scene & Manage" };
            openBtn.style.height = 40;
            openBtn.style.marginTop = 40;
            openBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            openBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
            _contentRoot.Add(openBtn);
        }

        private void DrawStep6()
        {
            _stepTitle.text = "Main Menu Dashboard";
            _stepDescription.text = "Scan and configure your active MainMenu scene features.";

            string scenePath = Path.Combine(_outputPath, "Scenes", "MainMenu.unity").Replace("\\", "/");
            string activeScenePath = EditorSceneManager.GetActiveScene().path.Replace("\\", "/");
            bool isSceneOpen = activeScenePath == scenePath;

            if (activeScenePath != _lastScannedScenePath)
            {
                _lastScannedScenePath = activeScenePath;
                _tempSceneAsset = null;
                _tempInputAsset = null;
                _controlsLoaded = false;
                _soundsLoaded = false;
            }

            if (isSceneOpen)
            {
                var managerObj = GameObject.Find("MainMenuRoot");
                if (managerObj != null)
                {
                    var mainMenuManager = managerObj.GetComponent<MainMenuManager>();
                    if (mainMenuManager != null)
                    {
                        string sceneName = mainMenuManager.sceneToLoad;
                        if (!string.IsNullOrEmpty(sceneName) && _tempSceneAsset == null)
                        {
                            var guids = AssetDatabase.FindAssets("t:Scene " + sceneName);
                            if (guids.Length > 0)
                            {
                                _tempSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                            }
                        }
                    }
                }

                var eventSystem = GameObject.Find("EventSystem") ?? GameObject.Find("UIEventSystem");
                if (eventSystem != null && _tempInputAsset == null)
                {
                    var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    if (inputModule != null)
                    {
                        _tempInputAsset = inputModule.actionsAsset;
                    }
                }
            }

            if (!_controlsLoaded && isSceneOpen)
            {
                var managerObj = GameObject.Find("MainMenuRoot");
                if (managerObj != null)
                {
                    var settings = managerObj.GetComponent<SettingsManager>();
                    if (settings != null)
                    {
                        _tempKeyboardControls.Clear();
                        foreach (var ctrl in settings.exposedKeyboardControls)
                        {
                            _tempKeyboardControls.Add(new ExposedControl { displayName = ctrl.displayName, actionName = ctrl.actionName });
                        }

                        _tempControllerControls.Clear();
                        foreach (var ctrl in settings.exposedControllerControls)
                        {
                            _tempControllerControls.Add(new ExposedControl { displayName = ctrl.displayName, actionName = ctrl.actionName });
                        }
                        _controlsLoaded = true;
                    }
                }
            }

            if (!_soundsLoaded && isSceneOpen)
            {
                var managerObj = GameObject.Find("MainMenuRoot");
                if (managerObj != null)
                {
                    var audioManager = managerObj.GetComponent<AudioManager>();
                    if (audioManager != null)
                    {
                        _tempMainMenuMusic.Clear();
                        if (audioManager.mainMenuMusic != null)
                        {
                            foreach (var clip in audioManager.mainMenuMusic)
                            {
                                _tempMainMenuMusic.Add(clip);
                            }
                        }
                        _tempLoopMainMenuMusic = audioManager.loopMainMenuMusic;
                        _tempMainMenuMusicVolume = audioManager.mainMenuMusicVolume;

                        _tempClickSFX = audioManager.clickSFX;
                        _tempClickSFXVolume = audioManager.clickSFXVolume;

                        _tempToggleSFX = audioManager.toggleSFX;
                        _tempToggleSFXVolume = audioManager.toggleSFXVolume;

                        _tempPanelChangeSFX = audioManager.panelChangeSFX;
                        _tempPanelChangeSFXVolume = audioManager.panelChangeSFXVolume;

                        _tempNavigateSFX = audioManager.navigateSFX;
                        _tempNavigateSFXVolume = audioManager.navigateSFXVolume;
                    }
                    _soundsLoaded = true;
                }
            }

            // Tab Navigation Row
            var tabSelector = new VisualElement();
            tabSelector.style.flexDirection = FlexDirection.Row;
            tabSelector.style.marginBottom = 15;

            var overviewTabBtn = new Button(() => {
                _dashboardTab = 0;
                DrawStep();
            }) { text = "OVERVIEW" };
            var controlsTabBtn = new Button(() => {
                _dashboardTab = 1;
                DrawStep();
            }) { text = "CONTROLS" };
            var soundTabBtn = new Button(() => {
                _dashboardTab = 2;
                DrawStep();
            }) { text = "SOUND" };

            overviewTabBtn.style.flexGrow = 1;
            overviewTabBtn.style.height = 25;
            controlsTabBtn.style.flexGrow = 1;
            controlsTabBtn.style.height = 25;
            soundTabBtn.style.flexGrow = 1;
            soundTabBtn.style.height = 25;

            overviewTabBtn.style.backgroundColor = _dashboardTab == 0 ? new Color(0f, 0.68f, 0.71f) : new Color(0.22f, 0.27f, 0.32f);
            overviewTabBtn.style.color = _dashboardTab == 0 ? new Color(0.13f, 0.16f, 0.19f) : Color.white;

            controlsTabBtn.style.backgroundColor = _dashboardTab == 1 ? new Color(0f, 0.68f, 0.71f) : new Color(0.22f, 0.27f, 0.32f);
            controlsTabBtn.style.color = _dashboardTab == 1 ? new Color(0.13f, 0.16f, 0.19f) : Color.white;

            soundTabBtn.style.backgroundColor = _dashboardTab == 2 ? new Color(0f, 0.68f, 0.71f) : new Color(0.22f, 0.27f, 0.32f);
            soundTabBtn.style.color = _dashboardTab == 2 ? new Color(0.13f, 0.16f, 0.19f) : Color.white;

            tabSelector.Add(overviewTabBtn);
            tabSelector.Add(controlsTabBtn);
            tabSelector.Add(soundTabBtn);
            _contentRoot.Add(tabSelector);

            if (_dashboardTab == 0)
            {
                DrawOverviewTab(isSceneOpen, scenePath);
            }
            else if (_dashboardTab == 1)
            {
                DrawControlsConfigTab(isSceneOpen, scenePath);
            }
            else
            {
                DrawSoundConfigTab(isSceneOpen, scenePath);
            }
        }

        private void DrawOverviewTab(bool isSceneOpen, string scenePath)
        {
            var sceneControls = new VisualElement();
            sceneControls.style.flexDirection = FlexDirection.Row;
            sceneControls.style.justifyContent = Justify.SpaceBetween;
            sceneControls.style.alignItems = Align.Center;
            sceneControls.style.marginBottom = 12;

            var statusLabel = new Label(isSceneOpen ? "✓ Scene Status: Open & Active" : "○ Scene Status: Closed");
            statusLabel.style.color = isSceneOpen ? Color.green : Color.white;
            statusLabel.style.fontSize = 13;
            sceneControls.Add(statusLabel);

            var openSceneBtn = new Button(() => {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                DrawStep();
            }) { text = "Open Scene" };
            openSceneBtn.style.width = 120;
            openSceneBtn.style.height = 25;
            openSceneBtn.style.backgroundColor = new Color(0.25f, 0.3f, 0.35f);
            openSceneBtn.style.color = Color.white;
            sceneControls.Add(openSceneBtn);
            _contentRoot.Add(sceneControls);

            var playControls = new VisualElement();
            playControls.style.flexDirection = FlexDirection.Row;
            playControls.style.justifyContent = Justify.SpaceBetween;
            playControls.style.alignItems = Align.Center;
            playControls.style.marginBottom = 15;

            playControls.Add(new Label("Play Game Mode:") { style = { color = Color.white, fontSize = 13 } });

            var playBtn = new Button(() => {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }) { text = EditorApplication.isPlaying ? "Stop Game" : "Play Scene" };
            playBtn.style.width = 120;
            playBtn.style.height = 25;
            playBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            playBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
            playControls.Add(playBtn);
            _contentRoot.Add(playControls);

            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.backgroundColor = new Color(0.3f, 0.35f, 0.4f);
            divider.style.marginBottom = 12;
            _contentRoot.Add(divider);

            var featuresTitle = new Label("SCENE FEATURE ANALYSIS");
            featuresTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            featuresTitle.style.color = new Color(0f, 0.68f, 0.71f);
            featuresTitle.style.marginBottom = 8;
            _contentRoot.Add(featuresTitle);

            if (isSceneOpen)
            {
                var managerObj = GameObject.Find("MainMenuRoot");
                bool hasManager = managerObj != null;
                bool hasSaveSystem = managerObj != null && managerObj.GetComponent("SaveSlotManager") != null;
                bool hasPanelManager = managerObj != null && managerObj.GetComponent("PanelManager") != null;
                bool hasSettings = managerObj != null && managerObj.GetComponent("SettingsManager") != null;

                bool hasCinemachine = GameObject.Find("CinemachineCamera") != null;

                _contentRoot.Add(new Label($"• Root Controller (MainMenuRoot): {(hasManager ? "✓ Detected" : "✗ Missing")}") { style = { color = hasManager ? Color.green : Color.red, marginBottom = 4 } });
                _contentRoot.Add(new Label($"• UI Panel Stack (PanelManager): {(hasPanelManager ? "✓ Active" : "✗ Missing")}") { style = { color = hasPanelManager ? Color.green : Color.red, marginBottom = 4 } });
                _contentRoot.Add(new Label($"• Settings Controller: {(hasSettings ? "✓ Active" : "✗ Missing")}") { style = { color = hasSettings ? Color.green : Color.red, marginBottom = 4 } });
                _contentRoot.Add(new Label($"• Save Slot System: {(hasSaveSystem ? "✓ Enabled" : "○ Disabled/Not Configured")}") { style = { color = hasSaveSystem ? Color.green : Color.white, marginBottom = 4 } });
                _contentRoot.Add(new Label($"• Cinemachine Camera Rig: {(hasCinemachine ? "✓ Connected" : "○ Static Camera Only")}") { style = { color = hasCinemachine ? Color.green : Color.white, marginBottom = 15 } });
            }
            else
            {
                var scanWarning = new Label("Open the MainMenu scene to run live feature analysis.");
                scanWarning.style.color = new Color(0.7f, 0.7f, 0.7f);
                scanWarning.style.unityFontStyleAndWeight = FontStyle.Italic;
                scanWarning.style.marginBottom = 15;
                _contentRoot.Add(scanWarning);
            }

            var divider2 = new VisualElement();
            divider2.style.height = 1;
            divider2.style.backgroundColor = new Color(0.3f, 0.35f, 0.4f);
            divider2.style.marginBottom = 12;
            _contentRoot.Add(divider2);

            var configTitle = new Label("CONFIGURATION (NON-REALTIME)");
            configTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            configTitle.style.color = new Color(0f, 0.68f, 0.71f);
            configTitle.style.marginBottom = 8;
            _contentRoot.Add(configTitle);

            var scenePicker = new ObjectField("Scene to Load on Play");
            scenePicker.objectType = typeof(SceneAsset);
            scenePicker.value = _tempSceneAsset;
            scenePicker.RegisterValueChangedCallback(evt => {
                _tempSceneAsset = evt.newValue as SceneAsset;
            });
            _contentRoot.Add(scenePicker);

            var inputField = new ObjectField("Custom Input Actions");
            inputField.objectType = typeof(UnityEngine.InputSystem.InputActionAsset);
            inputField.value = _tempInputAsset;
            inputField.RegisterValueChangedCallback(evt => {
                _tempInputAsset = evt.newValue as UnityEngine.InputSystem.InputActionAsset;
            });
            _contentRoot.Add(inputField);

            var alignmentField = new DropdownField("Menu Alignment", new List<string> { "Left", "Right", "Middle Center", "Top", "Bottom" }, _menuAlignment);
            alignmentField.RegisterValueChangedCallback(evt => {
                _menuAlignment = evt.newValue;
                SaveState();
            });
            _contentRoot.Add(alignmentField);

            var saveBtn = new Button(ApplyAndSaveChanges) { text = "Save Changes" };
            saveBtn.style.height = 30;
            saveBtn.style.marginTop = 15;
            saveBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            saveBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
            _contentRoot.Add(saveBtn);
        }

        private void DrawControlsConfigTab(bool isSceneOpen, string scenePath)
        {
            if (!isSceneOpen)
            {
                var scanWarning = new Label("Open the MainMenu scene to configure controls.");
                scanWarning.style.color = new Color(0.7f, 0.7f, 0.7f);
                scanWarning.style.unityFontStyleAndWeight = FontStyle.Italic;
                scanWarning.style.marginBottom = 15;
                _contentRoot.Add(scanWarning);
                return;
            }

            if (_tempInputAsset == null)
            {
                var inputWarning = new Label("Please link an Input Actions asset in the OVERVIEW tab first.");
                inputWarning.style.color = Color.red;
                inputWarning.style.unityFontStyleAndWeight = FontStyle.Bold;
                inputWarning.style.marginBottom = 15;
                _contentRoot.Add(inputWarning);
                return;
            }

            var actionNames = new List<string>();
            foreach (var map in _tempInputAsset.actionMaps)
            {
                foreach (var act in map.actions)
                {
                    actionNames.Add(map.name + "/" + act.name);
                }
            }

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.maxHeight = 220;

            var kbHeader = new Label("KEYBOARD CONTROLS");
            kbHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            kbHeader.style.color = new Color(0f, 0.68f, 0.71f);
            kbHeader.style.marginTop = 5;
            kbHeader.style.marginBottom = 8;
            scroll.Add(kbHeader);

            for (int i = 0; i < _tempKeyboardControls.Count; i++)
            {
                var ctrl = _tempKeyboardControls[i];
                var row = CreateEditorControlRow(ctrl, actionNames, true);
                scroll.Add(row);
            }

            var addKbBtn = new Button(() => {
                string defaultAction = actionNames.Count > 0 ? actionNames[0] : "";
                _tempKeyboardControls.Add(new ExposedControl { displayName = "New Keyboard Action", actionName = defaultAction });
                DrawStep();
            }) { text = "+ Add Keyboard Control" };
            addKbBtn.style.height = 20;
            addKbBtn.style.fontSize = 11;
            addKbBtn.style.marginBottom = 15;
            scroll.Add(addKbBtn);

            var ctrlHeader = new Label("CONTROLLER CONTROLS");
            ctrlHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            ctrlHeader.style.color = new Color(0f, 0.68f, 0.71f);
            ctrlHeader.style.marginTop = 10;
            ctrlHeader.style.marginBottom = 8;
            scroll.Add(ctrlHeader);

            for (int i = 0; i < _tempControllerControls.Count; i++)
            {
                var ctrl = _tempControllerControls[i];
                var row = CreateEditorControlRow(ctrl, actionNames, false);
                scroll.Add(row);
            }

            var addCtrlBtn = new Button(() => {
                string defaultAction = actionNames.Count > 0 ? actionNames[0] : "";
                _tempControllerControls.Add(new ExposedControl { displayName = "New Controller Action", actionName = defaultAction });
                DrawStep();
            }) { text = "+ Add Controller Control" };
            addCtrlBtn.style.height = 20;
            addCtrlBtn.style.fontSize = 11;
            addCtrlBtn.style.marginBottom = 15;
            scroll.Add(addCtrlBtn);

            _contentRoot.Add(scroll);

            var saveBtn = new Button(ApplyAndSaveChanges) { text = "Save Changes" };
            saveBtn.style.height = 30;
            saveBtn.style.marginTop = 15;
            saveBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            saveBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
        }

        private void DrawSoundConfigTab(bool isSceneOpen, string scenePath)
        {
            if (!isSceneOpen)
            {
                var scanWarning = new Label("Open the MainMenu scene to configure sound.");
                scanWarning.style.color = new Color(0.7f, 0.7f, 0.7f);
                scanWarning.style.unityFontStyleAndWeight = FontStyle.Italic;
                scanWarning.style.marginBottom = 15;
                _contentRoot.Add(scanWarning);
                return;
            }

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.maxHeight = 350;

            // Playlist Header
            var musicHeader = new Label("BACKGROUND MUSIC PLAYLIST");
            musicHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            musicHeader.style.color = new Color(0f, 0.68f, 0.71f);
            musicHeader.style.marginTop = 5;
            musicHeader.style.marginBottom = 8;
            scroll.Add(musicHeader);

            // Render background music items
            for (int i = 0; i < _tempMainMenuMusic.Count; i++)
            {
                int index = i;
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 6;
                row.style.alignItems = Align.Center;

                var trackLabel = new Label($"Track {index + 1}");
                trackLabel.style.width = 60;
                trackLabel.style.color = Color.white;
                row.Add(trackLabel);

                var musicField = new ObjectField();
                musicField.objectType = typeof(AudioClip);
                musicField.value = _tempMainMenuMusic[index];
                musicField.style.flexGrow = 1;
                musicField.RegisterValueChangedCallback(evt => {
                    _tempMainMenuMusic[index] = evt.newValue as AudioClip;
                });
                row.Add(musicField);

                var removeBtn = new Button(() => {
                    _tempMainMenuMusic.RemoveAt(index);
                    DrawStep();
                }) { text = "X" };
                removeBtn.style.width = 20;
                removeBtn.style.height = 20;
                removeBtn.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
                removeBtn.style.color = Color.white;
                removeBtn.style.marginLeft = 5;
                row.Add(removeBtn);

                scroll.Add(row);
            }

            var addTrackBtn = new Button(() => {
                _tempMainMenuMusic.Add(null);
                DrawStep();
            }) { text = "+ Add Music Track" };
            addTrackBtn.style.height = 20;
            addTrackBtn.style.fontSize = 11;
            addTrackBtn.style.marginBottom = 10;
            scroll.Add(addTrackBtn);

            // Playlist settings
            var loopToggle = new Toggle("Loop Playlist");
            loopToggle.value = _tempLoopMainMenuMusic;
            loopToggle.style.color = Color.white;
            loopToggle.style.marginBottom = 5;
            loopToggle.RegisterValueChangedCallback(evt => {
                _tempLoopMainMenuMusic = evt.newValue;
            });
            scroll.Add(loopToggle);

            var musicVolSlider = new Slider("Music Volume", 0f, 1f);
            musicVolSlider.value = _tempMainMenuMusicVolume;
            musicVolSlider.style.color = Color.white;
            musicVolSlider.style.marginBottom = 15;
            musicVolSlider.showInputField = true;
            musicVolSlider.RegisterValueChangedCallback(evt => {
                _tempMainMenuMusicVolume = evt.newValue;
            });
            scroll.Add(musicVolSlider);

            // SFX Header
            var sfxHeader = new Label("UI SFX CONFIGURATION");
            sfxHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            sfxHeader.style.color = new Color(0f, 0.68f, 0.71f);
            sfxHeader.style.marginTop = 10;
            sfxHeader.style.marginBottom = 8;
            scroll.Add(sfxHeader);

            // Render SFX rows
            scroll.Add(CreateSFXRow("Click SFX", _tempClickSFX, _tempClickSFXVolume, (clip) => _tempClickSFX = clip, (vol) => _tempClickSFXVolume = vol));
            scroll.Add(CreateSFXRow("Toggle SFX", _tempToggleSFX, _tempToggleSFXVolume, (clip) => _tempToggleSFX = clip, (vol) => _tempToggleSFXVolume = vol));
            scroll.Add(CreateSFXRow("Panel SFX", _tempPanelChangeSFX, _tempPanelChangeSFXVolume, (clip) => _tempPanelChangeSFX = clip, (vol) => _tempPanelChangeSFXVolume = vol));
            scroll.Add(CreateSFXRow("Navigate SFX", _tempNavigateSFX, _tempNavigateSFXVolume, (clip) => _tempNavigateSFX = clip, (vol) => _tempNavigateSFXVolume = vol));

            _contentRoot.Add(scroll);

            var saveBtn = new Button(ApplyAndSaveChanges) { text = "Save Changes" };
            saveBtn.style.height = 30;
            saveBtn.style.marginTop = 15;
            saveBtn.style.backgroundColor = new Color(0f, 0.68f, 0.71f);
            saveBtn.style.color = new Color(0.13f, 0.16f, 0.19f);
            _contentRoot.Add(saveBtn);
        }

        private VisualElement CreateSFXRow(string labelText, AudioClip clip, float volume, Action<AudioClip> onClipChanged, Action<float> onVolumeChanged)
        {
            var container = new VisualElement();
            container.style.marginBottom = 10;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.style.paddingTop = 5;
            container.style.paddingBottom = 5;
            container.style.backgroundColor = new Color(0.22f, 0.27f, 0.32f);
            container.style.borderTopLeftRadius = 4;
            container.style.borderTopRightRadius = 4;
            container.style.borderBottomLeftRadius = 4;
            container.style.borderBottomRightRadius = 4;

            var row1 = new VisualElement();
            row1.style.flexDirection = FlexDirection.Row;
            row1.style.alignItems = Align.Center;
            row1.style.marginBottom = 4;

            var lbl = new Label(labelText);
            lbl.style.width = 90;
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            row1.Add(lbl);

            var field = new ObjectField();
            field.objectType = typeof(AudioClip);
            field.value = clip;
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(evt => {
                onClipChanged(evt.newValue as AudioClip);
            });
            row1.Add(field);
            container.Add(row1);

            var slider = new Slider("Volume", 0f, 1f);
            slider.value = volume;
            slider.style.color = Color.white;
            slider.showInputField = true;
            slider.RegisterValueChangedCallback(evt => {
                onVolumeChanged(evt.newValue);
            });
            container.Add(slider);

            return container;
        }

        private VisualElement CreateEditorControlRow(ExposedControl ctrl, List<string> actionNames, bool isKeyboard)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 6;
            row.style.alignItems = Align.Center;

            var nameField = new TextField();
            nameField.value = ctrl.displayName;
            nameField.style.width = 110;
            nameField.RegisterValueChangedCallback(evt => {
                ctrl.displayName = evt.newValue;
            });
            row.Add(nameField);

            var actionDropdown = new DropdownField(actionNames, ctrl.actionName);
            actionDropdown.style.width = 130;
            actionDropdown.style.marginLeft = 5;
            actionDropdown.RegisterValueChangedCallback(evt => {
                ctrl.actionName = evt.newValue;
                DrawStep();
            });
            row.Add(actionDropdown);

            var action = _tempInputAsset.FindAction(ctrl.actionName);
            if (action != null)
            {
                var entries = GetBindableEntries(action, isKeyboard);
                var bindingContainer = new VisualElement();
                bindingContainer.style.flexDirection = FlexDirection.Column;
                bindingContainer.style.flexGrow = 1;
                bindingContainer.style.marginLeft = 5;

                foreach (var entry in entries)
                {
                    if (entry.partName != null && entry.primaryIndex == -1 && entry.secondaryIndex == -1)
                    {
                        continue;
                    }

                    var entryRow = new VisualElement();
                    entryRow.style.flexDirection = FlexDirection.Row;
                    entryRow.style.alignItems = Align.Center;
                    entryRow.style.marginBottom = 2;

                    if (entry.partName != null)
                    {
                        var partLabel = new Label(entry.partName.ToUpper());
                        partLabel.style.width = 40;
                        partLabel.style.fontSize = 9;
                        partLabel.style.color = Color.gray;
                        entryRow.Add(partLabel);
                    }

                    // Primary Button
                    var btnPrimary = new Button();
                    btnPrimary.style.width = 50;
                    btnPrimary.style.fontSize = 9;
                    if (entry.primaryIndex >= 0)
                    {
                        btnPrimary.text = action.GetBindingDisplayString(entry.primaryIndex);
                        int idx = entry.primaryIndex;
                        var act = action;
                        btnPrimary.clicked += () => StartEditorRebinding(act, idx);
                    }
                    else
                    {
                        btnPrimary.text = "Not Assigned";
                        btnPrimary.SetEnabled(false);
                    }
                    entryRow.Add(btnPrimary);

                    // Secondary Button
                    var btnSecondary = new Button();
                    btnSecondary.style.width = 50;
                    btnSecondary.style.fontSize = 9;
                    btnSecondary.style.marginLeft = 3;
                    if (entry.secondaryIndex >= 0)
                    {
                        btnSecondary.text = action.GetBindingDisplayString(entry.secondaryIndex);
                        int idx = entry.secondaryIndex;
                        var act = action;
                        btnSecondary.clicked += () => StartEditorRebinding(act, idx);
                    }
                    else
                    {
                        btnSecondary.text = "Not Assigned";
                        btnSecondary.SetEnabled(false);
                    }
                    entryRow.Add(btnSecondary);

                    bindingContainer.Add(entryRow);
                }
                row.Add(bindingContainer);
            }
            else
            {
                var missingLabel = new Label("Action Not Found");
                missingLabel.style.color = Color.red;
                missingLabel.style.fontSize = 10;
                missingLabel.style.flexGrow = 1;
                missingLabel.style.marginLeft = 5;
                row.Add(missingLabel);
            }

            var removeBtn = new Button(() => {
                if (isKeyboard) _tempKeyboardControls.Remove(ctrl);
                else _tempControllerControls.Remove(ctrl);
                DrawStep();
            }) { text = "X" };
            removeBtn.style.width = 20;
            removeBtn.style.height = 20;
            removeBtn.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
            removeBtn.style.color = Color.white;
            removeBtn.style.marginLeft = 5;
            row.Add(removeBtn);

            return row;
        }

        private void StartEditorRebinding(UnityEngine.InputSystem.InputAction action, int bindingIndex)
        {
            if (action == null) return;

            var map = action.actionMap;
            bool wasEnabled = map.enabled;
            if (wasEnabled) map.Disable();

            var root = rootVisualElement;
            var overlay = new VisualElement();
            overlay.name = "editor-rebind-overlay";
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            overlay.style.justifyContent = Justify.Center;
            overlay.style.alignItems = Align.Center;

            var title = new Label("REBINDING KEY");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new Color(0f, 0.68f, 0.71f);
            title.style.marginBottom = 10;
            overlay.Add(title);

            var desc = new Label("Press any key or button on your device...");
            desc.style.fontSize = 14;
            desc.style.color = Color.white;
            overlay.Add(desc);

            root.Add(overlay);

            var operation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f);

            if (action.bindings[bindingIndex].isPartOfComposite || action.type == UnityEngine.InputSystem.InputActionType.Button)
            {
                operation.WithExpectedControlType("Button");
            }

            operation.WithBindingGroup(null);

            operation.OnComplete(op => {
                root.Remove(overlay);
                if (wasEnabled) map.Enable();

                EditorUtility.SetDirty(action.actionMap.asset);
                AssetDatabase.SaveAssets();

                op.Dispose();
                DrawStep();
            })
            .OnCancel(op => {
                root.Remove(overlay);
                if (wasEnabled) map.Enable();
                op.Dispose();
                DrawStep();
            });

            operation.Start();
        }

        private bool IsKeyboardMouseBinding(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>") || path.StartsWith("<Pointer>") || path.StartsWith("<Pen>");
        }

        private bool IsControllerBinding(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith("<Gamepad>") || path.StartsWith("<Joystick>") || path.StartsWith("<XRController>");
        }

        private class BindableEntry
        {
            public UnityEngine.InputSystem.InputAction action;
            public string partName;
            public int primaryIndex = -1;
            public int secondaryIndex = -1;
        }

        private List<BindableEntry> GetBindableEntries(UnityEngine.InputSystem.InputAction action, bool isKeyboard)
        {
            var entries = new List<BindableEntry>();
            if (action == null) return entries;

            bool hasComposites = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite)
                {
                    hasComposites = true;
                    break;
                }
            }

            if (!hasComposites)
            {
                var entry = new BindableEntry();
                entry.action = action;
                entry.partName = null;

                var matchingIndices = new List<int>();
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || binding.isPartOfComposite) continue;

                    bool match = isKeyboard ? IsKeyboardMouseBinding(binding.path) : IsControllerBinding(binding.path);
                    if (match)
                    {
                        matchingIndices.Add(i);
                    }
                }

                if (matchingIndices.Count > 0) entry.primaryIndex = matchingIndices[0];
                if (matchingIndices.Count > 1) entry.secondaryIndex = matchingIndices[1];
                entries.Add(entry);
            }
            else
            {
                var partNames = new List<string>();
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isPartOfComposite && !string.IsNullOrEmpty(binding.name))
                    {
                        if (!partNames.Contains(binding.name))
                        {
                            partNames.Add(binding.name);
                        }
                    }
                }

                foreach (var part in partNames)
                {
                    var entry = new BindableEntry();
                    entry.action = action;
                    entry.partName = part;

                    var matchingIndices = new List<int>();
                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        var binding = action.bindings[i];
                        if (binding.isPartOfComposite && binding.name == part)
                        {
                            bool match = isKeyboard ? IsKeyboardMouseBinding(binding.path) : IsControllerBinding(binding.path);
                            if (match)
                            {
                                matchingIndices.Add(i);
                            }
                        }
                    }

                    if (matchingIndices.Count > 0) entry.primaryIndex = matchingIndices[0];
                    if (matchingIndices.Count > 1) entry.secondaryIndex = matchingIndices[1];
                    entries.Add(entry);
                }

                var nonCompositeMatchingIndices = new List<int>();
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || binding.isPartOfComposite) continue;

                    bool match = isKeyboard ? IsKeyboardMouseBinding(binding.path) : IsControllerBinding(binding.path);
                    if (match)
                    {
                        nonCompositeMatchingIndices.Add(i);
                    }
                }

                if (nonCompositeMatchingIndices.Count > 0)
                {
                    var entry = new BindableEntry();
                    entry.action = action;
                    entry.partName = null;
                    entry.primaryIndex = nonCompositeMatchingIndices[0];
                    if (nonCompositeMatchingIndices.Count > 1) entry.secondaryIndex = nonCompositeMatchingIndices[1];
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private void ApplyAndSaveChanges()
        {
            string scenePath = Path.Combine(_outputPath, "Scenes", "MainMenu.unity").Replace("\\", "/");
            bool isSceneOpen = EditorSceneManager.GetActiveScene().path.Replace("\\", "/") == scenePath;
            if (!isSceneOpen)
            {
                EditorUtility.DisplayDialog("Save Failed", "Please open the MainMenu scene before applying changes.", "OK");
                return;
            }

            var managerObj = GameObject.Find("MainMenuRoot");
            if (managerObj != null)
            {
                var mainMenuManager = managerObj.GetComponent<MainMenuManager>();
                if (mainMenuManager != null)
                {
                    Undo.RecordObject(mainMenuManager, "Update Main Menu Scene Configuration");
                    if (_tempSceneAsset != null)
                    {
                        mainMenuManager.sceneToLoad = _tempSceneAsset.name;
                    }
                    mainMenuManager.menuAlignment = _menuAlignment;
                }

                var uiDoc = managerObj.GetComponent<UIDocument>();
                if (uiDoc != null && uiDoc.rootVisualElement != null)
                {
                    var rootVE = uiDoc.rootVisualElement.Q<VisualElement>("root");
                    if (rootVE != null)
                    {
                        rootVE.RemoveFromClassList("menu-root--left");
                        rootVE.RemoveFromClassList("menu-root--right");
                        rootVE.RemoveFromClassList("menu-root--middle-center");
                        rootVE.RemoveFromClassList("menu-root--top");
                        rootVE.RemoveFromClassList("menu-root--bottom");
                        rootVE.AddToClassList("menu-root--" + _menuAlignment.ToLower().Replace(" ", "-"));
                    }
                }

                var settings = managerObj.GetComponent<SettingsManager>();
                if (settings != null)
                {
                    Undo.RecordObject(settings, "Update Exposed Controls");
                    
                    settings.exposedKeyboardControls.Clear();
                    foreach (var ctrl in _tempKeyboardControls)
                    {
                        settings.exposedKeyboardControls.Add(new ExposedControl { displayName = ctrl.displayName, actionName = ctrl.actionName });
                    }

                    settings.exposedControllerControls.Clear();
                    foreach (var ctrl in _tempControllerControls)
                    {
                        settings.exposedControllerControls.Add(new ExposedControl { displayName = ctrl.displayName, actionName = ctrl.actionName });
                    }
                }

                var audioManager = managerObj.GetComponent<AudioManager>();
                if (audioManager == null)
                {
                    audioManager = managerObj.AddComponent<AudioManager>();
                }

                if (audioManager != null)
                {
                    Undo.RecordObject(audioManager, "Update Audio Manager Config");

                    audioManager.mainMenuMusic.Clear();
                    foreach (var clip in _tempMainMenuMusic)
                    {
                        audioManager.mainMenuMusic.Add(clip);
                    }
                    audioManager.loopMainMenuMusic = _tempLoopMainMenuMusic;
                    audioManager.mainMenuMusicVolume = _tempMainMenuMusicVolume;

                    audioManager.clickSFX = _tempClickSFX;
                    audioManager.clickSFXVolume = _tempClickSFXVolume;

                    audioManager.toggleSFX = _tempToggleSFX;
                    audioManager.toggleSFXVolume = _tempToggleSFXVolume;

                    audioManager.panelChangeSFX = _tempPanelChangeSFX;
                    audioManager.panelChangeSFXVolume = _tempPanelChangeSFXVolume;

                    audioManager.navigateSFX = _tempNavigateSFX;
                    audioManager.navigateSFXVolume = _tempNavigateSFXVolume;
                }
            }

            var eventSystem = GameObject.Find("EventSystem") ?? GameObject.Find("UIEventSystem");
            if (eventSystem != null && _tempInputAsset != null)
            {
                var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                if (inputModule != null)
                {
                    Undo.RecordObject(inputModule, "Update Input Asset");
                    inputModule.actionsAsset = _tempInputAsset;
                }
            }

            if (managerObj != null) EditorUtility.SetDirty(managerObj);
            if (eventSystem != null) EditorUtility.SetDirty(eventSystem);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", "All configurator changes have been successfully saved to the active scene!", "OK");
            DrawStep();
        }

        private void OnBackClicked()
        {
            if (_currentStep == 6)
            {
                _currentStep = 1;
                SaveState();
                DrawStep();
                return;
            }

            if (_currentStep > 1)
            {
                _currentStep--;
                if (_currentStep == 2 && _selectedPreset == "VR / XR")
                {
                    _currentStep--;
                }
                SaveState();
                DrawStep();
            }
        }

        private void OnNextClicked()
        {
            var nextBtn = rootVisualElement.Q<Button>("next-btn");
            if (nextBtn != null && !nextBtn.enabledSelf) return;

            if (_currentStep == 4)
            {
                MainMenuGenerator.Generate(_selectedPreset, _is2D, _saveSlotsCount, _outputPath, _menuAlignment);
                _currentStep = 5;
                SaveState();
                DrawStep();
            }
            else if (_currentStep < 4)
            {
                _currentStep++;
                if (_currentStep == 2 && _selectedPreset == "VR / XR")
                {
                    _is2D = false;
                    _currentStep++;
                }
                SaveState();
                DrawStep();
            }
        }

        private List<string> GetRequiredPackages()
        {
            List<string> required = new List<string> {
                "com.unity.inputsystem",
                "com.unity.render-pipelines.universal",
                "com.unity.cinemachine"
            };

            if (_is2D)
            {
                required.Add("com.unity.2d.sprite");
            }

            if (_selectedPreset == "VR / XR")
            {
                required.Add("com.unity.xr.interaction.toolkit");
                required.Add("com.unity.xr.openxr");
            }

            if (_selectedPreset == "Online Multiplayer")
            {
                required.Add("com.unity.netcode.gameobjects");
            }

            return required;
        }

        private List<string> GetMissingPackages()
        {
            var required = GetRequiredPackages();
            string manifestPath = "Packages/manifest.json";
            if (!File.Exists(manifestPath)) return required;

            string content = File.ReadAllText(manifestPath);
            List<string> missing = new List<string>();

            foreach (var pkg in required)
            {
                if (!content.Contains($"\"{pkg}\""))
                {
                    missing.Add(pkg);
                }
            }

            return missing;
        }

        private void InstallMissingPackages(List<string> packageIds)
        {
            string path = "Packages/manifest.json";
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);
            foreach (var pkg in packageIds)
            {
                if (content.Contains(pkg)) continue;

                int index = content.IndexOf("\"dependencies\": {");
                if (index != -1)
                {
                    int insertIndex = content.IndexOf("\n", index) + 1;
                    
                    string version = "1.0.0";
                    if (pkg == "com.unity.cinemachine") version = "3.1.7";
                    if (pkg == "com.unity.inputsystem") version = "1.19.0";
                    if (pkg == "com.unity.render-pipelines.universal") version = "17.4.0";
                    if (pkg == "com.unity.2d.sprite") version = "1.0.0";
                    if (pkg == "com.unity.xr.interaction.toolkit") version = "3.0.7";
                    if (pkg == "com.unity.xr.openxr") version = "1.11.1";
                    if (pkg == "com.unity.netcode.gameobjects") version = "1.9.1";

                    string line = $"    \"{pkg}\": \"{version}\",\n";
                    content = content.Insert(insertIndex, line);
                }
            }
            File.WriteAllText(path, content);
            UnityEditor.PackageManager.Client.Resolve();

            EditorUtility.DisplayDialog("Installing Packages", "Dependencies added to Packages/manifest.json. Unity is compiling packages...", "OK");
        }
    }
}
