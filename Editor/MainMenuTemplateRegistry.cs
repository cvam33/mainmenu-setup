using System.Collections.Generic;

namespace MCPForUnity.Editor.Helpers
{
    public static class MainMenuTemplateRegistry
    {
        public static string GetMainMenuManager(string preset, bool is2D)
        {
            return @"using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    public string sceneToLoad = ""GameScene"";

    private UIDocument _uiDocument;
    private VisualElement _root;
    
    private Button _playButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _quitButton;
    private Button _creditsBackButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;
        
        _root = _uiDocument.rootVisualElement;
        
        _playButton = _root.Q<Button>(""play-button"");
        _settingsButton = _root.Q<Button>(""settings-button"");
        _creditsButton = _root.Q<Button>(""credits-button"");
        _quitButton = _root.Q<Button>(""quit-button"");
        _creditsBackButton = _root.Q<Button>(""credits-back-button"");

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
        if (_creditsButton != null) _creditsButton.clicked += OnCreditsClicked;
        if (_quitButton != null) _quitButton.clicked += OnQuitClicked;
        if (_creditsBackButton != null) _creditsBackButton.clicked += OnCreditsBackClicked;
    }

    private void Start()
    {
        PanelManager.Instance.ShowPanel(""main-menu"");
    }

    private void OnPlayClicked()
    {
        Debug.Log(""Play Clicked!"");
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError(""SceneTransitionManager.Instance is null!"");
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log(""Settings Clicked!"");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PushPanel(""settings-panel"");
        }
        else
        {
            Debug.LogError(""PanelManager.Instance is null!"");
        }
    }

