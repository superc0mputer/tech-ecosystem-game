using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OutcomePanelController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject outcomeBarPrefab; 
    [SerializeField] private Transform container; 

    [Header("Resource Colors")]
    [SerializeField] private List<ResourceColorConfig> resourceColors = new List<ResourceColorConfig>();

    [System.Serializable]
    public struct ResourceColorConfig
    {
        public string id;      // e.g. "Industry"
        public Color color;    // e.g. Orange
    }

    public void GenerateOutcomeBars(ResourceManager.ResourceSaveData oldStats, ResourceManager currentStats)
    {
        // 1. Clear previous bars
        foreach(Transform child in container) 
        {
            if (child.gameObject != this.gameObject) 
                Destroy(child.gameObject);
        }

        // 2. Generate new bars if values changed
        CreateBarIfChanged("Industry",      oldStats.industry,   currentStats.industryVal);
        CreateBarIfChanged("Civil Society", oldStats.civil,      currentStats.civilVal);
        CreateBarIfChanged("Governance",    oldStats.governance, currentStats.governanceVal);
        CreateBarIfChanged("Innovation",    oldStats.innovation, currentStats.innovationVal);

        // 3. FORCE REBUILD LAYOUT (Crucial Fix for "Nothing Showing Up")
        // This forces Unity to recalculate the height of the container immediately
        LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
    }

    private void CreateBarIfChanged(string name, int oldVal, int newVal)
    {
        if (oldVal == newVal) return; 

        Color useColor = Color.white; 
        foreach(var config in resourceColors)
        {
            if (config.id == name) 
            {
                useColor = config.color;
                break;
            }
        }

        GameObject obj = Instantiate(outcomeBarPrefab, container);
        var vis = obj.GetComponent<OutcomeBarVisualizer>();
        if (vis != null)
        {
            vis.Initialize(name, oldVal, newVal, useColor);
        }
    }
}