using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    public string sceneToLoad = "GameScene";
    public string menuAlignment = "Middle Center";

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

    private void Start()
    {
        PanelManager.Instance.ShowPanel("main-menu");
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