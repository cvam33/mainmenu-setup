using System.Collections;
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
                _fadeOverlay = root.Q("fade-overlay");
                if (_fadeOverlay == null)
                {
                    _fadeOverlay = new VisualElement();
                    _fadeOverlay.name = "fade-overlay";
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
}