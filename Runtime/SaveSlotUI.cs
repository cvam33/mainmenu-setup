using UnityEngine;
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
        _backButton = _root.Q<Button>("saveslots-back-button");

        if (_backButton != null)
        {
            _backButton.clicked += () => PanelManager.Instance?.PopPanel();
        }

        // Setup individual save slots (1 to 3/5)
        for (int i = 1; i <= 5; i++)
        {
            int index = i;
            var slotBtn = _root.Q<Button>($"slot-{index}-button");
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
            Debug.Log($"Starting new game in slot {slotIndex}");
            GameSaveData newData = new GameSaveData();
            SaveSlotManager.Instance?.SaveGame(slotIndex, newData);
            SceneTransitionManager.Instance?.LoadScene("GameScene");
        }
        else
        {
            Debug.Log($"Loaded chapter {data.chapterName} in slot {slotIndex}");
            SceneTransitionManager.Instance?.LoadScene("GameScene");
        }
    }
}