    private void OnCreditsClicked()
    {
        Debug.Log(""Credits Clicked!"");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PushPanel(""credits-panel"");
        }
        else
        {
            Debug.LogError(""PanelManager.Instance is null!"");
        }
    }

    private void OnCreditsBackClicked()
    {
        Debug.Log(""Credits Back Clicked!"");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PopPanel();
        }
        else
        {
            Debug.LogError(""PanelManager.Instance is null!"");
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log(""Quit Clicked!"");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}";
        }

        public static string GetPanelManager()
        {
            return @"using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    private UIDocument _uiDocument;
    private VisualElement _root;
    private Stack<VisualElement> _panelStack = new Stack<VisualElement>();
    private List<VisualElement> _allPanels = new List<VisualElement>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null)
        {
            _root = _uiDocument.rootVisualElement;
            // Find all panels (by convention, elements with class 'menu-panel')
            _root.Query<VisualElement>(className: ""menu-panel"").ForEach(panel =>
            {
                _allPanels.Add(panel);
                panel.style.display = DisplayStyle.None;
            });
        }
    }

    public void ShowPanel(string panelName)
    {
        _panelStack.Clear();
        foreach (var p in _allPanels)
        {
            if (p.name == panelName)
            {
                p.style.display = DisplayStyle.Flex;
                _panelStack.Push(p);
            }
            else
            {
                p.style.display = DisplayStyle.None;
            }
        }
    }

    public void PushPanel(string panelName)
    {
        VisualElement targetPanel = _allPanels.Find(p => p.name == panelName);
        if (targetPanel == null) return;

        if (_panelStack.Count > 0)
        {
            _panelStack.Peek().style.display = DisplayStyle.None;
        }

        targetPanel.style.display = DisplayStyle.Flex;
        _panelStack.Push(targetPanel);
        
        AudioManager.Instance?.PlayPanelChangeSFX();
    }

    public void PopPanel()
    {
        if (_panelStack.Count <= 1) return;

        VisualElement current = _panelStack.Pop();
        current.style.display = DisplayStyle.None;

        if (_panelStack.Count > 0)
        {
            _panelStack.Peek().style.display = DisplayStyle.Flex;
        }
        
        AudioManager.Instance?.PlayPanelChangeSFX();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PopPanel();
        }
    }
}";
        }

        public static string GetSceneTransitionManager()
        {
            return @"using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private UIDocument _uiDocument;
    private VisualElement _fadeOverlay;
    private float _fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            _uiDocument = gameObject.AddComponent<UIDocument>();
            // Try to link the PanelSettings from an existing UIDocument in the scene
            var existingDoc = FindAnyObjectByType<UIDocument>();
            if (existingDoc != null && existingDoc != _uiDocument)
            {
                _uiDocument.panelSettings = existingDoc.panelSettings;
            }
        }

        if (_uiDocument != null)
        {
            var root = _uiDocument.rootVisualElement;
            if (root != null)
            {
                _fadeOverlay = root.Q(""fade-overlay"");
                if (_fadeOverlay == null)
                {
                    _fadeOverlay = new VisualElement();
                    _fadeOverlay.name = ""fade-overlay"";
                    _fadeOverlay.style.position = Position.Absolute;
                    _fadeOverlay.style.left = 0;
                    _fadeOverlay.style.top = 0;
                    _fadeOverlay.style.right = 0;
                    _fadeOverlay.style.bottom = 0;
                    _fadeOverlay.style.backgroundColor = Color.black;
                    _fadeOverlay.pickingMode = PickingMode.Ignore;
                    root.Add(_fadeOverlay);
                }

                _fadeOverlay.style.opacity = 1f;
                StartCoroutine(FadeIn());
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            if (_fadeOverlay != null) _fadeOverlay.style.opacity = 1f - (timer / _fadeDuration);
            yield return null;
        }
        if (_fadeOverlay != null) _fadeOverlay.style.opacity = 0f;
    }

    private IEnumerator FadeOut()
    {
        float timer = 0;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            if (_fadeOverlay != null) _fadeOverlay.style.opacity = timer / _fadeDuration;
            yield return null;
        }
        if (_fadeOverlay != null) _fadeOverlay.style.opacity = 1f;
    }
}";
        }

        public static string GetAudioManager()
        {
            return @"using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header(""Music Playlist"")]
    public List<AudioClip> mainMenuMusic = new List<AudioClip>();
    public bool loopMainMenuMusic = true;
    [Range(0f, 1f)] public float mainMenuMusicVolume = 0.5f;

    [Header(""UI SFX Clips"")]
    public AudioClip clickSFX;
    [Range(0f, 1f)] public float clickSFXVolume = 0.8f;

    public AudioClip toggleSFX;
    [Range(0f, 1f)] public float toggleSFXVolume = 0.8f;

    public AudioClip panelChangeSFX;
    [Range(0f, 1f)] public float panelChangeSFXVolume = 0.8f;

    public AudioClip navigateSFX;
    [Range(0f, 1f)] public float navigateSFXVolume = 0.6f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private int _currentMusicIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
    }

    private void Start()
    {
        UpdateVolumes();

        if (mainMenuMusic.Count > 0)
        {
            PlayMusicAtIndex(0);
        }

        // Dynamically find UIDocument root and bind listeners
        var uiDoc = GetComponent<UIDocument>() ?? FindFirstObjectByType<UIDocument>();
        if (uiDoc != null && uiDoc.rootVisualElement != null)
        {
            HookUIEvents(uiDoc.rootVisualElement);
        }
    }

    private void Update()
    {
        if (_musicSource != null && !_musicSource.isPlaying && mainMenuMusic.Count > 0)
        {
            int nextIndex = _currentMusicIndex + 1;
            if (nextIndex >= mainMenuMusic.Count)
            {
                if (loopMainMenuMusic)
                {
                    nextIndex = 0;
                }
                else
                {
                    nextIndex = -1;
                }
            }

            if (nextIndex >= 0)
            {
                PlayMusicAtIndex(nextIndex);
            }
        }
    }

    public void PlayMusicAtIndex(int index)
    {
        if (index < 0 || index >= mainMenuMusic.Count) return;
        _currentMusicIndex = index;

        float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
        float music = PlayerPrefs.GetFloat(""MusicVolume"", 0.75f);

        _musicSource.clip = mainMenuMusic[index];
        _musicSource.volume = mainMenuMusicVolume * music * master;
        _musicSource.Play();
    }

    public void PlayClickSFX()
    {
        if (clickSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
            float sfx = PlayerPrefs.GetFloat(""SFXVolume"", 0.75f);
            _sfxSource.PlayOneShot(clickSFX, clickSFXVolume * sfx * master);
        }
    }

    public void PlayToggleSFX()
    {
        if (toggleSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
            float sfx = PlayerPrefs.GetFloat(""SFXVolume"", 0.75f);
            _sfxSource.PlayOneShot(toggleSFX, toggleSFXVolume * sfx * master);
        }
    }

    public void PlayPanelChangeSFX()
    {
        if (panelChangeSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
            float sfx = PlayerPrefs.GetFloat(""SFXVolume"", 0.75f);
            _sfxSource.PlayOneShot(panelChangeSFX, panelChangeSFXVolume * sfx * master);
        }
    }

    public void PlayNavigateSFX()
    {
        if (navigateSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
            float sfx = PlayerPrefs.GetFloat(""SFXVolume"", 0.75f);
            _sfxSource.PlayOneShot(navigateSFX, navigateSFXVolume * sfx * master);
        }
    }

    public void HookUIEvents(VisualElement root)
    {
        if (root == null) return;

        // Buttons
        root.Query<Button>().ForEach(btn =>
        {
            btn.RegisterCallback<ClickEvent>(evt => PlayClickSFX());
            btn.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Toggles
        root.Query<Toggle>().ForEach(tgl =>
        {
            tgl.RegisterCallback<ChangeEvent<bool>>(evt => PlayToggleSFX());
            tgl.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Sliders
        root.Query<Slider>().ForEach(sld =>
        {
            // Small tick audio on value change
            sld.RegisterCallback<ChangeEvent<float>>(evt => PlayClickSFX());
            sld.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Dropdowns
        root.Query<DropdownField>().ForEach(drp =>
        {
            drp.RegisterCallback<ChangeEvent<string>>(evt => PlayClickSFX());
            drp.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(""MasterVolume"", volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(""MusicVolume"", volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(""SFXVolume"", volume);
        UpdateVolumes();
    }

    public void UpdateVolumes()
    {
        float master = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
        float music = PlayerPrefs.GetFloat(""MusicVolume"", 0.75f);

        if (_musicSource != null)
        {
            _musicSource.volume = mainMenuMusicVolume * music * master;
        }
    }
}";
        }

        public static string GetSettingsManager()
        {
            return @"using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

[System.Serializable]
public class ExposedControl
{
    public string displayName;
    public string actionName; // e.g. ""Player/Jump""
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
    private string _activeControlsSubTab = ""keyboard"";

    private Button _backButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;

        // Query Tabs
        _tabAudioBtn = _root.Q<Button>(""tab-audio-btn"");
        _tabGraphicsBtn = _root.Q<Button>(""tab-graphics-btn"");
        _tabControlsBtn = _root.Q<Button>(""tab-controls-btn"");

        _tabContentAudio = _root.Q<VisualElement>(""tab-content-audio"");
        _tabContentGraphics = _root.Q<VisualElement>(""tab-content-graphics"");
        _tabContentControls = _root.Q<VisualElement>(""tab-content-controls"");

        if (_tabAudioBtn != null) _tabAudioBtn.clicked += () => SwitchTab(""audio"");
        if (_tabGraphicsBtn != null) _tabGraphicsBtn.clicked += () => SwitchTab(""graphics"");
        if (_tabControlsBtn != null) _tabControlsBtn.clicked += () => SwitchTab(""controls"");

        // Query Audio
        _masterSlider = _root.Q<Slider>(""master-volume-slider"");
        _musicSlider = _root.Q<Slider>(""music-volume-slider"");
        _sfxSlider = _root.Q<Slider>(""sfx-volume-slider"");

        if (_masterSlider != null)
        {
            _masterSlider.value = PlayerPrefs.GetFloat(""MasterVolume"", 0.75f);
            _masterSlider.RegisterValueChangedCallback(evt => SetVolumeHelper(""Master"", evt.newValue));
        }
        if (_musicSlider != null)
        {
            _musicSlider.value = PlayerPrefs.GetFloat(""MusicVolume"", 0.75f);
            _musicSlider.RegisterValueChangedCallback(evt => SetVolumeHelper(""Music"", evt.newValue));
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = PlayerPrefs.GetFloat(""SFXVolume"", 0.75f);
            _sfxSlider.RegisterValueChangedCallback(evt => SetVolumeHelper(""SFX"", evt.newValue));
        }

        // Query Graphics
        _resolutionDropdown = _root.Q<DropdownField>(""resolution-dropdown"");
        _fullscreenToggle = _root.Q<Toggle>(""fullscreen-toggle"");
        _qualityDropdown = _root.Q<DropdownField>(""quality-dropdown"");
        _vsyncToggle = _root.Q<Toggle>(""vsync-toggle"");

        InitializeGraphicsUI();

        // Query Controls & Rebind
        _subTabKeyboardBtn = _root.Q<Button>(""controls-sub-tab-keyboard-btn"");
        _subTabControllerBtn = _root.Q<Button>(""controls-sub-tab-controller-btn"");
        _keyboardScroll = _root.Q<ScrollView>(""controls-keyboard-scroll"");
        _controllerScroll = _root.Q<ScrollView>(""controls-controller-scroll"");
        _rebindOverlay = _root.Q<VisualElement>(""rebind-overlay"");
        _rebindOverlayText = _root.Q<Label>(""rebind-overlay-text"");

        if (_subTabKeyboardBtn != null) _subTabKeyboardBtn.clicked += () => SwitchControlsSubTab(""keyboard"");
        if (_subTabControllerBtn != null) _subTabControllerBtn.clicked += () => SwitchControlsSubTab(""controller"");

        InitializeControlsUI();

        // Query Back
        _backButton = _root.Q<Button>(""settings-back-button"");
        if (_backButton != null)
        {
            _backButton.clicked += () => PanelManager.Instance?.PopPanel();
        }
    }

    private void SwitchTab(string tabName)
    {
        // Toggle contents
        if (_tabContentAudio != null) _tabContentAudio.style.display = tabName == ""audio"" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_tabContentGraphics != null) _tabContentGraphics.style.display = tabName == ""graphics"" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_tabContentControls != null) _tabContentControls.style.display = tabName == ""controls"" ? DisplayStyle.Flex : DisplayStyle.None;

        // Toggle button active classes
        UpdateButtonTabStyle(_tabAudioBtn, tabName == ""audio"");
        UpdateButtonTabStyle(_tabGraphicsBtn, tabName == ""graphics"");
        UpdateButtonTabStyle(_tabControlsBtn, tabName == ""controls"");
    }

    private void SwitchControlsSubTab(string subTab)
    {
        _activeControlsSubTab = subTab;
        if (_keyboardScroll != null) _keyboardScroll.style.display = subTab == ""keyboard"" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_controllerScroll != null) _controllerScroll.style.display = subTab == ""controller"" ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateSubTabButtonStyle(_subTabKeyboardBtn, subTab == ""keyboard"");
        UpdateSubTabButtonStyle(_subTabControllerBtn, subTab == ""controller"");
    }

    private void UpdateButtonTabStyle(Button btn, bool active)
    {
        if (btn == null) return;
        if (active)
        {
            btn.AddToClassList(""settings-tab-button--active"");
        }
        else
        {
            btn.RemoveFromClassList(""settings-tab-button--active"");
        }
    }

    private void UpdateSubTabButtonStyle(Button btn, bool active)
    {
        if (btn == null) return;
        if (active)
        {
            btn.AddToClassList(""controls-sub-tab-button--active"");
        }
        else
        {
            btn.RemoveFromClassList(""controls-sub-tab-button--active"");
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
                string opt = res.width + ""x"" + res.height;
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
        var eventSystem = GameObject.Find(""EventSystem"") ?? GameObject.Find(""UIEventSystem"");
        if (eventSystem != null)
        {
            var inputModule = eventSystem.GetComponent(""InputSystemUIInputModule"");
            if (inputModule != null)
            {
                var actionsProp = inputModule.GetType().GetProperty(""actionsAsset"");
                if (actionsProp != null)
                {
                    _inputActionAsset = actionsProp.GetValue(inputModule) as InputActionAsset;
                }
            }
        }

        if (_inputActionAsset == null)
        {
            var warning = new Label(""No Input Action Asset is linked to the EventSystem. Map one in the Setup Wizard Dashboard."");
            warning.style.color = Color.gray;
            warning.style.unityFontStyleAndWeight = FontStyle.Italic;
            warning.style.whiteSpace = WhiteSpace.Normal;
            if (_keyboardScroll != null) _keyboardScroll.Add(warning);
            return;
        }

        // Load saved overrides
        string overrides = PlayerPrefs.GetString(""InputBindingOverrides"", """");
        if (!string.IsNullOrEmpty(overrides))
        {
            _inputActionAsset.LoadBindingOverridesFromJson(overrides);
        }

        RebuildKeybindsList();
    }

    private bool IsKeyboardMouseBinding(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return path.StartsWith(""<Keyboard>"") || path.StartsWith(""<Mouse>"") || path.StartsWith(""<Pointer>"") || path.StartsWith(""<Pen>"");
    }

    private bool IsControllerBinding(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return path.StartsWith(""<Gamepad>"") || path.StartsWith(""<Joystick>"") || path.StartsWith(""<XRController>"");
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
                        labelText += "" - "" + entry.partName.ToUpper();
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
                        labelText += "" - "" + entry.partName.ToUpper();
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
        row.AddToClassList(""keybind-row"");

        var label = new Label(labelText);
        label.AddToClassList(""keybind-label"");
        row.Add(label);

        var btnContainer = new VisualElement();
        btnContainer.AddToClassList(""keybind-buttons-container"");
        btnContainer.style.flexDirection = FlexDirection.Row;

        // Primary Button
        var btnPrimary = new Button();
        btnPrimary.AddToClassList(""keybind-button"");
        if (primaryIndex >= 0)
        {
            UpdateBindingButtonText(btnPrimary, action, primaryIndex);
            int idx = primaryIndex;
            var act = action;
            btnPrimary.clicked += () => StartRebinding(act, idx, btnPrimary);
        }
        else
        {
            btnPrimary.text = ""Not Assigned"";
            btnPrimary.SetEnabled(false);
        }
        btnContainer.Add(btnPrimary);

        // Secondary Button
        var btnSecondary = new Button();
        btnSecondary.AddToClassList(""keybind-button"");
        if (secondaryIndex >= 0)
        {
            UpdateBindingButtonText(btnSecondary, action, secondaryIndex);
            int idx = secondaryIndex;
            var act = action;
            btnSecondary.clicked += () => StartRebinding(act, idx, btnSecondary);
        }
        else
        {
            btnSecondary.text = ""Not Assigned"";
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
            display = ""[None]"";
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
            .WithControlsExcluding(""Mouse"") // Prevent accidental mouse click binding
            .OnMatchWaitForAnother(0.1f);

        if (action.bindings[bindingIndex].isPartOfComposite || action.type == InputActionType.Button)
        {
            operation.WithExpectedControlType(""Button"");
        }

        operation.WithBindingGroup(null);

        operation.OnPotentialMatch(op => Debug.Log(""[Rebind] Potential match: "" + op.selectedControl.path))
            .OnComplete(op => {
                Debug.Log(""[Rebind] Completed with control: "" + op.selectedControl.path);
                _rebindOverlay.style.display = DisplayStyle.None;
                if (wasEnabled) map.Enable();

                // Save overrides
                string overrides = _inputActionAsset.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(""InputBindingOverrides"", overrides);
                PlayerPrefs.Save();

                UpdateBindingButtonText(button, action, bindingIndex);
                op.Dispose();
            })
            .OnCancel(op => {
                Debug.Log(""[Rebind] Cancelled"");
                _rebindOverlay.style.display = DisplayStyle.None;
                if (wasEnabled) map.Enable();
                UpdateBindingButtonText(button, action, bindingIndex);
                op.Dispose();
            });

        operation.Start();
    }

    private void SetVolumeHelper(string type, float value)
    {
        var audioManagerType = System.Type.GetType(""AudioManager"");
        if (audioManagerType != null)
        {
            var instanceProp = audioManagerType.GetProperty(""Instance"", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var method = audioManagerType.GetMethod(""Set"" + type + ""Volume"");
                    if (method != null)
                    {
                        method.Invoke(instance, new object[] { value });
                        return;
                    }
                }
            }
        }
        PlayerPrefs.SetFloat(type + ""Volume"", value);
    }
}
";
        }

        public static string GetCameraController2D()
        {
            return @"using UnityEngine;
using Unity.Cinemachine;

public class CameraController2D : MonoBehaviour
{
    private CinemachineCamera _vcam;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (_vcam != null)
        {
            _vcam.Follow = target;
        }
    }
}";
        }

        public static string GetCameraController3D()
        {
            return @"using UnityEngine;
using Unity.Cinemachine;

public class CameraController3D : MonoBehaviour
{
    private CinemachineCamera _vcam;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
    }

    public void SetTarget(Transform target)
    {
        if (_vcam != null)
        {
            _vcam.Follow = target;
            _vcam.LookAt = target;
        }
    }
}";
        }

        public static string GetSaveSlotManager()
        {
            return @"using System;
using System.IO;
using UnityEngine;

public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameSaveData LoadGame(int slotIndex)
    {
        string path = GetSaveFilePath(slotIndex);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($""Failed to load save from slot {slotIndex}: {ex.Message}"");
            return null;
        }
    }

    public void SaveGame(int slotIndex, GameSaveData data)
    {
        string path = GetSaveFilePath(slotIndex);
        try
        {
            data.timestamp = DateTime.UtcNow.ToString(""g"");
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($""Failed to save game in slot {slotIndex}: {ex.Message}"");
        }
    }

    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $""save_slot_{slotIndex}.json"");
    }
}

[Serializable]
public class GameSaveData
{
    public string chapterName = ""Chapter 1"";
    public string timestamp = """";
    public float playtimeSeconds = 0f;
    public int playerLevel = 1;
}";
        }

        public static string GetSaveSlotUI()
        {
            return @"using UnityEngine;
using UnityEngine.UIElements;

public class SaveSlotUI : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _root;
    private Button _backButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;
        _backButton = _root.Q<Button>(""saveslots-back-button"");

        if (_backButton != null)
        {
            _backButton.clicked += () => PanelManager.Instance?.PopPanel();
        }

        // Setup individual save slots (1 to 3/5)
        for (int i = 1; i <= 5; i++)
        {
            int index = i;
            var slotBtn = _root.Q<Button>($""slot-{index}-button"");
            if (slotBtn != null)
            {
                slotBtn.clicked += () => OnSlotSelected(index);
            }
        }
    }

    private void OnSlotSelected(int slotIndex)
    {
        GameSaveData data = SaveSlotManager.Instance?.LoadGame(slotIndex);
        if (data == null)
        {
            Debug.Log($""Starting new game in slot {slotIndex}"");
            GameSaveData newData = new GameSaveData();
            SaveSlotManager.Instance?.SaveGame(slotIndex, newData);
            SceneTransitionManager.Instance?.LoadScene(""GameScene"");
        }
        else
        {
            Debug.Log($""Loaded chapter {data.chapterName} in slot {slotIndex}"");
            SceneTransitionManager.Instance?.LoadScene(""GameScene"");
        }
    }
}";
        }

        public static string GetUXMLMainMenu()
        {
            return @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""False"">
    <Style src=""project://database/Assets/MainMenu1/UI/USS/MainMenu.uss?fileID=74334511186159674&amp;guid=0a927c3bc410427a92fb49c71b69446d&amp;type=3#MainMenu"" />
    <ui:VisualElement name=""root"" class=""menu-root"">
        <ui:VisualElement name=""fade-overlay"" class=""fade-overlay"" />
        
        <ui:VisualElement name=""main-menu"" class=""menu-panel"">
            <ui:Label text=""MY AAA GAME"" class=""menu-title"" />
            <ui:VisualElement class=""menu-buttons-container"">
                <ui:Button text=""CONTINUE"" name=""continue-button"" class=""menu-button"" />
                <ui:Button text=""PLAY"" name=""play-button"" class=""menu-button"" />
                <ui:Button text=""SETTINGS"" name=""settings-button"" class=""menu-button"" />
                <ui:Button text=""CREDITS"" name=""credits-button"" class=""menu-button"" />
                <ui:Button text=""QUIT"" name=""quit-button"" class=""menu-button"" />
            </ui:VisualElement>
        </ui:VisualElement>
 
        <ui:VisualElement name=""settings-panel"" class=""menu-panel"" style=""display: none;"">
            <ui:VisualElement name=""rebind-overlay"" class=""rebind-overlay"" style=""display: none;"">
                <ui:Label text=""PRESS ANY KEY..."" class=""rebind-overlay-text"" />
            </ui:VisualElement>

            <ui:Label text=""SETTINGS"" class=""menu-title"" />
            
            <ui:VisualElement class=""settings-tabs-container"">
                <ui:Button text=""AUDIO"" name=""tab-audio-btn"" class=""settings-tab-button settings-tab-button--active"" />
                <ui:Button text=""GRAPHICS"" name=""tab-graphics-btn"" class=""settings-tab-button"" />
                <ui:Button text=""CONTROLS"" name=""tab-controls-btn"" class=""settings-tab-button"" />
            </ui:VisualElement>

            <ui:VisualElement class=""settings-container"">
                <ui:VisualElement name=""tab-content-audio"" class=""settings-tab-content"">
                    <ui:Slider label=""Master Volume"" name=""master-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                    <ui:Slider label=""Music Volume"" name=""music-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                    <ui:Slider label=""SFX Volume"" name=""sfx-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                </ui:VisualElement>

                <ui:VisualElement name=""tab-content-graphics"" class=""settings-tab-content"" style=""display: none;"">
                    <ui:DropdownField label=""Resolution"" name=""resolution-dropdown"" class=""settings-dropdown"" />
                    <ui:Toggle label=""Fullscreen"" name=""fullscreen-toggle"" class=""settings-toggle"" />
                    <ui:DropdownField label=""Quality"" name=""quality-dropdown"" class=""settings-dropdown"" />
                    <ui:Toggle label=""VSync"" name=""vsync-toggle"" class=""settings-toggle"" />
                </ui:VisualElement>

                <ui:VisualElement name=""tab-content-controls"" class=""settings-tab-content"" style=""display: none;"">
                    <ui:VisualElement class=""controls-sub-tabs-container"">
                        <ui:Button text=""KEYBOARD"" name=""controls-sub-tab-keyboard-btn"" class=""controls-sub-tab-button controls-sub-tab-button--active"" />
                        <ui:Button text=""CONTROLLER"" name=""controls-sub-tab-controller-btn"" class=""controls-sub-tab-button"" />
                    </ui:VisualElement>
                    <ui:ScrollView name=""controls-keyboard-scroll"" class=""keybinds-scroll-view"" />
                    <ui:ScrollView name=""controls-controller-scroll"" class=""keybinds-scroll-view"" style=""display: none;"" />
                </ui:VisualElement>

                <ui:Button text=""BACK"" name=""settings-back-button"" class=""menu-button"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement name=""credits-panel"" class=""menu-panel"" style=""display: none;"">
            <ui:Label text=""CREDITS"" class=""menu-title"" />
            <ui:ScrollView class=""credits-scroll"">
                <ui:Label text=""DEVELOPED BY SHIVAM"" class=""credits-text"" />
                <ui:Label text=""HELP &amp; SUPPORT: lxcvam406@gmail.com"" class=""credits-text"" />
                <ui:Label text=""BUILT WITH UNITY 6"" class=""credits-text"" />
                <ui:Label text=""&quot;Imagination is the blueprint of reality.&quot;"" class=""credits-text"" style=""font-style: italic; color: #00ADB5; margin-top: 15px;"" />
            </ui:ScrollView>
            <ui:Button text=""BACK"" name=""credits-back-button"" class=""menu-button"" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>";
        }

        public static string GetUSSMainMenu()
        {
            return @".menu-root {
    flex-grow: 1;
    background-color: rgba(18, 18, 18, 0.95);
    justify-content: center;
    align-items: center;
    font-size: 20px;
    color: white;
}

.fade-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: black;
    opacity: 0;
    picking-mode: Ignore;
}

.menu-panel {
    width: 600px;
    padding: 40px;
    background-color: rgba(30, 30, 30, 0.9);
    border-width: 2px;
    border-color: rgba(100, 100, 100, 0.3);
    border-radius: 10px;
    align-items: center;
}

.menu-title {
    font-size: 48px;
    -unity-font-style: bold;
    color: #00ADB5;
    margin-bottom: 40px;
}

.menu-buttons-container {
    width: 100%;
    align-items: center;
}

.menu-button {
    width: 80%;
    padding: 12px;
    margin: 8px 0;
    font-size: 20px;
    -unity-font-style: bold;
    background-color: #222831;
    color: white;
    border-width: 1px;
    border-color: #393E46;
    border-radius: 5px;
    transition-duration: 0.15s;
}

.menu-button:hover {
    background-color: #00ADB5;
    color: #222831;
    border-color: #00ADB5;
}

.settings-container {
    width: 100%;
}

.settings-slider {
    margin: 15px 0;
}

.settings-toggle {
    margin: 15px 0;
}

.credits-scroll {
    height: 200px;
    width: 100%;
    margin-bottom: 20px;
}

.credits-text {
    font-size: 18px;
    margin: 5px 0;
    text-align: center;
}

.settings-tabs-container {
    flex-direction: row;
    justify-content: center;
    margin-bottom: 20px;
    width: 100%;
}

.settings-tab-button {
    flex-grow: 1;
    padding: 10px;
    font-size: 16px;
    background-color: #222831;
    color: white;
    border-width: 1px;
    border-color: #393E46;
    border-radius: 0;
    transition-duration: 0.15s;
}

.settings-tab-button:hover {
    background-color: #393E46;
}

.settings-tab-button--active {
    background-color: #00ADB5;
    color: #222831;
    border-color: #00ADB5;
    -unity-font-style: bold;
}

.settings-tab-content {
    width: 100%;
    margin-bottom: 20px;
}

.settings-dropdown {
    margin: 15px 0;
}

.controls-header {
    font-size: 18px;
    -unity-font-style: bold;
    margin-bottom: 10px;
    color: #00ADB5;
}

.keybinds-scroll-view {
    max-height: 250px;
    background-color: rgba(20, 20, 20, 0.5);
    padding: 10px;
    border-radius: 5px;
}

.controls-sub-tabs-container {
    flex-direction: row;
    justify-content: center;
    margin-bottom: 10px;
    width: 100%;
}

.controls-sub-tab-button {
    flex-grow: 1;
    padding: 6px;
    font-size: 14px;
    background-color: #222831;
    color: white;
    border-width: 1px;
    border-color: #393E46;
    border-radius: 0;
    transition-duration: 0.15s;
}

.controls-sub-tab-button:hover {
    background-color: #393E46;
}

.controls-sub-tab-button--active {
    background-color: #00ADB5;
    color: #222831;
    border-color: #00ADB5;
    -unity-font-style: bold;
}

.keybind-row {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom-width: 1px;
    border-bottom-color: rgba(255, 255, 255, 0.1);
}

.keybind-label {
    font-size: 14px;
    color: white;
    flex-grow: 1;
}

.keybind-buttons-container {
    flex-direction: row;
}

.keybind-button {
    width: 100px;
    margin-left: 5px;
    background-color: #393E46;
    color: white;
    border-width: 1px;
    border-color: #222831;
    border-radius: 3px;
    padding: 5px;
    font-size: 13px;
}

.keybind-button:hover {
    background-color: #00ADB5;
    color: #222831;
}

.keybind-button:disabled {
    background-color: #1a1a1a;
    color: #555555;
    border-color: #1a1a1a;
}

.rebind-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.85);
    justify-content: center;
    align-items: center;
    border-radius: 10px;
}

.rebind-overlay-text {
    font-size: 28px;
    -unity-font-style: bold;
    color: #00ADB5;
}";
        }
    }
}
