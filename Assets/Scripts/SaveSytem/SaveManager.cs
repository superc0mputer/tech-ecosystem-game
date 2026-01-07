using UnityEngine;
using System.Linq; 
using Newtonsoft.Json; 
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

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
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        saveData.timestamp = System.DateTime.Now.ToString();
        
        // 1. Find all objects that have the SaveableEntity script
        var saveables = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        // 2. Loop through them and ask for their data
        foreach (var saveable in saveables)
        {
            ISaveable saveableScript = saveable.GetComponent<ISaveable>();
            if (saveableScript != null)
            {
                saveData.objectsData[saveable.Id] = saveableScript.CaptureState();
            }
        }

        // 3. Write to file
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game Saved to: " + SavePath);
    }

    public void LoadGame()
    {
        if (!HasSaveFile()) return;

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json);

        var saveables = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);

        foreach (var saveable in saveables)
        {
            if (saveData.objectsData.TryGetValue(saveable.Id, out object data))
            {
                ISaveable saveableScript = saveable.GetComponent<ISaveable>();
                // RestoreState handles the casting internally in the specific scripts
                saveableScript.RestoreState(data); 
            }
        }
        Debug.Log("Game Loaded.");
    }
}