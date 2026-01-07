using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Current Values (0-10)")]
    public int industryVal;
    public int civilVal;
    public int governanceVal;
    public int innovationVal;

    // Start values usually start in the middle (5)
    public void InitializeResources()
    {
        industryVal = 5;
        civilVal = 5;
        governanceVal = 5;
        innovationVal = 5;
        UpdateUI(); // Placeholder for UI update
    }

    public void ApplyEffects(StatBlock effects)
    {
        // Add the new values to the current ones
        industryVal   += effects.industry;
        civilVal      += effects.civilSociety;
        governanceVal += effects.governance;
        innovationVal += effects.innovation;

        // Clamp them to ensure they stay between 0 and 10
        industryVal   = Mathf.Clamp(industryVal, 0, 10);
        civilVal      = Mathf.Clamp(civilVal, 0, 10);
        governanceVal = Mathf.Clamp(governanceVal, 0, 10);
        innovationVal = Mathf.Clamp(innovationVal, 0, 10);

        UpdateUI();
        Debug.Log($"Values Updated: Ind:{industryVal} Civ:{civilVal} Gov:{governanceVal} Inn:{innovationVal}");
    }

    // Returns TRUE if any value hit 0 (Lose Condition)
    public bool CheckGameOverCondition()
    {
        return (industryVal <= 0 || civilVal <= 0 || governanceVal <= 0 || innovationVal <= 0);
    }

    private void UpdateUI()
    {
        // TODO: Connect this to your UI Sliders later
    }
}
