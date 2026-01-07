using UnityEngine;
using System.Linq; // for FindObjectsByType
using Newtonsoft.Json; 
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance; // Singleton for easy access

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        
        // 1. Find all objects that have the SaveableEntity script
        var saveables = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        // 2. Loop through them and ask for their data
        foreach (var saveable in saveables)
        {
            // Get the script that actually holds the logic (Player, Enemy, etc.)
            ISaveable saveableScript = saveable.GetComponent<ISaveable>();
            
            if (saveableScript != null)
            {
                // Store their data in the dictionary using their Unique ID
                saveData.objectsData[saveable.Id] = saveableScript.CaptureState();
            }
        }

        // 3. Write to file (using the method from the previous response)
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savegame.json"), json);
    }

    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json);

        // 1. Find all objects in the scene
        var saveables = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        // 2. Distribute data back to them
        foreach (var saveable in saveables)
        {
            if (saveData.objectsData.TryGetValue(saveable.Id, out object data))
            {
                ISaveable saveableScript = saveable.GetComponent<ISaveable>();
                
                // IMPORTANT: JSON often loads as a generic JObject, so we might need to cast it
                // This part depends on your JSON library settings, but conceptual logic applies
                saveableScript.RestoreState(data); 
            }
        }
    }
}