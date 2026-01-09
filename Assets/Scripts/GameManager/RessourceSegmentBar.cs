using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResourceSegmentBar : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject segmentPrefab; // A single Image (White Square)
    [SerializeField] private Transform container;      // Object with HorizontalLayoutGroup
    [SerializeField] private int maxSegments = 10;

    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(1f, 0.8f, 0.4f, 1f);   // Yellow (Current)
    [SerializeField] private Color emptyColor  = new Color(1f, 1f, 1f, 0.2f);     // Faded (Empty)
    [SerializeField] private Color previewColor = new Color(0.4f, 0.7f, 1f, 1f);  // Light Blue (The Change)

    private List<Image> segments = new List<Image>();

    private void Awake()
    {
        InitializeSegments();
    }

    private void InitializeSegments()
    {
        // Clear existing children if any (mostly for editor safety)
        foreach (Transform child in container) 
        {
            if(Application.isPlaying) Destroy(child.gameObject);
        }
        segments.Clear();

        // Spawn new segments
        for (int i = 0; i < maxSegments; i++)
        {
            GameObject newObj = Instantiate(segmentPrefab, container);
            Image img = newObj.GetComponent<Image>();
            segments.Add(img);
        }
    }

    /// <summary>
    /// Updates the visual state of the bar.
    /// </summary>
    /// <param name="currentVal">The current resource value (0-10)</param>
    /// <param name="predictedChange">The previewed change (e.g., +2 or -2). 0 for no preview.</param>
    public void UpdateVisuals(int currentVal, int predictedChange = 0)
    {
        int futureVal = Mathf.Clamp(currentVal + predictedChange, 0, maxSegments);

        for (int i = 0; i < segments.Count; i++)
        {
            // Logic Index: +1 because Loop 0 represents the 1st box.
            int currentSegment = i + 1; 

            // DEFAULT STATE:
            if (currentSegment <= currentVal)
                segments[i].color = activeColor;
            else
                segments[i].color = emptyColor;

            // PREVIEW OVERLAY (The Blue Highlight):
            if (predictedChange != 0)
            {
                // Case A: Gaining Resource (Green/Blue zone)
                // Highlight the gap between current and future
                if (predictedChange > 0)
                {
                    if (currentSegment > currentVal && currentSegment <= futureVal)
                    {
                        segments[i].color = previewColor;
                    }
                }
                // Case B: Losing Resource (Red/Blue zone)
                // Highlight the segments we are about to lose
                else if (predictedChange < 0)
                {
                    if (currentSegment > futureVal && currentSegment <= currentVal)
                    {
                        segments[i].color = previewColor;
                    }
                }
            }
        }
    }
}