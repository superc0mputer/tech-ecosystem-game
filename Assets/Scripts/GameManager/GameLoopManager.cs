using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 

public class GameLoopManager : MonoBehaviour, ISaveable
{
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
        StartGame();
    }

    public void StartGame()
    {
        currentTurn = 0;
        isGameActive = true;
        
        stakeholderManager.InitializeGame();
        resourceManager.InitializeResources();

        StartCoroutine(NextTurnRoutine());
    }

    private IEnumerator NextTurnRoutine()
    {
        if (!isGameActive) yield break;

        // Reset UI
        if(uiController != null) 
        {
            uiController.ResetTurnUI();
            uiController.ResetPreviews(resourceManager);
        }

        // --- CHECK WIN BY TIME ---
        if (currentTurn >= maxTurns)
        {
            // Reached the end safely -> Trigger Win Screen
            EndGame(true);
            yield break;
        }

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
            EndGame(true); // Deck empty = Win
        }
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

    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        ClearPreview(); 

        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        resourceManager.ApplyEffects(selectedOption.effects);

        StartCoroutine(OutcomeSequence(selectedOption));
    }

    private IEnumerator OutcomeSequence(ChoiceData choice)
    {
        if(uiController != null) uiController.ShowOutcomeUI(choice.flavor);

        // --- CHECK LOSS CONDITION (0 or 10) ---
        if (resourceManager.CheckGameOverCondition())
        {
            yield return new WaitForSeconds(1.5f);
            EndGame(false); // Trigger Loss Screen
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
        
        // Hide the Game UI
        //if(uiController != null) uiController.gameObject.SetActive(false);
        uiController.HideGameInterface();

        // Show the Feedback UI
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
        
        Debug.Log(isWin ? "Game Complete" : "Game Over (Stat failure)");
    }
    
    // Pass-throughs for SwipeController
    public void ShowPreview(bool isLeft) { /* ... keep existing ... */ }
    public void ClearPreview() { /* ... keep existing ... */ }

    // ... (Keep CaptureState and RestoreState) ...
    public object CaptureState() { return null; } 
    public void RestoreState(object state) { }
}