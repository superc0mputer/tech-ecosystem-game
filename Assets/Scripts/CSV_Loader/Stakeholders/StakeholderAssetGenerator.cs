using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class StakeholderAssetGenerator : EditorWindow
{
    private TextAsset csvFile;
    private string targetFolder = "Assets/Data/Scriptable Objects/Stakeholders"; // Change this if needed

    [MenuItem("Tools/Generate Stakeholder Assets")]
    public static void ShowWindow()
    {
        GetWindow<StakeholderAssetGenerator>("Stakeholder Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Individual Assets", EditorStyles.boldLabel);
        
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        
        GUILayout.Space(10);
        GUILayout.Label($"Target Folder: {targetDatabasePath()}", EditorStyles.miniLabel);

        if (GUILayout.Button("Generate Assets"))
        {
            if (csvFile != null)
            {
                CreateAssets();
            }
            else
            {
                Debug.LogError("Please assign a CSV file.");
            }
        }
    }

    private string targetDatabasePath()
    {
        // Ensure folder exists logic could go here, for now we assume it exists or create it
        return targetFolder;
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
            if (cells.Length < 12) continue;

            string id = cells[0];
            string fileName = $"Stakeholder_{cells[4]}_{cells[5]}.asset"; // e.g., Stakeholder_Marcus_Sterling.asset
            string fullPath = $"{targetFolder}/{fileName}";

            // 1. Check if asset already exists
            StakeholderData asset = AssetDatabase.LoadAssetAtPath<StakeholderData>(fullPath);

            // 2. If not, create it
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StakeholderData>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            // 3. Populate Data
            Undo.RecordObject(asset, "Update Stakeholder Data");

            asset.id = id;
            asset.group = cells[1];
            asset.archetype = cells[2];
            asset.displayName = cells[3];
            asset.firstName = cells[4];
            asset.lastName = cells[5];
            asset.goal = cells[6];
            asset.intrinsicNeed = cells[7];
            asset.extrinsicNeed = cells[8];
            asset.uniqueTension = cells[9];
            asset.headAddress = cells[10];
            asset.bodyAddress = cells[11];

            EditorUtility.SetDirty(asset);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully generated/updated {count} Stakeholder assets in {targetFolder}");
    }

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