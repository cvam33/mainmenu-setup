using System.Collections.Generic;
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
        if (MainMenuManager.Instance != null && MainMenuManager.Instance.gameObject != gameObject)
        {
            return;
        }

        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null)
        {
            _root = _uiDocument.rootVisualElement;
            if (_root != null)
            {
                // Find all panels (by convention, elements with class 'menu-panel')
                _root.Query<VisualElement>(className: "menu-panel").ForEach(panel =>
                {
                    _allPanels.Add(panel);
                    panel.style.display = DisplayStyle.None;
                });
            }
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
}