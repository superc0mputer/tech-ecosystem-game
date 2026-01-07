using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour, ISaveable
{
    // ... (Keep existing Header variables: Dependencies, Settings, State) ...
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;
    public GameUIController uiController; 
    public EndGameFeedbackManager feedbackManager;

    [Header("Game Settings")]
    public int maxTurns = 10;
    public float outcomeDelay = 2.0f; 
    
    [Header("Current State")]
    public int currentTurn = 0;
    public EventCardData currentCard;
    public bool isGameActive = false;

    private void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            Debug.Log("Save file found. Attempting load...");
            // We do NOT call InitializeGame() here because RestoreState will handle filling the lists
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
        // Clear previous state manually if starting fresh
        if(stakeholderManager.activeStakeholders.Count > 0) stakeholderManager.activeStakeholders.Clear();

        currentTurn = 0;
        isGameActive = true;
        
        stakeholderManager.InitializeGame(); // Fills lists randomly
        resourceManager.InitializeResources();

        StartCoroutine(NextTurnRoutine());
    }

    // ... (Keep ExitAndSave, RestartGame, NextTurnRoutine, UpdateUI, etc. EXACTLY AS THEY WERE) ...
    // Just pasting the Save System fix below to save space. 
    
    // (Ensure you have ExitAndSave, RestartGame, NextTurnRoutine, ShowPreview, OnPlayerChoice, OutcomeSequence, EndGame implemented as before)
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

        // 1. Reset UI for the new turn (Buttons back, colors reset)
        if(uiController != null) 
        {
            uiController.ResetTurnUI();
            uiController.ResetPreviews(resourceManager);
        }

        if (currentTurn >= maxTurns) { EndGame(true); yield break; }

        // 3. Draw Card
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
        
        uiController.SetMainCard(actorName, actorBodyAddress);
        uiController.SetOptions(card.choiceA.label, card.choiceA.flavor, card.choiceB.label, card.choiceB.flavor);
    }

    // --- PREVIEW SYSTEM (THE FIX) ---
    // Called by SwipeController when dragging
    public void ShowPreview(bool isLeft)
    {
        if (currentCard == null || uiController == null) return;

        // 1. Get stats from the card data
        StatBlock effects = isLeft ? currentCard.choiceA.effects : currentCard.choiceB.effects;
        
        // 2. Send them to the UI Controller to move the sliders
        uiController.ShowStatPreview(effects, resourceManager);
    }

    public void ClearPreview()
    {
        if (uiController == null) return;
        
        // Reset sliders to their actual values
        uiController.ResetPreviews(resourceManager);
    }
    // --------------------------------

    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        // Clear any lingering previews
        ClearPreview(); 

        // 1. Get Data
        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        Debug.Log($"Selected: {selectedOption.label}");

        // 2. Apply Stats
        resourceManager.ApplyEffects(selectedOption.effects);

        // 3. Start Outcome Phase
        StartCoroutine(OutcomeSequence(selectedOption));
    }

    private IEnumerator OutcomeSequence(ChoiceData choice)
    {
        // A. Show Outcome Text
        if(uiController != null) uiController.ShowOutcomeUI(choice.outcome);

        // B. Check Game Over (Stat Failure)
        if (resourceManager.CheckGameOverCondition())
        {
            yield return new WaitForSeconds(1.5f);
            EndGame(false); 
            yield break;
        }

        // C. Reading Time
        yield return new WaitForSeconds(outcomeDelay);

        // D. Next Turn
        currentCard = null; 
        currentTurn++;
        StartCoroutine(NextTurnRoutine());
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        if(SaveManager.Instance != null) SaveManager.Instance.DeleteSaveFile();
        if(uiController != null) uiController.HideGameInterface();

        // 2. Show the Feedback Screen
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

    // --- COORDINATOR SAVE SYSTEM FIX ---

    [System.Serializable]
    public class MasterSaveData
    {
        public int currentTurn;
        public bool isGameActive;
        public string currentCardName;
        
        // These will be saved as JObjects inside the JSON
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
            
            // Capture sub-states
            resourceData = resourceManager.CaptureState(),
            stakeholderData = stakeholderManager.CaptureState()
        };
    }

    public void RestoreState(object state)
    {
        Debug.Log("[GameLoop] Starting Restore...");
        
        // 1. Cast the generic object to JObject, then to MasterSaveData
        var jState = state as JObject;
        if (jState == null) { Debug.LogError("Save state is null!"); return; }

        var data = jState.ToObject<MasterSaveData>();

        this.currentTurn = data.currentTurn;
        this.isGameActive = data.isGameActive;

        // 2. Restore Sub-Systems
        // We pass the inner data objects. Newtonsoft makes them JObjects/JTokens automatically.
        if(data.resourceData != null) 
        {
            Debug.Log("[GameLoop] Passing data to Resource Manager...");
            resourceManager.RestoreState(data.resourceData);
        }
        
        if(data.stakeholderData != null) 
        {
            Debug.Log("[GameLoop] Passing data to Stakeholder Manager...");
            stakeholderManager.RestoreState(data.stakeholderData);
        }

        // 3. Restore Card Logic
        if (!string.IsNullOrEmpty(data.currentCardName))
        {
            this.currentCard = stakeholderManager.FindCardByName(data.currentCardName);
        }

        // 4. Update UI
        if(isGameActive && currentCard != null)
        {
            StakeholderData actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
            UpdateUI(currentCard, actor);
        }
        else if (isGameActive && currentCard == null)
        {
            // If we saved exactly between turns, resume the loop
            StartCoroutine(NextTurnRoutine());
        }
        
        Debug.Log("[GameLoop] Restore Complete.");
    }
}