using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 

public class GameLoopManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;
    public GameUIController uiController; 
    public EndGameFeedbackManager feedbackManager; // Reference to the new End Screen Manager

    [Header("Game Settings")]
    public int maxTurns = 10;
    public float outcomeDelay = 2.0f; // Time to read the result before next card
    
    [Header("Current State")]
    public int currentTurn = 0;
    public EventCardData currentCard;
    public bool isGameActive = false;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentTurn = 0;
        isGameActive = true;
        
        // Initialize systems
        stakeholderManager.InitializeGame();
        resourceManager.InitializeResources();

        StartCoroutine(NextTurnRoutine());
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

        // 2. Check Win Condition (Time)
        if (currentTurn >= maxTurns)
        {
            EndGame(true); // Win by surviving
            yield break;
        }

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
        else
        {
            Debug.Log("Deck Empty!");
            EndGame(true); // Win by empty deck
        }
    }

    private void UpdateUI(EventCardData card, StakeholderData actor)
    {
        if (uiController == null) return;

        uiController.UpdateRoundInfo(currentTurn + 1, card.bodyText);

        string actorName = actor != null ? actor.displayName : "Unknown";
        string actorBodyAddress = actor != null ? actor.bodyAddress : "";
        
        uiController.SetMainCard(actorName, actorBodyAddress);

        uiController.SetOptions(
            card.choiceA.label, card.choiceA.flavor,
            card.choiceB.label, card.choiceB.flavor
        );
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
            yield return new WaitForSeconds(1.5f); // Suspense wait
            EndGame(false); // Loss
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
        
        // 1. Hide ONLY the gameplay panels (keep Canvas active for Feedback)
        if(uiController != null) uiController.HideGameInterface();

        // 2. Show the Feedback Screen
        if (feedbackManager != null)
        {
            feedbackManager.ShowFeedback(
                resourceManager.industryVal,
                resourceManager.governanceVal,
                resourceManager.civilVal,
                resourceManager.innovationVal,
                isWin
            );
        }
        
        Debug.Log(isWin ? "Game Complete (Victory)" : "Game Over (Stat Collapse)");
    }

    // --- SAVE SYSTEM ---

    [System.Serializable]
    private struct GameLoopData
    {
        public int currentTurn;
        public bool isGameActive;
        public string currentCardName; 
    }

    public object CaptureState()
    {
        return new GameLoopData
        {
            currentTurn = this.currentTurn,
            isGameActive = this.isGameActive,
            currentCardName = currentCard != null ? currentCard.name : ""
        };
    }

    public void RestoreState(object state)
    {
        var data = ((JObject)state).ToObject<GameLoopData>();

        this.currentTurn = data.currentTurn;
        this.isGameActive = data.isGameActive;

        if (!string.IsNullOrEmpty(data.currentCardName))
        {
            this.currentCard = stakeholderManager.FindCardByName(data.currentCardName);
            if(this.currentCard != null)
            {
                StakeholderData actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
                UpdateUI(currentCard, actor);
            }
        }
        else
        {
            this.currentCard = null;
        }
        
        if(isGameActive && currentCard == null)
        {
            StartCoroutine(NextTurnRoutine());
        }
    }
}