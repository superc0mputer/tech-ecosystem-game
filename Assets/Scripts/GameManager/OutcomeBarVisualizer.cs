using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OutcomeBarVisualizer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Transform segmentContainer; 
    [SerializeField] private GameObject segmentPrefab;   
    [SerializeField] private TextMeshProUGUI arrowText;  

    [Header("Separator")]
    [SerializeField] private GameObject separatorPrefab; 

    [Header("Configuration")]
    [SerializeField] private int maxSegments = 10;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.1f); 

    private List<Image> _segments = new List<Image>();
    private GameObject _separatorInstance; 

    public void Initialize(string resourceName, int oldValue, int newValue, Color themeColor)
    {
        labelText.text = resourceName;
        GenerateSegments();
        UpdateVisuals(oldValue, newValue, themeColor);
    }

    private void GenerateSegments()
    {
        // 1. Clear previous children
        foreach(Transform child in segmentContainer) Destroy(child.gameObject);
        _segments.Clear();
        _separatorInstance = null;

        // --- FIX: FORCE ALIGNMENT TO MIDDLE CENTER ---
        // This ensures the separator and squares are vertically centered relative to each other
        HorizontalLayoutGroup layout = segmentContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.childAlignment = TextAnchor.MiddleCenter;
            // Ensure the layout doesn't stretch them weirdly if they have different heights
            layout.childForceExpandHeight = false; 
            layout.childControlHeight = false;
        }
        // ---------------------------------------------

        // 2. Instantiate the 10 Segments
        for (int i = 0; i < maxSegments; i++)
        {
            GameObject obj = Instantiate(segmentPrefab, segmentContainer);
            _segments.Add(obj.GetComponent<Image>());
        }

        // 3. Instantiate the Separator & Place in Middle
        if (separatorPrefab != null)
        {
            _separatorInstance = Instantiate(separatorPrefab, segmentContainer);
            _separatorInstance.transform.SetSiblingIndex(maxSegments / 2); // 5 | 5
        }
    }

    private void UpdateVisuals(int oldVal, int newVal, Color themeColor)
    {
        oldVal = Mathf.Clamp(oldVal, 0, maxSegments);
        newVal = Mathf.Clamp(newVal, 0, maxSegments);

        // --- 1. SETUP COLORS ---
        Color ghostColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.35f); 

        // --- 2. SETUP ARROWS ---
        if (arrowText != null)
        {
            int diff = Mathf.Abs(newVal - oldVal);

            if (diff == 0) arrowText.text = "-";
            else if (newVal > oldVal) arrowText.text = new string('>', diff);
            else arrowText.text = new string('<', diff);
        }

        // --- 3. COLOR SEGMENTS ---
        bool isGain = newVal > oldVal;
        
        for (int i = 0; i < maxSegments; i++)
        {
            int index = i + 1; // 1-based index
            Image seg = _segments[i];

            if (isGain)
            {
                // GAIN SCENARIO
                if (index <= oldVal)
                    seg.color = themeColor; 
                else if (index > oldVal && index <= newVal)
                    seg.color = ghostColor; 
                else
                    seg.color = emptyColor;
            }
            else
            {
                // LOSS SCENARIO
                if (index <= newVal)
                    seg.color = themeColor; 
                else if (index > newVal && index <= oldVal)
                    seg.color = ghostColor; 
                else
                    seg.color = emptyColor;
            }
        }
    }
}