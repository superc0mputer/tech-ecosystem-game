using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets; 
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;

public class EndGameFeedbackManager : MonoBehaviour
{
    [Header("UI Panels (Auto-Wired)")]
    [SerializeField] private GameObject panelEndGame;
    [SerializeField] private TextMeshProUGUI mainResultTitle;

    private FeedbackSlot slotIndustry = new FeedbackSlot();
    private FeedbackSlot slotGovernance = new FeedbackSlot();
    private FeedbackSlot slotCivilSociety = new FeedbackSlot();
    private FeedbackSlot slotInnovation = new FeedbackSlot();

    [System.Serializable]
    public class FeedbackSlot
    {
        public TextMeshProUGUI scoreText;       
        public TextMeshProUGUI stateText;       
        public TextMeshProUGUI descriptionText;
        public ResourceSegmentBar resourceBar; 
        public Image headshotImage;
        public TextMeshProUGUI nameText;
    }

    private void Awake()
    {
        AutoWireUI();
        if(panelEndGame) panelEndGame.SetActive(false);
    }

    private void AutoWireUI()
    {
        if (panelEndGame == null)
        {
            Debug.LogError("Panel End Screen is not assigned in the Inspector!");
            return;
        }

        // UPDATED: Find the container "Panel Stakeholder" first
        Transform container = panelEndGame.transform.Find("Panel Stakeholder");

        if (container == null)
        {
            Debug.LogError("Could not find 'Panel Stakeholder' inside Panel End Screen. Check hierarchy names.");
            return;
        }

        // UPDATED: Wire using the 'container' as the root, not panelEndGame
        WireSlot(container, "Panel Stakeholder Summary Industry",      slotIndustry);
        
        // IMPORTANT: Matches the typo "Gocernance" seen in your screenshot
        WireSlot(container, "Panel Stakeholder Summary Gocernance",    slotGovernance); 
        
        WireSlot(container, "Panel Stakeholder Summary Civil Society", slotCivilSociety);
        WireSlot(container, "Panel Stakeholder Summary Innovation",    slotInnovation);
    }

    private void WireSlot(Transform root, string panelName, FeedbackSlot slot)
    {
        Transform panel = root.Find(panelName);
        if (panel == null) 
        {
            Debug.LogWarning($"Could not find panel named '{panelName}' inside '{root.name}'");
            return;
        }

        var scoreObj = panel.Find("Score Number");
        var stateObj = panel.Find("State Text") ?? panel.Find("State");
        var descObj  = panel.Find("Description");
        var resourceObj = panel.Find("Ressource");
        var nameObj = panel.Find("Name");
        var headObj = panel.Find("Headshot");

        if (scoreObj) slot.scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        if (stateObj) slot.stateText = stateObj.GetComponent<TextMeshProUGUI>();
        if (descObj)  slot.descriptionText = descObj.GetComponent<TextMeshProUGUI>();
        if (resourceObj) slot.resourceBar = resourceObj.GetComponent<ResourceSegmentBar>();
        if (nameObj) slot.nameText = nameObj.GetComponent<TextMeshProUGUI>();
        if (headObj) slot.headshotImage = headObj.GetComponent<Image>();
    }
    
    public void ShowFeedback(int industry, int gov, int civil, int innov, bool isWin, List<StakeholderData> activeStakeholders)
    {
        if(panelEndGame) panelEndGame.SetActive(true);

        if(mainResultTitle) 
            mainResultTitle.text = isWin ? "SIMULATION COMPLETE" : "GAME OVER";

        FillSlot(slotIndustry, "Industry", industry, GetStakeholderByGroup(activeStakeholders, "Industry"));
        FillSlot(slotGovernance, "Governance", gov, GetStakeholderByGroup(activeStakeholders, "Governance"));
        FillSlot(slotCivilSociety, "Civil Society", civil, GetStakeholderByGroup(activeStakeholders, "Civil Society"));
        FillSlot(slotInnovation, "Innovation", innov, GetStakeholderByGroup(activeStakeholders, "Innovation")); 
    }

