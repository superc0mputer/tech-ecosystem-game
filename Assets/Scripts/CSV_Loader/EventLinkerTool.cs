#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EventLinkerTool : EditorWindow
{
    private string stakeholderPath = "Assets/Data/Scriptable Objects/Stakeholders";
    private string eventPath = "Assets/Data/Scriptable Objects/Events";

    [MenuItem("Tools/Link Events to Stakeholders")]
    public static void ShowWindow()
    {
        GetWindow<EventLinkerTool>("Event Linker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Link Event Cards to Stakeholders", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Settings", EditorStyles.boldLabel);
        stakeholderPath = EditorGUILayout.TextField("Stakeholder Folder", stakeholderPath);
        eventPath = EditorGUILayout.TextField("Event Folder", eventPath);

        GUILayout.Space(20);

        if (GUILayout.Button("Link All Events"))
        {
            LinkEvents();
        }
    }

    private void LinkEvents()
    {
        // 1. Find all Assets
        List<StakeholderData> stakeholders = LoadAllAssets<StakeholderData>(stakeholderPath);
        List<EventCardData> events = LoadAllAssets<EventCardData>(eventPath);

        if (stakeholders.Count == 0 || events.Count == 0)
        {
            Debug.LogError("Could not find stakeholders or events. Check your folder paths.");
            return;
        }

        int linkedCount = 0;

        // 2. Clear old lists first (to avoid duplicates)
        foreach (var stakeholder in stakeholders)
        {
            Undo.RecordObject(stakeholder, "Link Events");
            stakeholder.associatedEvents.Clear();
        }

        // 3. Match Events to Stakeholders
        foreach (var card in events)
        {
            // Find the stakeholder where ID matches the Card's CharacterID
            // Using InvariantCultureIgnoreCase to be safe with capitalization
            var owner = stakeholders.FirstOrDefault(s => s.id.Trim().Equals(card.characterId.Trim(), System.StringComparison.InvariantCultureIgnoreCase));

            if (owner != null)
            {
                owner.associatedEvents.Add(card);
                EditorUtility.SetDirty(owner); // Mark as changed so Unity saves it
                linkedCount++;
            }
            else
            {
                Debug.LogWarning($"Event '{card.name}' has Character ID '{card.characterId}', but no matching Stakeholder was found.");
            }
        }

        // 4. Save Changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>SUCCESS:</color> Linked {linkedCount} events to {stakeholders.Count} stakeholders.");
    }

    // Helper to load all assets of type T from a specific folder
    private List<T> LoadAllAssets<T>(string folderPath) where T : ScriptableObject
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
}
#endif
