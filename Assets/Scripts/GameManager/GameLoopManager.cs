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
    public float outcomeDelay = 60.0f; 
    
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

        // 1. Reset UI (Options, sliders, etc)
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
            
            // --- SYNC POINT: WAIT FOR ASSETS ---
            // We use yield return StartCoroutine to pause here until UpdateUIAsync is totally finished
            yield return StartCoroutine(UpdateUIAsync(currentCard, actor));
            
            // If the screen was hidden (Alpha 0), fade it in now that everything is loaded
            if (uiController.gameplayCanvasGroup != null && uiController.gameplayCanvasGroup.alpha < 0.9f)
            {
               yield return StartCoroutine(uiController.FadeInUI(0.5f));
            }
        }
        else { EndGame(true); }
    }

    // Changed from void to IEnumerator to support waiting
    private IEnumerator UpdateUIAsync(EventCardData card, StakeholderData actor)
    {
        if (uiController == null) yield break;

        uiController.UpdateRoundInfo(currentTurn + 1, card.bodyText);

        string actorName = actor != null ? actor.displayName : "Unknown";
        string actorBodyAddress = actor != null ? actor.bodyAddress : "";
        string actorGroup = actor != null ? actor.group : "";

        // Wait for Addressables to finish downloading/assigning
        yield return StartCoroutine(uiController.SetMainCardAsync(actorName, actorBodyAddress, actorGroup));
        
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

    private IEnumerator OutcomeSequence(ChoiceData choice, ResourceManager.ResourceSaveData oldState)
    {
        StakeholderData actor = null;
        if(currentCard != null)
        {
            actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
        }

        if(uiController != null) 
        {
            uiController.ShowOutcomeUI(choice.outcomeTitle, choice.outcomeText, actor);
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
            // Must use Coroutine here for restore as well
            StartCoroutine(UpdateUIAsync(currentCard, actor));
            
            // Fade in manually on restore if needed
            if(uiController != null && uiController.gameplayCanvasGroup != null)
                StartCoroutine(uiController.FadeInUI());
        }
        else if (isGameActive && currentCard == null)
        {
            StartCoroutine(NextTurnRoutine());
        }
        
        Debug.Log("[GameLoop] Restore Complete.");
    }
}