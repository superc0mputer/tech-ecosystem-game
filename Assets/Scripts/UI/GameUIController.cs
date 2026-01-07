using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AddressableAssets; 
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameUIController : MonoBehaviour
{
    // --- SERIALIZED FIELDS (For Debugging in Inspector) ---

    [Header("--- PANELS ---")]
    [SerializeField] private GameObject panelStakeholders;
    [SerializeField] private GameObject panelTop;
    [SerializeField] private GameObject panelGameplay; // NEW PARENT

    [Header("--- TOP PANEL ---")]
    [SerializeField] private TextMeshProUGUI roundNumber; 
    [SerializeField] private TextMeshProUGUI topFlavorText; 

    [Header("--- MAIN CARD ---")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardBodyshot;
    [SerializeField] private TextMeshProUGUI cardName;

    [Header("--- OPTION A ---")]
    [SerializeField] private Image optionABackground;
    [SerializeField] private TextMeshProUGUI optionATitle;
    [SerializeField] private TextMeshProUGUI optionAFlavor;

    [Header("--- OPTION B ---")]
    [SerializeField] private Image optionBBackground;
    [SerializeField] private TextMeshProUGUI optionBTitle;
    [SerializeField] private TextMeshProUGUI optionBFlavor;

    [Header("--- STAKEHOLDERS ---")]
    [SerializeField] private List<StakeholderSlot> stakeholders = new List<StakeholderSlot>();

    [System.Serializable]
    public class StakeholderSlot
    {
        public string id; 
        public Image background;
        public Slider resourceSlider;
        public Image headshot;
        public TextMeshProUGUI nameText;
        public GameObject groupParent;
    }

    // --- AUTOMATIC SETUP ---
    private void Awake()
    {
        AutoWireUI();
    }

    private void AutoWireUI()
    {
        // 1. Find Main Panels
        // Hierarchy: Canvas -> Panel Stakeholders
        panelStakeholders = transform.Find("Panel Stakeholders").gameObject;
        
        // Hierarchy: Canvas -> Panel Top
        panelTop = transform.Find("Panel Top").gameObject;
        
        // Hierarchy: Canvas -> Panel Gameplay
        panelGameplay = transform.Find("Panel Gameplay").gameObject; 

        // 2. Wire Top Panel
        Transform roundObj = panelTop.transform.Find("Round");
        // Try finding text inside Round, or look for specific names
        var roundNumCheck = roundObj.Find("Text/Round Number");
        if (roundNumCheck != null) roundNumber = roundNumCheck.GetComponent<TextMeshProUGUI>();

        // Hierarchy: Panel Top -> Flavor -> Flavor Text
        Transform flavorObj = panelTop.transform.Find("Flavor");
        topFlavorText = flavorObj.Find("Flavor Text").GetComponent<TextMeshProUGUI>();

        // 3. Wire Gameplay Panel (Card & Options)
        GameObject panelCard = panelGameplay.transform.Find("Panel Card").gameObject;
        GameObject panelOptionA = panelGameplay.transform.Find("Panel Option A").gameObject;
        GameObject panelOptionB = panelGameplay.transform.Find("Panel Option B").gameObject;

        // Wire Card
        cardBackground = panelCard.transform.Find("Background").GetComponent<Image>();
        cardBodyshot   = panelCard.transform.Find("Bodyshot").GetComponent<Image>();
        cardName       = panelCard.transform.Find("Name").GetComponent<TextMeshProUGUI>();

        // Wire Option A
        optionABackground = panelOptionA.transform.Find("Background").GetComponent<Image>();
        optionATitle      = panelOptionA.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        optionAFlavor     = panelOptionA.transform.Find("Flavor").GetComponent<TextMeshProUGUI>();

        // Wire Option B
        optionBBackground = panelOptionB.transform.Find("Background").GetComponent<Image>();
        optionBTitle      = panelOptionB.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        optionBFlavor     = panelOptionB.transform.Find("Flavor").GetComponent<TextMeshProUGUI>();

        // 4. Wire Stakeholders
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
        if (t == null)
        {
            Debug.LogError($"UI Error: Could not find '{objectName}' inside Panel Stakeholders/Group");
            return;
        }

        StakeholderSlot slot = new StakeholderSlot();
        slot.id = id;
        slot.groupParent = t.gameObject;
        
        slot.background   = t.Find("Background").GetComponent<Image>();
        
        // UPDATED: Now looks for Slider component on "Ressource"
        slot.resourceSlider = t.Find("Ressource").GetComponent<Slider>(); 
        
        slot.headshot     = t.Find("Headshot").GetComponent<Image>();
        slot.nameText     = t.Find("Name").GetComponent<TextMeshProUGUI>();

        stakeholders.Add(slot);
    }

    // --- PUBLIC METHODS ---

    public void UpdateRoundInfo(int currentRound, string description)
    {
        if(roundNumber != null) roundNumber.text = currentRound.ToString();
        if(topFlavorText != null) topFlavorText.text = description;
    }

    public void SetMainCard(string name, string bodyAddressKey)
    {
        if(cardName != null) cardName.text = name;

        if (!string.IsNullOrEmpty(bodyAddressKey) && cardBodyshot != null)
        {
            cardBodyshot.sprite = null; 
            Addressables.LoadAssetAsync<Sprite>(bodyAddressKey).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && cardBodyshot != null)
                {
                    cardBodyshot.sprite = handle.Result;
                }
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

    public void SetupStakeholderSlot(string groupID, string name, string headAddressKey)
    {
        foreach (var slot in stakeholders)
        {
            if (slot.id == groupID)
            {
                if(slot.nameText != null) slot.nameText.text = name;
                
                // Initialize Slider (Assuming 0-10 scale in logic, Slider is 0-1.0 or 0-10)
                // If Slider is set to Max Value 10 in Inspector, use value directly.
                // If Slider is standard 0-1, use normalization.
                // Safest to assume normalized (0.5f) for start:
                if(slot.resourceSlider != null) 
                {
                    // If you haven't set Max Value to 10 in Inspector, do this:
                    slot.resourceSlider.maxValue = 10; 
                    slot.resourceSlider.value = 5; 
                }

                if (!string.IsNullOrEmpty(headAddressKey) && slot.headshot != null)
                {
                    Addressables.LoadAssetAsync<Sprite>(headAddressKey).Completed += (AsyncOperationHandle<Sprite> obj) =>
                    {
                        if (obj.Status == AsyncOperationStatus.Succeeded && slot.headshot != null)
                        {
                            slot.headshot.sprite = obj.Result;
                        }
                    };
                }
                return;
            }
        }
    }

    public void UpdateResourceDisplay(string groupID, int value)
    {
        foreach (var slot in stakeholders)
        {
            if (slot.id == groupID)
            {
                if(slot.resourceSlider != null)
                {
                    // Directly set the slider value (assuming Max Value is 10)
                    slot.resourceSlider.value = value;
                }
                return;
            }
        }
    }
}