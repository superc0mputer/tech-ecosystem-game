using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AddressableAssets; 
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameUIController : MonoBehaviour
{
    [Header("--- PANELS ---")]
    [SerializeField] private GameObject panelStakeholders;
    [SerializeField] private GameObject panelTop;
    [SerializeField] private GameObject panelGameplay;

    [Header("--- OUTCOME POPUP ---")]
    [SerializeField] private GameObject panelOutcomeRoot; 
    [SerializeField] private TextMeshProUGUI outcomeExplanationText; 
    [SerializeField] private TextMeshProUGUI outcomeTitleText; 
    [SerializeField] private OutcomePanelController outcomeBarsController; 

    // NEW: References specifically for the Card inside the Outcome Panel
    [Header("--- OUTCOME CARD ---")]
    [SerializeField] private OutcomeCardRefs outcomeCard;

    [System.Serializable]
    public struct OutcomeCardRefs
    {
        public TextMeshProUGUI nameText;
        public Image bodyshotImage;
    }

    [Header("--- TOP PANEL ---")]
    [SerializeField] private TextMeshProUGUI roundNumber; 
    [SerializeField] private TextMeshProUGUI topFlavorText; 

    [Header("--- MAIN GAMEPLAY CARD ---")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardBodyshot;
    [SerializeField] private TextMeshProUGUI cardName;

    [Header("--- CARD VISUALS (SHARED) ---")]
    [SerializeField] private Image cardGlow; 
    [SerializeField] private List<GroupColorDefinition> groupColors = new List<GroupColorDefinition>();

    [System.Serializable]
    public struct GroupColorDefinition
    {
        public string id;       
        public Color glowColor; 
    }

    [Header("--- OPTIONS ---")]
    [SerializeField] private GameObject panelOptionAObj;
    [SerializeField] private GameObject panelOptionBObj;
    
    [SerializeField] public CanvasGroup optionACanvasGroup; 
    [SerializeField] public CanvasGroup optionBCanvasGroup; 

    [SerializeField] private TextMeshProUGUI optionATitle;
    [SerializeField] private TextMeshProUGUI optionAFlavor;
    [SerializeField] private TextMeshProUGUI optionBTitle;
    [SerializeField] private TextMeshProUGUI optionBFlavor;

    [Header("--- STAKEHOLDERS ---")]
    [SerializeField] private List<StakeholderSlot> stakeholders = new List<StakeholderSlot>();

    [System.Serializable]
    public class StakeholderSlot
    {
        public string id; 
        public GameObject groupParent;
        public Image background;
        public Image headshot;
        public TextMeshProUGUI nameText;
        public ResourceSegmentBar segmentBar; 
    }

    private void Awake()
    {
        AutoWireUI();
    }

    private void AutoWireUI()
    {
        // Existing Wiring
        panelStakeholders = transform.Find("Panel Stakeholders")?.gameObject;
        panelTop = transform.Find("Panel Top")?.gameObject;
        panelGameplay = transform.Find("Panel Gameplay")?.gameObject; 

        if(panelTop != null)
        {
            Transform roundObj = panelTop.transform.Find("Round");
            if(roundObj) roundNumber = roundObj.GetComponentInChildren<TextMeshProUGUI>();
            topFlavorText = panelTop.transform.Find("Flavor/Flavor Text")?.GetComponent<TextMeshProUGUI>();
        }

        if(panelGameplay != null)
        {
            GameObject panelCard = panelGameplay.transform.Find("Panel Card")?.gameObject;
            panelOptionAObj = panelGameplay.transform.Find("Panel Option A")?.gameObject;
            panelOptionBObj = panelGameplay.transform.Find("Panel Option B")?.gameObject;
            
            if(panelOptionAObj) optionACanvasGroup = panelOptionAObj.GetComponent<CanvasGroup>();
            if(panelOptionBObj) optionBCanvasGroup = panelOptionBObj.GetComponent<CanvasGroup>();

            if(panelCard)
            {
                cardBackground = panelCard.transform.Find("Background")?.GetComponent<Image>();
                cardBodyshot   = panelCard.transform.Find("Bodyshot")?.GetComponent<Image>();
                cardName       = panelCard.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                cardGlow       = panelCard.transform.Find("Glow")?.GetComponent<Image>();
            }

            if(panelOptionAObj)
            {
                optionATitle  = panelOptionAObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                optionAFlavor = panelOptionAObj.transform.Find("Flavor")?.GetComponent<TextMeshProUGUI>();
            }
            if(panelOptionBObj)
            {
                optionBTitle  = panelOptionBObj.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                optionBFlavor = panelOptionBObj.transform.Find("Flavor")?.GetComponent<TextMeshProUGUI>();
            }
        }

        // --- NEW: AUTO-WIRE OUTCOME PANEL ---
        if(panelOutcomeRoot != null)
        {
            Transform pCard = panelOutcomeRoot.transform.Find("Panel Card");
            if(pCard != null)
            {
                outcomeCard.nameText = pCard.Find("Name")?.GetComponent<TextMeshProUGUI>();
                outcomeCard.bodyshotImage = pCard.Find("Bodyshot")?.GetComponent<Image>();
            }
        }

        // Stakeholders
        stakeholders.Clear();
        if(panelStakeholders)
        {
            Transform groupContainer = panelStakeholders.transform.Find("Group");
            if(groupContainer)
            {
                AddStakeholderToList(groupContainer, "Stakeholder Civil Society", "Civil Society");
                AddStakeholderToList(groupContainer, "Stakeholder Industry",      "Industry");
                AddStakeholderToList(groupContainer, "Stakeholder Governance",    "Governance");
                AddStakeholderToList(groupContainer, "Stakeholder Innovation",    "Innovation");
            }
        }
    }

    private void AddStakeholderToList(Transform container, string objectName, string id)
    {
        Transform t = container.Find(objectName);
        if (t == null) return;

        StakeholderSlot slot = new StakeholderSlot();
        slot.id = id;
        slot.groupParent = t.gameObject;
        slot.background = t.Find("Background")?.GetComponent<Image>();
        slot.headshot = t.Find("Headshot")?.GetComponent<Image>();
        slot.nameText = t.Find("Name")?.GetComponent<TextMeshProUGUI>();
        
        Transform resObj = t.Find("Ressource");
        if (resObj != null)
        {
            slot.segmentBar = resObj.GetComponent<ResourceSegmentBar>();
        }

        stakeholders.Add(slot);
    }

    // --- OUTCOME SYSTEM (UPDATED) ---

    public void ShowOutcomeUI(string outcomeText, StakeholderData actor)
    {
        // 1. Hide Options
        if(panelOptionAObj) panelOptionAObj.SetActive(false);
        if(panelOptionBObj) panelOptionBObj.SetActive(false);

        // 2. Show Outcome Panel
        if(panelOutcomeRoot != null) 
        {
            panelOutcomeRoot.SetActive(true);
        }

        // 3. Title Text
        if(outcomeTitleText != null)
        {
            outcomeTitleText.text = outcomeText;
        }
        
        // 4. Explanation Text
        if(outcomeExplanationText != null)
        {
            outcomeExplanationText.text = outcomeText;
        }

        // 4. Update Card Visuals in Outcome Panel
        if(actor != null)
        {
            // Set Name
            if(outcomeCard.nameText != null) outcomeCard.nameText.text = actor.displayName;

            // Set Image
            if(!string.IsNullOrEmpty(actor.bodyAddress) && outcomeCard.bodyshotImage != null)
            {
                // Clear old sprite
                outcomeCard.bodyshotImage.sprite = null;
                Addressables.LoadAssetAsync<Sprite>(actor.bodyAddress).Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded) 
                        outcomeCard.bodyshotImage.sprite = handle.Result;
                };
            }
        }
    }

    public void DisplayOutcomeSummary(ResourceManager.ResourceSaveData oldStats, ResourceManager newStats)
    {
        if(outcomeBarsController != null)
        {
            outcomeBarsController.GenerateOutcomeBars(oldStats, newStats);
        }
    }

    public void HideOutcomeSummary()
    {
        if(panelOutcomeRoot != null) 
        {
            panelOutcomeRoot.SetActive(false);
        }
    }

    // --- VISUAL METHODS & PREVIEWS ---

    public void ResetTurnUI()
    {
        HideOutcomeSummary(); // Ensure Popup is closed

        // Bring back options
        if(panelOptionAObj) panelOptionAObj.SetActive(true);
        if(panelOptionBObj) panelOptionBObj.SetActive(true);
        if(optionACanvasGroup) optionACanvasGroup.alpha = 1f; 
        if(optionBCanvasGroup) optionBCanvasGroup.alpha = 1f;
    }

    public void ShowStatPreview(StatBlock effects, ResourceManager currentResources)
    {
        UpdateSinglePreview("Industry",      currentResources.industryVal,   effects.industry);
        UpdateSinglePreview("Civil Society", currentResources.civilVal,      effects.civilSociety);
        UpdateSinglePreview("Governance",    currentResources.governanceVal, effects.governance);
        UpdateSinglePreview("Innovation",    currentResources.innovationVal, effects.innovation);
    }

    private void UpdateSinglePreview(string id, int currentVal, int change)
    {
        foreach(var slot in stakeholders)
        {
            if(slot.id == id && slot.segmentBar != null)
            {
                slot.segmentBar.UpdateVisuals(currentVal, change);
                return;
            }
        }
    }

    public void ResetPreviews(ResourceManager currentResources)
    {
        UpdateResourceDisplay("Industry",      currentResources.industryVal);
        UpdateResourceDisplay("Civil Society", currentResources.civilVal);
        UpdateResourceDisplay("Governance",    currentResources.governanceVal);
        UpdateResourceDisplay("Innovation",    currentResources.innovationVal);
    }

    public void UpdateResourceDisplay(string groupID, int value)
    {
        foreach (var slot in stakeholders)
        {
            if (slot.id == groupID && slot.segmentBar != null)
            {
                slot.segmentBar.UpdateVisuals(value, 0);
                return;
            }
        }
    }

    // --- THIS IS THE METHOD THAT WAS MISSING ---
    public void SetupStakeholderSlot(string groupID, string name, string headAddressKey)
    {
        foreach (var slot in stakeholders)
        {
            if (slot.id == groupID)
            {
                if(slot.nameText != null) slot.nameText.text = name;
                if (!string.IsNullOrEmpty(headAddressKey) && slot.headshot != null)
                {
                    Addressables.LoadAssetAsync<Sprite>(headAddressKey).Completed += (op) =>
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded) slot.headshot.sprite = op.Result;
                    };
                }
                return;
            }
        }
    }
    // -------------------------------------------

    public void SetMainCard(string name, string bodyAddressKey, string groupID)
    {
        if(cardName != null) cardName.text = name;
        var swipe = transform.GetComponentInChildren<SwipeController>(); 
        if(swipe != null) swipe.ResetCardPosition();

        if (!string.IsNullOrEmpty(bodyAddressKey) && cardBodyshot != null)
        {
            cardBodyshot.sprite = null; 
            Addressables.LoadAssetAsync<Sprite>(bodyAddressKey).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) cardBodyshot.sprite = handle.Result;
            };
        }

        if (cardGlow != null)
        {
            if (string.IsNullOrEmpty(groupID))
            {
                cardGlow.gameObject.SetActive(false);
            }
            else
            {
                Color c = Color.white; 
                bool found = false;
                foreach(var def in groupColors)
                {
                    if (def.id == groupID) 
                    {
                        c = def.glowColor;
                        found = true;
                        break;
                    }
                }

                cardGlow.color = c;
                cardGlow.gameObject.SetActive(found);
            }
        }
    }

    public void SetOptions(string titleA, string flavorA, string titleB, string flavorB)
    {
        if(optionATitle != null) optionATitle.text = titleA;
        if(optionAFlavor != null) optionAFlavor.text = flavorA;
        if(optionBTitle != null) optionBTitle.text = titleB;
        if(optionBFlavor != null) optionBFlavor.text = flavorB;
    }
    
    public void UpdateRoundInfo(int currentRound, string description)
    {
        if(roundNumber != null) roundNumber.text = currentRound.ToString();
        if(topFlavorText != null) topFlavorText.text = description;
    }
    
    public void HideGameInterface()
    {
        if(panelStakeholders) panelStakeholders.SetActive(false);
        if(panelTop) panelTop.SetActive(false);
        if(panelGameplay) panelGameplay.SetActive(false);
        if(panelOutcomeRoot) panelOutcomeRoot.SetActive(false);

    }
}