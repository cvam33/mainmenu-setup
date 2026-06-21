using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    public string sceneToLoad = "GameScene";
    public string menuAlignment = "Middle Center";

    private bool _isInGame = false;
    private bool _isPaused = false;
    public bool IsInGame => _isInGame;

    private UIDocument _uiDocument;
    private VisualElement _root;
    
    private Button _playButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _quitButton;
    private Button _creditsBackButton;

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
        if (_uiDocument == null) return;
        
        _root = _uiDocument.rootVisualElement;
        
        var menuRoot = _root.Q<VisualElement>("root");
        if (menuRoot != null)
        {
            string alignClass = "menu-root--" + menuAlignment.ToLower().Replace(" ", "-");
            menuRoot.AddToClassList(alignClass);
        }
        
        _playButton = _root.Q<Button>("play-button");
        _settingsButton = _root.Q<Button>("settings-button");
        _creditsButton = _root.Q<Button>("credits-button");
        _quitButton = _root.Q<Button>("quit-button");
        _creditsBackButton = _root.Q<Button>("credits-back-button");

        if (_playButton != null) _playButton.clicked += OnPlayClicked;
        if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
        if (_creditsButton != null) _creditsButton.clicked += OnCreditsClicked;
        if (_quitButton != null) _quitButton.clicked += OnQuitClicked;
        if (_creditsBackButton != null) _creditsBackButton.clicked += OnCreditsBackClicked;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PanelManager.Instance?.ShowPanel("main-menu");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            _isInGame = false;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                PanelManager.Instance?.ShowPanel("main-menu");
                
                var menuRoot = _root.Q<VisualElement>("root");
                if (menuRoot != null)
                {
                    menuRoot.RemoveFromClassList("menu-root--paused");
                }
                ConfigureSettingsButtons(false);
            }
            ToggleVolumeBlur(false);
        }
        else
        {
            _isInGame = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
                ConfigureSettingsButtons(true);
            }
        }
    }

    private void ConfigureSettingsButtons(bool inGame)
    {
        if (_root == null) return;
        var settingsPanel = _root.Q<VisualElement>("settings-panel");
        if (settingsPanel != null)
        {
            var mainMenuBtn = settingsPanel.Q<Button>("settings-main-menu-button");
            if (mainMenuBtn != null)
            {
                mainMenuBtn.style.display = inGame ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var backBtn = settingsPanel.Q<Button>("settings-back-button");
            if (backBtn != null)
            {
                backBtn.text = inGame ? "RESUME" : "BACK";
            }
        }
    }

    private void Update()
    {
        if (_isInGame)
        {
            bool pausePressed = false;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                pausePressed = true;
            }
            if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.selectButton.wasPressedThisFrame))
            {
                pausePressed = true;
            }

            if (pausePressed)
            {
                TogglePause();
            }
        }
    }


    public void TogglePause()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
        {
            Time.timeScale = 0f;
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
                PanelManager.Instance?.ShowPanel("settings-panel");
                
                var menuRoot = _root.Q<VisualElement>("root");
                if (menuRoot != null)
                {
                    menuRoot.AddToClassList("menu-root--paused");
                }
            }
            ToggleVolumeBlur(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
                
                var menuRoot = _root.Q<VisualElement>("root");
                if (menuRoot != null)
                {
                    menuRoot.RemoveFromClassList("menu-root--paused");
                }
            }
            ToggleVolumeBlur(false);
        }
    }

    private void ToggleVolumeBlur(bool active)
    {
        var volumes = FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None);
        foreach (var vol in volumes)
        {
            if (vol.sharedProfile != null)
            {
                if (vol.sharedProfile.TryGet<UnityEngine.Rendering.Universal.DepthOfField>(out var dof))
                {
                    dof.active = active;
                }
            }
        }
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play Clicked!");
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("SceneTransitionManager.Instance is null!");
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings Clicked!");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PushPanel("settings-panel");
        }
        else
        {
            Debug.LogError("PanelManager.Instance is null!");
        }
    }

    private void OnCreditsClicked()
    {
        Debug.Log("Credits Clicked!");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PushPanel("credits-panel");
        }
        else
        {
            Debug.LogError("PanelManager.Instance is null!");
        }
    }

    private void OnCreditsBackClicked()
    {
        Debug.Log("Credits Back Clicked!");
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.PopPanel();
        }
        else
        {
            Debug.LogError("PanelManager.Instance is null!");
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit Clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}