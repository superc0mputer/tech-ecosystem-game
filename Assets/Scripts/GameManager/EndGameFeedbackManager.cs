using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    }

    private void Awake()
    {
        AutoWireUI();
        if(panelEndGame) panelEndGame.SetActive(false);
    }

    private void AutoWireUI()
    {
        // 1. Find the Root Panel
        if (panelEndGame == null)
            panelEndGame = transform.Find("Panel End Screen")?.gameObject;

        if (panelEndGame == null)
        {
            Debug.LogError("EndGameFeedbackManager: Could not find 'Panel End Screen'.");
            return;
        }

        // 2. Find the Main Title
        Transform topPanel = panelEndGame.transform.Find("Panel Top");
        if (topPanel != null)
        {
            Transform titleObj = topPanel.Find("Title"); 
            if (titleObj == null) titleObj = topPanel.Find("Text"); 
            
            if (titleObj != null) mainResultTitle = titleObj.GetComponent<TextMeshProUGUI>();
        }

        // 3. Find Category Slots (CORRECTED SPELLING)
        // Make sure your Hierarchy names match these exactly:
        WireSlot(panelEndGame.transform, "Panel Stakeholder Summary Industry",      slotIndustry);
        WireSlot(panelEndGame.transform, "Panel Stakeholder Summary Governance",    slotGovernance);
        WireSlot(panelEndGame.transform, "Panel Stakeholder Summary Civil Society", slotCivilSociety);
        WireSlot(panelEndGame.transform, "Panel Stakeholder Summary Innovation",    slotInnovation);
    }

    private void WireSlot(Transform root, string panelName, FeedbackSlot slot)
    {
        Transform panel = root.Find(panelName);
        if (panel == null)
        {
            Debug.LogError($"UI Error: Could not find '{panelName}'. Did you rename it in the Hierarchy?");
            return;
        }

        var scoreObj = panel.Find("Score Number");
        var stateObj = panel.Find("State Text");
        var descObj  = panel.Find("Description");

        if (scoreObj) slot.scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        if (stateObj) slot.stateText = stateObj.GetComponent<TextMeshProUGUI>();
        if (descObj)  slot.descriptionText = descObj.GetComponent<TextMeshProUGUI>();
    }

    // --- PUBLIC METHODS ---

    public void ShowFeedback(int industry, int gov, int civil, int innov, bool isWin)
    {
        if(panelEndGame) panelEndGame.SetActive(true);

        if(mainResultTitle) 
            mainResultTitle.text = isWin ? "SIMULATION COMPLETE" : "GAME OVER";

        FillSlot(slotIndustry, "Industry", industry);
        FillSlot(slotGovernance, "Governance", gov);
        FillSlot(slotCivilSociety, "Civil Society", civil);
        FillSlot(slotInnovation, "Capability", innov);
    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FillSlot(FeedbackSlot slot, string category, int score)
    {
        if (slot.scoreText) slot.scoreText.text = score.ToString();
        
        var data = GetOutcomeData(category, score);
        if (slot.stateText) slot.stateText.text = data.state;
        if (slot.descriptionText) slot.descriptionText.text = data.description;
        
        if (slot.stateText)
        {
            if (score <= 0 || score >= 10)
            {
                slot.stateText.color = Color.maroon;
            }
            else slot.stateText.color = Color.white;
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

            case "Capability":
                if (score <= 0) return ("Technical Debt", "Game Over: Buggy and slow. Engineers quit. Effectively a pile of broken code.");
                if (score <= 3) return ("Low-Fi Utility", "Simple and reliable, but lacks 'magic.' Users are migrating to exciting platforms.");
                if (score <= 6) return ("Balanced Innovation", "Top-tier UX. Powerful but stays within human-controllable limits.");
                if (score <= 9) return ("The Cutting Edge", "Singularity's doorstep. Solving problems humans can't describe yet.");
                return ("The Black Box Event", "Game Over: AI is so complex no human understands it. It started rewriting its own goals.");
        }
        return ("Unknown", "No data.");
    }
}