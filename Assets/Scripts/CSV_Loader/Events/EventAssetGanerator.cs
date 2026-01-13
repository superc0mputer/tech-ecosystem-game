using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class EventAssetGenerator : EditorWindow
{
    private TextAsset csvFile;
    private string targetFolder = "Assets/Data/Scriptable Objects/Events";

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
            if (csvFile != null) CreateAssets();
            else Debug.LogError("Please assign the Event CSV file.");
        }
    }

    private void CreateAssets()
    {
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
            
            // The new CSV has 20 columns. 
            // If a line is empty or malformed, skip it.
            if (cells.Length < 20) 
            {
                Debug.LogWarning($"Skipping line {i} (ID: {(cells.Length > 0 ? cells[0] : "null")}): Insufficient columns ({cells.Length}).");
                continue;
            }

            string id = cells[0];
            string sanitizedId = id.Replace(":", "").Replace("/", "_");
            string fileName = $"{sanitizedId}.asset"; // Removed "Event_" prefix if ID already has it (e.g. evt_marcus)
            string fullPath = $"{targetFolder}/{fileName}";

            EventCardData asset = AssetDatabase.LoadAssetAtPath<EventCardData>(fullPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EventCardData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            Undo.RecordObject(asset, "Update Event Data");

            // -- Core Info --
            asset.id = cells[0];
            asset.characterId = cells[1];
            asset.title = cells[2];
            asset.bodyText = cells[3];

            // -- Choice A --
            ChoiceData cA = new ChoiceData();
            cA.label = cells[4];
            cA.flavor = cells[5];
            
            // Map Stats A (Indices 6-9)
            cA.effects = new StatBlock();
            cA.effects.industry = ParseInt(cells[6]);
            cA.effects.civilSociety = ParseInt(cells[7]);
            cA.effects.governance = ParseInt(cells[8]);
            cA.effects.innovation = ParseInt(cells[9]);
            
            // New Outcome fields (Indices 10-11)
            cA.outcomeTitle = cells[10]; 
            cA.outcomeText = cells[11];
            
            asset.choiceA = cA;

            // -- Choice B --
            ChoiceData cB = new ChoiceData();
            cB.label = cells[12];
            cB.flavor = cells[13];

            // Map Stats B (Indices 14-17)
            cB.effects = new StatBlock();
            cB.effects.industry = ParseInt(cells[14]);
            cB.effects.civilSociety = ParseInt(cells[15]);
            cB.effects.governance = ParseInt(cells[16]);
            cB.effects.innovation = ParseInt(cells[17]);

            // New Outcome fields (Indices 18-19)
            cB.outcomeTitle = cells[18];
            cB.outcomeText = cells[19];
            
            asset.choiceB = cB;

            EditorUtility.SetDirty(asset);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully generated/updated {count} Event assets in {targetFolder}");
    }

    private int ParseInt(string s) => int.TryParse(s, out int v) ? v : 0;

    private string[] SplitCsvLine(string line)
    {
        // Regex handles commas inside quotes correctly
        string pattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        string[] tokens = Regex.Split(line, pattern);

        for (int i = 0; i < tokens.Length; i++)
        {
            string val = tokens[i].Trim();
            // Remove wrapping quotes
            if (val.Length >= 2 && val.StartsWith("\"") && val.EndsWith("\""))
            {
                val = val.Substring(1, val.Length - 2);
            }
            // Unescape double quotes ("" -> ")
            val = val.Replace("\"\"", "\"");
            tokens[i] = val;
        }
        return tokens;
    }
}