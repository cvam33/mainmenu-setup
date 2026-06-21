using System;
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
            Debug.LogError($"Failed to load save from slot {slotIndex}: {ex.Message}");
            return null;
        }
    }

    public void SaveGame(int slotIndex, GameSaveData data)
    {
        string path = GetSaveFilePath(slotIndex);
        try
        {
            data.timestamp = DateTime.UtcNow.ToString("g");
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save game in slot {slotIndex}: {ex.Message}");
        }
    }

    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }
}

[Serializable]
public class GameSaveData
{
    public string chapterName = "Chapter 1";
    public string timestamp = "";
    public float playtimeSeconds = 0f;
    public int playerLevel = 1;
}