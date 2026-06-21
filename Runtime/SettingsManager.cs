using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

[System.Serializable]
public class ExposedControl
{
    public string displayName;
    public string actionName; // e.g. "Player/Jump"
}

public class SettingsManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _root;

    // Tabs
    private Button _tabAudioBtn;
    private Button _tabGraphicsBtn;
    private Button _tabControlsBtn;

    private VisualElement _tabContentAudio;
    private VisualElement _tabContentGraphics;
    private VisualElement _tabContentControls;

    // Audio Controls
    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;

    // Graphics Controls
    private DropdownField _resolutionDropdown;
    private Toggle _fullscreenToggle;
    private DropdownField _qualityDropdown;
    private Toggle _vsyncToggle;

    // Controls/Rebinding
    public List<ExposedControl> exposedKeyboardControls = new List<ExposedControl>();
    public List<ExposedControl> exposedControllerControls = new List<ExposedControl>();

    private Button _subTabKeyboardBtn;
    private Button _subTabControllerBtn;
    private ScrollView _keyboardScroll;
    private ScrollView _controllerScroll;
    private VisualElement _rebindOverlay;
    private Label _rebindOverlayText;
    private InputActionAsset _inputActionAsset;
    private List<VisualElement> _keybindRows = new List<VisualElement>();
    private string _activeControlsSubTab = "keyboard";

    private Button _backButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;

        // Query Tabs
        _tabAudioBtn = _root.Q<Button>("tab-audio-btn");
        _tabGraphicsBtn = _root.Q<Button>("tab-graphics-btn");
        _tabControlsBtn = _root.Q<Button>("tab-controls-btn");

        _tabContentAudio = _root.Q<VisualElement>("tab-content-audio");
        _tabContentGraphics = _root.Q<VisualElement>("tab-content-graphics");
        _tabContentControls = _root.Q<VisualElement>("tab-content-controls");

        if (_tabAudioBtn != null) _tabAudioBtn.clicked += () => SwitchTab("audio");
        if (_tabGraphicsBtn != null) _tabGraphicsBtn.clicked += () => SwitchTab("graphics");
        if (_tabControlsBtn != null) _tabControlsBtn.clicked += () => SwitchTab("controls");

        // Query Audio
        _masterSlider = _root.Q<Slider>("master-volume-slider");
        _musicSlider = _root.Q<Slider>("music-volume-slider");
        _sfxSlider = _root.Q<Slider>("sfx-volume-slider");

        if (_masterSlider != null)
        {
            _masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            _masterSlider.RegisterValueChangedCallback(evt => SetVolumeHelper("Master", evt.newValue));
        }
        if (_musicSlider != null)
        {
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            _musicSlider.RegisterValueChangedCallback(evt => SetVolumeHelper("Music", evt.newValue));
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            _sfxSlider.RegisterValueChangedCallback(evt => SetVolumeHelper("SFX", evt.newValue));
        }

        // Query Graphics
        _resolutionDropdown = _root.Q<DropdownField>("resolution-dropdown");
        _fullscreenToggle = _root.Q<Toggle>("fullscreen-toggle");
        _qualityDropdown = _root.Q<DropdownField>("quality-dropdown");
        _vsyncToggle = _root.Q<Toggle>("vsync-toggle");

        InitializeGraphicsUI();

        // Query Controls & Rebind
        _subTabKeyboardBtn = _root.Q<Button>("controls-sub-tab-keyboard-btn");
        _subTabControllerBtn = _root.Q<Button>("controls-sub-tab-controller-btn");
        _keyboardScroll = _root.Q<ScrollView>("controls-keyboard-scroll");
        _controllerScroll = _root.Q<ScrollView>("controls-controller-scroll");
        _rebindOverlay = _root.Q<VisualElement>("rebind-overlay");
        _rebindOverlayText = _root.Q<Label>("rebind-overlay-text");

        if (_subTabKeyboardBtn != null) _subTabKeyboardBtn.clicked += () => SwitchControlsSubTab("keyboard");
        if (_subTabControllerBtn != null) _subTabControllerBtn.clicked += () => SwitchControlsSubTab("controller");

        InitializeControlsUI();

        // Query Back
        _backButton = _root.Q<Button>("settings-back-button");
        if (_backButton != null)
        {
            _backButton.clicked += () => PanelManager.Instance?.PopPanel();
        }
    }

    private void SwitchTab(string tabName)
    {
        // Toggle contents
        if (_tabContentAudio != null) _tabContentAudio.style.display = tabName == "audio" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_tabContentGraphics != null) _tabContentGraphics.style.display = tabName == "graphics" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_tabContentControls != null) _tabContentControls.style.display = tabName == "controls" ? DisplayStyle.Flex : DisplayStyle.None;

        // Toggle button active classes
        UpdateButtonTabStyle(_tabAudioBtn, tabName == "audio");
        UpdateButtonTabStyle(_tabGraphicsBtn, tabName == "graphics");
        UpdateButtonTabStyle(_tabControlsBtn, tabName == "controls");
    }

    private void SwitchControlsSubTab(string subTab)
    {
        _activeControlsSubTab = subTab;
        if (_keyboardScroll != null) _keyboardScroll.style.display = subTab == "keyboard" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_controllerScroll != null) _controllerScroll.style.display = subTab == "controller" ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateSubTabButtonStyle(_subTabKeyboardBtn, subTab == "keyboard");
        UpdateSubTabButtonStyle(_subTabControllerBtn, subTab == "controller");
    }

    private void UpdateButtonTabStyle(Button btn, bool active)
    {
        if (btn == null) return;
        if (active)
        {
            btn.AddToClassList("settings-tab-button--active");
        }
        else
        {
            btn.RemoveFromClassList("settings-tab-button--active");
        }
    }

    private void UpdateSubTabButtonStyle(Button btn, bool active)
    {
        if (btn == null) return;
        if (active)
        {
            btn.AddToClassList("controls-sub-tab-button--active");
        }
        else
        {
            btn.RemoveFromClassList("controls-sub-tab-button--active");
        }
    }

    private void InitializeGraphicsUI()
    {
        // Fullscreen Mode
        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.value = Screen.fullScreen;
            _fullscreenToggle.RegisterValueChangedCallback(evt => Screen.fullScreen = evt.newValue);
        }

        // Resolutions
        if (_resolutionDropdown != null)
        {
            List<string> options = new List<string>();
            int currentResIndex = -1;
            
            // Deduplicate and format resolutions
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                var res = Screen.resolutions[i];
                string opt = res.width + "x" + res.height;
                if (!options.Contains(opt))
                {
                    options.Add(opt);
                }
                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                {
                    currentResIndex = options.Count - 1;
                }
            }

            _resolutionDropdown.choices = options;
            if (currentResIndex >= 0 && currentResIndex < options.Count)
            {
                _resolutionDropdown.value = options[currentResIndex];
            }
            else if (options.Count > 0)
            {
                _resolutionDropdown.value = options[options.Count - 1]; // Fallback
            }

            _resolutionDropdown.RegisterValueChangedCallback(evt => {
                string[] parts = evt.newValue.Split('x');
                if (parts.Length == 2)
                {
                    int width = int.Parse(parts[0]);
                    int height = int.Parse(parts[1]);
                    Screen.SetResolution(width, height, Screen.fullScreen);
                }
            });
        }

        // Quality Presets
        if (_qualityDropdown != null)
        {
            List<string> options = new List<string>(QualitySettings.names);
            _qualityDropdown.choices = options;
            _qualityDropdown.value = options[QualitySettings.GetQualityLevel()];
            _qualityDropdown.RegisterValueChangedCallback(evt => {
                int index = options.IndexOf(evt.newValue);
                if (index >= 0)
                {
                    QualitySettings.SetQualityLevel(index, true);
                }
            });
        }

        // VSync Toggle
        if (_vsyncToggle != null)
        {
            _vsyncToggle.value = QualitySettings.vSyncCount > 0;
            _vsyncToggle.RegisterValueChangedCallback(evt => {
                QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
            });
        }
    }

    private void InitializeControlsUI()
    {
        var scrollContainer = _keyboardScroll ?? _controllerScroll;
        if (scrollContainer == null) return;

        // Try to load InputActionAsset from EventSystem
        var eventSystem = GameObject.Find("EventSystem") ?? GameObject.Find("UIEventSystem");
        if (eventSystem != null)
        {
            var inputModule = eventSystem.GetComponent("InputSystemUIInputModule");
            if (inputModule != null)
            {
                var actionsProp = inputModule.GetType().GetProperty("actionsAsset");
                if (actionsProp != null)
                {
                    _inputActionAsset = actionsProp.GetValue(inputModule) as InputActionAsset;
                }
            }
        }

        if (_inputActionAsset == null)
        {
            var warning = new Label("No Input Action Asset is linked to the EventSystem. Map one in the Setup Wizard Dashboard.");
            warning.style.color = Color.gray;
            warning.style.unityFontStyleAndWeight = FontStyle.Italic;
            warning.style.whiteSpace = WhiteSpace.Normal;
            if (_keyboardScroll != null) _keyboardScroll.Add(warning);
            return;
        }

        // Load saved overrides
        string overrides = PlayerPrefs.GetString("InputBindingOverrides", "");
        if (!string.IsNullOrEmpty(overrides))
        {
            _inputActionAsset.LoadBindingOverridesFromJson(overrides);
        }

        RebuildKeybindsList();
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
        public InputAction action;
        public string partName;
        public int primaryIndex = -1;
        public int secondaryIndex = -1;
    }

    private List<BindableEntry> GetBindableEntries(InputAction action, bool isKeyboard)
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

    private void RebuildKeybindsList()
    {
        if (_keyboardScroll != null) _keyboardScroll.Clear();
        if (_controllerScroll != null) _controllerScroll.Clear();
        _keybindRows.Clear();

        // Build Keyboard Controls
        if (_keyboardScroll != null)
        {
            foreach (var ctrl in exposedKeyboardControls)
            {
                var action = _inputActionAsset.FindAction(ctrl.actionName);
                if (action == null) continue;

                var entries = GetBindableEntries(action, true);
                foreach (var entry in entries)
                {
                    if (entry.partName != null && entry.primaryIndex == -1 && entry.secondaryIndex == -1)
                    {
                        continue;
                    }

                    string labelText = ctrl.displayName;
                    if (entry.partName != null)
                    {
                        labelText += " - " + entry.partName.ToUpper();
                    }

                    var row = CreateKeybindRow(labelText, action, entry.primaryIndex, entry.secondaryIndex);
                    _keyboardScroll.Add(row);
                }
            }
        }

        // Build Controller Controls
        if (_controllerScroll != null)
        {
            foreach (var ctrl in exposedControllerControls)
            {
                var action = _inputActionAsset.FindAction(ctrl.actionName);
                if (action == null) continue;

                var entries = GetBindableEntries(action, false);
                foreach (var entry in entries)
                {
                    if (entry.partName != null && entry.primaryIndex == -1 && entry.secondaryIndex == -1)
                    {
                        continue;
                    }

                    string labelText = ctrl.displayName;
                    if (entry.partName != null)
                    {
                        labelText += " - " + entry.partName.ToUpper();
                    }

                    var row = CreateKeybindRow(labelText, action, entry.primaryIndex, entry.secondaryIndex);
                    _controllerScroll.Add(row);
                }
            }
        }
    }

    private VisualElement CreateKeybindRow(string labelText, InputAction action, int primaryIndex, int secondaryIndex)
    {
        var row = new VisualElement();
        row.AddToClassList("keybind-row");

        var label = new Label(labelText);
        label.AddToClassList("keybind-label");
        row.Add(label);

        var btnContainer = new VisualElement();
        btnContainer.AddToClassList("keybind-buttons-container");
        btnContainer.style.flexDirection = FlexDirection.Row;

        // Primary Button
        var btnPrimary = new Button();
        btnPrimary.AddToClassList("keybind-button");
        if (primaryIndex >= 0)
        {
            UpdateBindingButtonText(btnPrimary, action, primaryIndex);
            int idx = primaryIndex;
            var act = action;
            btnPrimary.clicked += () => StartRebinding(act, idx, btnPrimary);
        }
        else
        {
            btnPrimary.text = "Not Assigned";
            btnPrimary.SetEnabled(false);
        }
        btnContainer.Add(btnPrimary);

        // Secondary Button
        var btnSecondary = new Button();
        btnSecondary.AddToClassList("keybind-button");
        if (secondaryIndex >= 0)
        {
            UpdateBindingButtonText(btnSecondary, action, secondaryIndex);
            int idx = secondaryIndex;
            var act = action;
            btnSecondary.clicked += () => StartRebinding(act, idx, btnSecondary);
        }
        else
        {
            btnSecondary.text = "Not Assigned";
            btnSecondary.SetEnabled(false);
        }
        btnContainer.Add(btnSecondary);

        row.Add(btnContainer);
        _keybindRows.Add(row);
        return row;
    }

    private void UpdateBindingButtonText(Button btn, InputAction action, int bindingIndex)
    {
        string display = action.GetBindingDisplayString(bindingIndex);
        if (string.IsNullOrEmpty(display))
        {
            display = "[None]";
        }
        btn.text = display;
    }

    private void StartRebinding(InputAction action, int bindingIndex, Button button)
    {
        if (action == null || _rebindOverlay == null) return;

        // Disable action map during rebind
        var map = action.actionMap;
        bool wasEnabled = map.enabled;
        if (wasEnabled) map.Disable();

        _rebindOverlay.style.display = DisplayStyle.Flex;

        var operation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse") // Prevent accidental mouse click binding
            .OnMatchWaitForAnother(0.1f);

        if (action.bindings[bindingIndex].isPartOfComposite || action.type == InputActionType.Button)
        {
            operation.WithExpectedControlType("Button");
        }

        operation.WithBindingGroup(null);

        operation.OnPotentialMatch(op => Debug.Log("[Rebind] Potential match: " + op.selectedControl.path))
            .OnComplete(op => {
                Debug.Log("[Rebind] Completed with control: " + op.selectedControl.path);
                _rebindOverlay.style.display = DisplayStyle.None;
                if (wasEnabled) map.Enable();

                // Save overrides
                string overrides = _inputActionAsset.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("InputBindingOverrides", overrides);
                PlayerPrefs.Save();

                UpdateBindingButtonText(button, action, bindingIndex);
                op.Dispose();
            })
            .OnCancel(op => {
                Debug.Log("[Rebind] Cancelled");
                _rebindOverlay.style.display = DisplayStyle.None;
                if (wasEnabled) map.Enable();
                UpdateBindingButtonText(button, action, bindingIndex);
                op.Dispose();
            });

        operation.Start();
    }

    private void SetVolumeHelper(string type, float value)
    {
        var audioManagerType = System.Type.GetType("AudioManager");
        if (audioManagerType != null)
        {
            var instanceProp = audioManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var method = audioManagerType.GetMethod("Set" + type + "Volume");
                    if (method != null)
                    {
                        method.Invoke(instance, new object[] { value });
                        return;
                    }
                }
            }
        }
        PlayerPrefs.SetFloat(type + "Volume", value);
    }
}
