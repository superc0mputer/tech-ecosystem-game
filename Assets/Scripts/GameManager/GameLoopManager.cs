using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 

public class GameLoopManager : MonoBehaviour, ISaveable
{
    // ... (Keep existing dependencies and variables) ...
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;
    public GameUIController uiController; 

    [Header("Game Settings")]
    public int maxTurns = 10;
    public float outcomeDelay = 2.0f;
    
    [Header("Current State")]
    public int currentTurn = 0;
    public EventCardData currentCard;
    public bool isGameActive = false;

    // ... (Keep Start, StartGame, NextTurnRoutine exactly as they were) ...
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

        // Reset UI (Buttons and Previews)
        if(uiController != null) 
        {
            uiController.ResetTurnUI();
            uiController.ResetPreviews(resourceManager); // Reset sliders to default
        }

        if (currentTurn >= maxTurns)
        {
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
            Debug.Log($"Turn {currentTurn + 1}: Displaying card '{currentCard.name}'");
            StakeholderData actor = stakeholderManager.GetStakeholderById(currentCard.characterId);
            UpdateUI(currentCard, actor);
        }
        else
        {
            Debug.LogWarning("Deck ran out of cards!");
            EndGame(true);
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

    // --- NEW: PREVIEW LOGIC ---

    public void ShowPreview(bool isLeft)
    {
        if (currentCard == null || uiController == null) return;

        // Get the potential effects
        StatBlock effects = isLeft ? currentCard.choiceA.effects : currentCard.choiceB.effects;
        
        // Tell UI to preview them using current resources
        uiController.ShowStatPreview(effects, resourceManager);
    }

    public void ClearPreview()
    {
        if (uiController == null) return;
        uiController.ResetPreviews(resourceManager);
    }

    // --------------------------

    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        // Ensure visuals are clean before applying
        ClearPreview(); 

        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        resourceManager.ApplyEffects(selectedOption.effects);

        StartCoroutine(OutcomeSequence(selectedOption));
    }

    private IEnumerator OutcomeSequence(ChoiceData choice)
    {
        if(uiController != null) uiController.ShowOutcomeUI(choice.flavor);

        if (resourceManager.CheckGameOverCondition())
        {
            yield return new WaitForSeconds(1.5f);
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
        string msg = isWin ? "VICTORY: Consensus Reached." : "DEFEAT: Talks broke down.";
        if(uiController != null) uiController.UpdateRoundInfo(currentTurn, msg);
    }
    
    // ... (Keep CaptureState and RestoreState the same) ...
    public object CaptureState() { return null; } 
    public void RestoreState(object state) { }
}