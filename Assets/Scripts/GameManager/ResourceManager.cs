using UnityEngine;
using Newtonsoft.Json.Linq; 

public class ResourceManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public GameUIController uiController; 

    [Header("Current Values (0-10)")]
    public int industryVal;
    public int civilVal;
    public int governanceVal;
    public int innovationVal;

    public void InitializeResources()
    {
        // Start in the middle
        industryVal = 5;
        civilVal = 5;
        governanceVal = 5;
        innovationVal = 5;
        UpdateUI();
    }

    public void ApplyEffects(StatBlock effects)
    {
        industryVal   += effects.industry;
        civilVal      += effects.civilSociety;
        governanceVal += effects.governance;
        innovationVal += effects.innovation;

        // Note: We do NOT clamp here immediately if we want to detect the "Game Over" 
        // condition of hitting 10 or 0 precisely. 
        // But for safety, let's keep them within bounds 0-10 so the UI doesn't break.
        industryVal   = Mathf.Clamp(industryVal, 0, 10);
        civilVal      = Mathf.Clamp(civilVal, 0, 10);
        governanceVal = Mathf.Clamp(governanceVal, 0, 10);
        innovationVal = Mathf.Clamp(innovationVal, 0, 10);

        UpdateUI();
        Debug.Log($"Stats: Ind:{industryVal} Civ:{civilVal} Gov:{governanceVal} Inn:{innovationVal}");
    }

    // --- UPDATED GAME OVER LOGIC ---
    public bool CheckGameOverCondition()
    {
        // Game Over if ANY value hits 0 (Collapse) OR 10 (Extreme)
        if (industryVal <= 0 || industryVal >= 10) return true;
        if (civilVal <= 0    || civilVal >= 10)    return true;
        if (governanceVal <= 0 || governanceVal >= 10) return true;
        if (innovationVal <= 0 || innovationVal >= 10) return true;

        return false;
    }

    private void UpdateUI()
    {
        if (uiController == null) return;
        uiController.UpdateResourceDisplay("Industry", industryVal);
        uiController.UpdateResourceDisplay("Civil Society", civilVal);
        uiController.UpdateResourceDisplay("Governance", governanceVal);
        uiController.UpdateResourceDisplay("Innovation", innovationVal);
    }

    // --- SAVE SYSTEM ---
    [System.Serializable]
    private struct ResourceSaveData
    {
        public int industry;
        public int civil;
        public int governance;
        public int innovation;
    }

    public object CaptureState()
    {
        return new ResourceSaveData
        {
            industry = industryVal,
            civil = civilVal,
            governance = governanceVal,
            innovation = innovationVal
        };
    }

    public void RestoreState(object state)
    {
        var data = ((JObject)state).ToObject<ResourceSaveData>();
        this.industryVal = data.industry;
        this.civilVal = data.civil;
        this.governanceVal = data.governance;
        this.innovationVal = data.innovation;
        UpdateUI(); 
    }
}