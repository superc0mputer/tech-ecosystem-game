using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class EventAssetGenerator : EditorWindow
{
    private TextAsset csvFile;
    private string targetFolder = "Assets/Data/Scriptable Objects/Events"; // Output folder

    [MenuItem("Tools/Generate Event Assets")]
    public static void ShowWindow()
    {
        GetWindow<EventAssetGenerator>("Event Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Individual Event Cards", EditorStyles.boldLabel);
        
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        
        GUILayout.Space(10);
        GUILayout.Label($"Target Folder: {targetFolder}", EditorStyles.miniLabel);

        if (GUILayout.Button("Generate Events"))
        {
            if (csvFile != null)
            {
                CreateAssets();
            }
            else
            {
                Debug.LogError("Please assign the Event CSV file.");
            }
        }
    }

    private void CreateAssets()
    {
        // Create folder if it doesn't exist
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
            AssetDatabase.Refresh();
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        int count = 0;

        // Skip Header (i=1)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cells = SplitCsvLine(lines[i]);
            
            // Safety check: ensure we have enough columns (12 based on your previous schema)
            if (cells.Length < 12) continue;

            string id = cells[0];
            
            // File Name: e.g., "Event_evt_marcus_01.asset"
            // We sanitize the ID to ensure it's a valid filename
            string sanitizedId = id.Replace(":", "").Replace("/", "_");
            string fileName = $"Event_{sanitizedId}.asset";
            string fullPath = $"{targetFolder}/{fileName}";

            // 1. Check if asset already exists
            EventCardData asset = AssetDatabase.LoadAssetAtPath<EventCardData>(fullPath);

            // 2. If not, create it
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EventCardData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            // 3. Populate Data
            Undo.RecordObject(asset, "Update Event Data");

            // -- Core Info --
            asset.id = cells[0];
            asset.characterId = cells[1];
            asset.title = cells[2];
            asset.bodyText = cells[3];

            // -- Choice A --
            // We must create a new struct (or modify the existing one)
            ChoiceData cA = new ChoiceData();
            cA.label = cells[4];
            cA.flavor = cells[5];
            cA.effects = cells[6];
            cA.result = cells[7];
            asset.choiceA = cA;

            // -- Choice B --
            ChoiceData cB = new ChoiceData();
            cB.label = cells[8];
            cB.flavor = cells[9];
            cB.effects = cells[10];
            cB.result = cells[11];
            asset.choiceB = cB;

            EditorUtility.SetDirty(asset);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully generated/updated {count} Event assets in {targetFolder}");
    }

    // Standard CSV Splitter (handles quotes)
    private string[] SplitCsvLine(string line)
    {
        string pattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        string[] tokens = Regex.Split(line, pattern);

        for (int i = 0; i < tokens.Length; i++)
        {
            string val = tokens[i].Trim();
            if (val.Length >= 2 && val.StartsWith("\"") && val.EndsWith("\""))
            {
                val = val.Substring(1, val.Length - 2);
            }
            val = val.Replace("\"\"", "\"");
            tokens[i] = val;
        }
        return tokens;
    }
}