    private StakeholderData GetStakeholderByGroup(List<StakeholderData> list, string group)
    {
        if (list == null) return null;
        return list.FirstOrDefault(s => s.group == group);
    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FillSlot(FeedbackSlot slot, string category, int score, StakeholderData person)
    {
        // 1. Fill Standard Stats
        if (slot.scoreText) slot.scoreText.text = score.ToString();

        if (slot.resourceBar) slot.resourceBar.UpdateVisuals(score, 0);

        var data = GetOutcomeData(category, score);
        if (slot.stateText) slot.stateText.text = data.state;
        if (slot.descriptionText) slot.descriptionText.text = data.description;
        
        if (slot.stateText)
        {
            slot.stateText.color = (score <= 0 || score >= 10) ? new Color(0.8f, 0f, 0f) : Color.white;
        }

        // 2. Fill Personal Identity
        if (person != null)
        {
            if (slot.nameText) slot.nameText.text = person.displayName;

            if (slot.headshotImage && !string.IsNullOrEmpty(person.headAddress))
            {
                slot.headshotImage.sprite = null; 
                Addressables.LoadAssetAsync<Sprite>(person.headAddress).Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded && slot.headshotImage != null)
                    {
                        slot.headshotImage.sprite = op.Result;
                    }
                };
            }
        }
    }

    private (string state, string description) GetOutcomeData(string category, int score)
    {
         switch (category)
        {
            case "Industry":
                if (score <= 0) return ("Bankruptcy", "Game Over: Your 'burn rate' exceeded your funding. Investors have liquidated your assets.");
                if (score <= 3) return ("The Struggling Startup", "You survived on the fringes. A 'zombie company' lacking capital to scale.");
                if (score <= 6) return ("Sustainable Innovator", "The 'Safe Zone.' A stable business model that satisfies investors without compromising soul.");
                if (score <= 9) return ("The Market Titan", "A dominant force. Your profit margins are the envy of Silicon Valley.");
                return ("Monopoly Liquidation", "Game Over: You became so dominant that regulators broke your company into six pieces.");

            case "Governance":
                if (score <= 0) return ("The Outlaw", "Game Over: You ignored the AI Act. Government seized your servers; you face trial in The Hague.");
                if (score <= 3) return ("Regulatory Rogue", "You operate in the 'Grey Market.' High-risk liability no government will touch.");
                if (score <= 6) return ("Certified Reliable", "Your models are audited and standardized. The gold standard for tech policy.");
                if (score <= 9) return ("The Policy Architect", "You help write the rules. Primary advisor for UN and EU tech frameworks.");
                return ("Bureaucratic Paralysis", "Game Over: Focused so much on compliance you never shipped. A museum of paperwork.");

            case "Civil Society":
                if (score <= 0) return ("Public Enemy No. 1", "Game Over: Massive protests. Branded as a tool of oppression. Reputation destroyed.");
                if (score <= 3) return ("The Elitist Tool", "Works well, but only for the wealthy. NGOs criticize the digital divide.");
                if (score <= 6) return ("Inclusive & Ethical", "Respected by civic actors. Used for public good, mitigating worst biases.");
                if (score <= 9) return ("The People's Champion", "Empowered grassroots movements. Synonymous with digital equity.");
                return ("The Design Trap", "Game Over: Tried to satisfy everyone. Product became so 'neutral' it is no longer functional.");

            case "Innovation":
                if (score <= 0) return ("Technical Debt", "Game Over: Buggy and slow. Engineers quit. Effectively a pile of broken code.");
                if (score <= 3) return ("Low-Fi Utility", "Simple and reliable, but lacks 'magic.' Users are migrating to exciting platforms.");
                if (score <= 6) return ("Balanced Innovation", "Top-tier UX. Powerful but stays within human-controllable limits.");
                if (score <= 9) return ("The Cutting Edge", "Singularity's doorstep. Solving problems humans can't describe yet.");
                return ("The Black Box Event", "Game Over: AI is so complex no human understands it. It started rewriting its own goals.");
        }
        return ("Unknown", "No data.");
    }
}