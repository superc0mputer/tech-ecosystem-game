using UnityEngine;
using Newtonsoft.Json.Linq; 

public class ResourceManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public GameUIController uiController; 

    [Header("Current Values (0-10)")]
    public int industryVal = 5;
    public int civilVal = 5;
    public int governanceVal = 5;
    public int innovationVal = 5;

    public void InitializeResources()
    {
        // Only reset if we are NOT loading (handled by GameLoop)
        // Default values set above
        UpdateUI();
    }

    public void ApplyEffects(StatBlock effects)
    {
        industryVal   = Mathf.Clamp(industryVal + effects.industry, 0, 10);
        civilVal      = Mathf.Clamp(civilVal + effects.civilSociety, 0, 10);
        governanceVal = Mathf.Clamp(governanceVal + effects.governance, 0, 10);
        innovationVal = Mathf.Clamp(innovationVal + effects.innovation, 0, 10);
        UpdateUI();
    }

    public bool CheckGameOverCondition()
    {
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

    // CHANGE 1: Made Public so GameLoopManager can see it clearly
    [System.Serializable]
    public struct ResourceSaveData
    {
        public int industry;
        public int civil;
        public int governance;
        public int innovation;
    }

    public object CaptureState()
    {
        Debug.Log($"[Resource] Saving: {industryVal}, {civilVal}, {governanceVal}, {innovationVal}");
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
        // CHANGE 2: Safer conversion using JToken
        JToken token = state as JToken;
        if (token != null)
        {
            var data = token.ToObject<ResourceSaveData>();
            this.industryVal = data.industry;
            this.civilVal = data.civil;
            this.governanceVal = data.governance;
            this.innovationVal = data.innovation;
            
            Debug.Log($"[Resource] Restored: {industryVal}, {civilVal}...");
            UpdateUI(); 
        }
        else
        {
            Debug.LogError("[Resource] Failed to restore state: Data was null or wrong type.");
        }
    }
}