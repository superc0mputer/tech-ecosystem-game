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

    [Header("--- TOP PANEL ---")]
    [SerializeField] private TextMeshProUGUI roundNumber; 
    [SerializeField] private TextMeshProUGUI topFlavorText; 

    [Header("--- MAIN CARD ---")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardBodyshot;
    [SerializeField] private TextMeshProUGUI cardName;

    [Header("--- OPTIONS ---")]
    [SerializeField] private GameObject panelOptionAObj;
    [SerializeField] private GameObject panelOptionBObj;
    
    // Canvas Groups for Swipe Transparency
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
        public Slider resourceSlider; 
        public Image sliderFillImage; // NEW: The colored part of the slider
        public Image headshot;
        public TextMeshProUGUI nameText;
        
        // Internal state to reset colors
        [HideInInspector] public Color defaultFillColor;
    }

    private void Awake()
    {
        AutoWireUI();
    }

    private void AutoWireUI()
    {
        panelStakeholders = transform.Find("Panel Stakeholders").gameObject;
        panelTop = transform.Find("Panel Top").gameObject;
        panelGameplay = transform.Find("Panel Gameplay").gameObject; 

        // Top Panel
        Transform roundObj = panelTop.transform.Find("Round");
        var roundNumCheck = roundObj.Find("Text/Round Number") ?? roundObj.Find("Round Number");
        if (roundNumCheck != null) roundNumber = roundNumCheck.GetComponent<TextMeshProUGUI>();

        Transform flavorObj = panelTop.transform.Find("Flavor");
        topFlavorText = flavorObj.Find("Flavor Text").GetComponent<TextMeshProUGUI>();

        // Gameplay Panel
        GameObject panelCard = panelGameplay.transform.Find("Panel Card").gameObject;
        panelOptionAObj = panelGameplay.transform.Find("Panel Option A").gameObject;
        panelOptionBObj = panelGameplay.transform.Find("Panel Option B").gameObject;
        
        optionACanvasGroup = panelOptionAObj.GetComponent<CanvasGroup>();
        optionBCanvasGroup = panelOptionBObj.GetComponent<CanvasGroup>();

        // Card & Options
        cardBackground = panelCard.transform.Find("Background").GetComponent<Image>();
        cardBodyshot   = panelCard.transform.Find("Bodyshot").GetComponent<Image>();
        cardName       = panelCard.transform.Find("Name").GetComponent<TextMeshProUGUI>();

        optionATitle  = panelOptionAObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        optionAFlavor = panelOptionAObj.transform.Find("Flavor").GetComponent<TextMeshProUGUI>();
        
        optionBTitle  = panelOptionBObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        optionBFlavor = panelOptionBObj.transform.Find("Flavor").GetComponent<TextMeshProUGUI>();

        // Stakeholders
        stakeholders.Clear();
        Transform groupContainer = panelStakeholders.transform.Find("Group");
        AddStakeholderToList(groupContainer, "Stakeholder Civil Society", "Civil Society");
        AddStakeholderToList(groupContainer, "Stakeholder Industry",      "Industry");
        AddStakeholderToList(groupContainer, "Stakeholder Governance",    "Governance");
        AddStakeholderToList(groupContainer, "Stakeholder Innovation",    "Innovation");
    }

    private void AddStakeholderToList(Transform container, string objectName, string id)
    {
        Transform t = container.Find(objectName);
        if (t == null) return;

        StakeholderSlot slot = new StakeholderSlot();
        slot.id = id;
        slot.groupParent = t.gameObject;
        slot.background = t.Find("Background").GetComponent<Image>();
        slot.headshot = t.Find("Headshot").GetComponent<Image>();
        slot.nameText = t.Find("Name").GetComponent<TextMeshProUGUI>();
        
        // Auto-Link Slider & Fill Image
        slot.resourceSlider = t.Find("Ressource").GetComponent<Slider>(); 
        if(slot.resourceSlider != null)
        {
            slot.resourceSlider.maxValue = 10;
            // Unity sliders usually have a "Fill Area/Fill" structure
            if(slot.resourceSlider.fillRect != null)
            {
                slot.sliderFillImage = slot.resourceSlider.fillRect.GetComponent<Image>();
                // Save original color (usually white/grey)
                if(slot.sliderFillImage != null) slot.defaultFillColor = slot.sliderFillImage.color;
            }
        }
        stakeholders.Add(slot);
    }

    // --- PREVIEW SYSTEM ---

    public void ShowStatPreview(StatBlock effects, ResourceManager currentResources)
    {
        // Calculate Future Values and Update Sliders
        UpdateSinglePreview("Industry",      currentResources.industryVal,   effects.industry);
        UpdateSinglePreview("Civil Society", currentResources.civilVal,      effects.civilSociety);
        UpdateSinglePreview("Governance",    currentResources.governanceVal, effects.governance);
        UpdateSinglePreview("Innovation",    currentResources.innovationVal, effects.innovation);
    }

    private void UpdateSinglePreview(string id, int currentVal, int change)
    {
        foreach(var slot in stakeholders)
        {
            if(slot.id == id && slot.resourceSlider != null)
            {
                int futureVal = Mathf.Clamp(currentVal + change, 0, 10);
                
                // 1. Show Future Value
                slot.resourceSlider.value = futureVal;

                // 2. Color Code
                if(slot.sliderFillImage != null)
                {
                    if (change > 0) slot.sliderFillImage.color = Color.green; // Going Up
                    else if (change < 0) slot.sliderFillImage.color = Color.red;   // Going Down
                    else slot.sliderFillImage.color = slot.defaultFillColor;       // No Change
                }
                return;
            }
        }
    }

    public void ResetPreviews(ResourceManager currentResources)
    {
        // Revert to actual current values and default colors
        UpdateResourceDisplay("Industry",      currentResources.industryVal);
        UpdateResourceDisplay("Civil Society", currentResources.civilVal);
        UpdateResourceDisplay("Governance",    currentResources.governanceVal);
        UpdateResourceDisplay("Innovation",    currentResources.innovationVal);

        // Reset Colors
        foreach(var slot in stakeholders)
        {
            if(slot.sliderFillImage != null)
            {
                slot.sliderFillImage.color = slot.defaultFillColor;
            }
        }
    }

    // --- EXISTING METHODS ---

    public void ShowOutcomeUI(string outcomeText)
    {
        if(panelOptionAObj) panelOptionAObj.SetActive(false);
        if(panelOptionBObj) panelOptionBObj.SetActive(false);
        if(topFlavorText) topFlavorText.text = outcomeText;
    }

    public void ResetTurnUI()
    {
        if(panelOptionAObj) panelOptionAObj.SetActive(true);
        if(panelOptionBObj) panelOptionBObj.SetActive(true);
        if(optionACanvasGroup) optionACanvasGroup.alpha = 1f; 
        if(optionBCanvasGroup) optionBCanvasGroup.alpha = 1f;
    }

    public void UpdateResourceDisplay(string groupID, int value)
    {
        foreach (var slot in stakeholders)
        {
            if (slot.id == groupID && slot.resourceSlider != null)
            {
                slot.resourceSlider.value = value;
                return;
            }
        }
    }

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

    public void SetMainCard(string name, string bodyAddressKey)
    {
        if(cardName != null) cardName.text = name;
        var swipe = panelGameplay.GetComponentInChildren<SwipeController>();
        if(swipe != null) swipe.ResetCardPosition();

        if (!string.IsNullOrEmpty(bodyAddressKey) && cardBodyshot != null)
        {
            cardBodyshot.sprite = null; 
            Addressables.LoadAssetAsync<Sprite>(bodyAddressKey).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) cardBodyshot.sprite = handle.Result;
            };
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
}