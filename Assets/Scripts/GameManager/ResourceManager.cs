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

        industryVal   = Mathf.Clamp(industryVal, 0, 10);
        civilVal      = Mathf.Clamp(civilVal, 0, 10);
        governanceVal = Mathf.Clamp(governanceVal, 0, 10);
        innovationVal = Mathf.Clamp(innovationVal, 0, 10);

        UpdateUI();
        Debug.Log($"Values Updated: Ind:{industryVal} Civ:{civilVal} Gov:{governanceVal} Inn:{innovationVal}");
    }

    public bool CheckGameOverCondition()
    {
        return (industryVal <= 0 || civilVal <= 0 || governanceVal <= 0 || innovationVal <= 0);
    }

    private void UpdateUI()
    {
        if (uiController == null) return;

        // UI: Update the filled images
        uiController.UpdateResourceDisplay("Industry", industryVal);
        uiController.UpdateResourceDisplay("Civil Society", civilVal);
        uiController.UpdateResourceDisplay("Governance", governanceVal);
        uiController.UpdateResourceDisplay("Innovation", innovationVal);
    }

    // --- SAVE SYSTEM IMPLEMENTATION ---

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
        
        UpdateUI(); // UI: Refresh UI after load
    }
}