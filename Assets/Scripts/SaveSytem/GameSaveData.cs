using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    // A dictionary is perfect here:
    // Key = A unique ID for that object (string)
    // Value = The data that object saved (object)
    public Dictionary<string, object> objectsData = new Dictionary<string, object>();
    
    public string sceneName;
    public string timestamp;
}