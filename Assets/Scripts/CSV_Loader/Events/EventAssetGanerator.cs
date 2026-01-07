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
            
            // We now have 18 columns with Outcome added back
            if (cells.Length < 18) continue;

            string id = cells[0];
            string sanitizedId = id.Replace(":", "").Replace("/", "_");
            string fileName = $"Event_{sanitizedId}.asset";
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
            
            // Map Columns 6-9 to Stats
            cA.effects = new StatBlock();
            cA.effects.industry = ParseInt(cells[6]);
            cA.effects.civilSociety = ParseInt(cells[7]);
            cA.effects.governance = ParseInt(cells[8]);
            cA.effects.innovation = ParseInt(cells[9]);
            
            cA.outcome = cells[10]; // New Column 10
            asset.choiceA = cA;

            // -- Choice B --
            ChoiceData cB = new ChoiceData();
            cB.label = cells[11];
            cB.flavor = cells[12];

            // Map Columns 13-16 to Stats
            cB.effects = new StatBlock();
            cB.effects.industry = ParseInt(cells[13]);
            cB.effects.civilSociety = ParseInt(cells[14]);
            cB.effects.governance = ParseInt(cells[15]);
            cB.effects.innovation = ParseInt(cells[16]);

            cB.outcome = cells[17]; // New Column 17
            asset.choiceB = cB;

            EditorUtility.SetDirty(asset);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully generated {count} Event assets in {targetFolder}");
    }

    private int ParseInt(string s) => int.TryParse(s, out int v) ? v : 0;

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