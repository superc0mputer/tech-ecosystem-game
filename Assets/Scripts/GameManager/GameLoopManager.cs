using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;
    public GameUIController uiController; 
    public EndGameFeedbackManager feedbackManager;

    [Header("Game Settings")]
    public int maxTurns = 10;
    public float outcomeDelay = 10.0f; 
    
    [Header("Current State")]
    public int currentTurn = 0;
    public EventCardData currentCard;
    public bool isGameActive = false;

    private void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            Debug.Log("Save file found. Attempting load...");
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.Log("No save file. Starting Fresh.");
            StartGame();
        }
    }

    public void StartGame()
    {
        if(stakeholderManager.activeStakeholders.Count > 0) stakeholderManager.activeStakeholders.Clear();

        currentTurn = 0;
        isGameActive = true;
        
        stakeholderManager.InitializeGame(); 
        resourceManager.InitializeResources();

        StartCoroutine(NextTurnRoutine());
    }

    public void ExitAndSave()
    {
        if(SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        SceneManager.LoadScene("Main Menu");
    }
    public void RestartGame()
    {
        if(SaveManager.Instance != null) SaveManager.Instance.DeleteSaveFile();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ExitAndReset()
    {
        if(SaveManager.Instance != null) SaveManager.Instance.DeleteSaveFile();
        SceneManager.LoadScene("Main Menu");
    }
    
    private IEnumerator NextTurnRoutine()
    {
        if (!isGameActive) yield break;

        // 1. Reset UI
        if(uiController != null) 
        {
            uiController.ResetTurnUI();
            uiController.ResetPreviews(resourceManager);
        }

        if (currentTurn >= maxTurns) { EndGame(true); yield break; }

        // 2. Draw Card
        if (currentCard == null && stakeholderManager.gameDeck.Count > 0)
        {
            currentCard = stakeholderManager.gameDeck[0];
            stakeholderManager.gameDeck.RemoveAt(0); 
        }

        if (currentCard != null)
        {
            Debug.Log($"Turn {currentTurn + 1}: {currentCard.name}");
            
            StakeholderData actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
            UpdateUI(currentCard, actor);
        }
        else { EndGame(true); }
    }

    private void UpdateUI(EventCardData card, StakeholderData actor)
    {
        if (uiController == null) return;

        uiController.UpdateRoundInfo(currentTurn + 1, card.bodyText);

        string actorName = actor != null ? actor.displayName : "Unknown";
        string actorBodyAddress = actor != null ? actor.bodyAddress : "";
        string actorGroup = actor != null ? actor.group : "";

        uiController.SetMainCard(actorName, actorBodyAddress, actorGroup);
        
        uiController.SetOptions(card.choiceA.label, card.choiceA.flavor, card.choiceB.label, card.choiceB.flavor);
    }

    // --- PREVIEW SYSTEM ---
    public void ShowPreview(bool isLeft)
    {
        if (currentCard == null || uiController == null) return;
        StatBlock effects = isLeft ? currentCard.choiceA.effects : currentCard.choiceB.effects;
        uiController.ShowStatPreview(effects, resourceManager);
    }

    public void ClearPreview()
    {
        if (uiController == null) return;
        uiController.ResetPreviews(resourceManager);
    }

    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        ClearPreview(); 

        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        Debug.Log($"Selected: {selectedOption.label}");

        // 1. CAPTURE OLD STATE
        var oldStateObj = resourceManager.CaptureState();
        ResourceManager.ResourceSaveData oldState = (ResourceManager.ResourceSaveData)oldStateObj;

        // 2. APPLY EFFECTS
        resourceManager.ApplyEffects(selectedOption.effects);

        // 3. START OUTCOME SEQUENCE
        StartCoroutine(OutcomeSequence(selectedOption, oldState));
    }

    // --- UPDATED METHOD ---
    private IEnumerator OutcomeSequence(ChoiceData choice, ResourceManager.ResourceSaveData oldState)
    {
        // Get the actor associated with the CURRENT card before we null it out
        StakeholderData actor = null;
        if(currentCard != null)
        {
            actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
        }

        if(uiController != null) 
        {
            // Pass the outcome text AND the actor to the UI
            uiController.ShowOutcomeUI(choice.outcome, actor);
            
            // Generate the resource bars
            uiController.DisplayOutcomeSummary(oldState, resourceManager);
        }

        if (resourceManager.CheckGameOverCondition())
        {
            yield return new WaitForSeconds(2.0f);
            EndGame(false); 
            yield break;
        }

        yield return new WaitForSeconds(outcomeDelay);

        currentCard = null; 
        currentTurn++;
        StartCoroutine(NextTurnRoutine());
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        if(SaveManager.Instance != null) SaveManager.Instance.DeleteSaveFile();
        if(uiController != null) uiController.HideGameInterface();

        if (feedbackManager != null)
        {
            feedbackManager.ShowFeedback(
                resourceManager.industryVal,
                resourceManager.governanceVal,
                resourceManager.civilVal,
                resourceManager.innovationVal,
                isWin,
                stakeholderManager.activeStakeholders 
            );
        }
        
        Debug.Log(isWin ? "Game Complete (Victory)" : "Game Over (Stat Collapse)");
    }

    // --- SAVE SYSTEM ---

    [System.Serializable]
    public class MasterSaveData
    {
        public int currentTurn;
        public bool isGameActive;
        public string currentCardName;
        public object resourceData;
        public object stakeholderData;
    }

    public object CaptureState()
    {
        return new MasterSaveData
        {
            currentTurn = this.currentTurn,
            isGameActive = this.isGameActive,
            currentCardName = currentCard != null ? currentCard.name : "",
            resourceData = resourceManager.CaptureState(),
            stakeholderData = stakeholderManager.CaptureState()
        };
    }

    public void RestoreState(object state)
    {
        Debug.Log("[GameLoop] Starting Restore...");
        
        var jState = state as JObject;
        if (jState == null) { Debug.LogError("Save state is null!"); return; }

        var data = jState.ToObject<MasterSaveData>();

        this.currentTurn = data.currentTurn;
        this.isGameActive = data.isGameActive;

        if(data.resourceData != null) resourceManager.RestoreState(data.resourceData);
        if(data.stakeholderData != null) stakeholderManager.RestoreState(data.stakeholderData);

        if (!string.IsNullOrEmpty(data.currentCardName))
        {
            this.currentCard = stakeholderManager.FindCardByName(data.currentCardName);
        }

        if(isGameActive && currentCard != null)
        {
            StakeholderData actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
            UpdateUI(currentCard, actor);
        }
        else if (isGameActive && currentCard == null)
        {
            StartCoroutine(NextTurnRoutine());
        }
        
        Debug.Log("[GameLoop] Restore Complete.");
    }
}