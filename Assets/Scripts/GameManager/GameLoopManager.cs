using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 

public class GameLoopManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;
    public GameUIController uiController; // UI REFERENCE

    [Header("Game Settings")]
    public int maxTurns = 10;
    
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

        if (currentTurn >= maxTurns)
        {
            EndGame(true);
            yield break;
        }

        // Logic: If we already have a card (e.g. from Save Game), don't draw a new one
        if (currentCard == null && stakeholderManager.gameDeck.Count > 0)
        {
            currentCard = stakeholderManager.gameDeck[0];
            stakeholderManager.gameDeck.RemoveAt(0); 
        }

        if (currentCard != null)
        {
            Debug.Log($"Turn {currentTurn + 1}: Displaying card '{currentCard.name}'");

            // UI: Find the actor and update the screen
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

        uiController.SetOptions(
            card.choiceA.label, card.choiceA.flavor,
            card.choiceB.label, card.choiceB.flavor
        );
    }

    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        Debug.Log($"Selected: {selectedOption.label}");

        resourceManager.ApplyEffects(selectedOption.effects);

        if (resourceManager.CheckGameOverCondition())
        {
            EndGame(false);
        }
        else
        {
            currentCard = null; // Clear card so we draw a new one
            currentTurn++;
            StartCoroutine(NextTurnRoutine());
        }
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        string msg = isWin ? "VICTORY: Consensus Reached." : "DEFEAT: Talks broke down.";
        Debug.Log(msg);
        
        // UI: Show end game text
        if(uiController != null) uiController.UpdateRoundInfo(currentTurn, msg);
    }

    // --- SAVE SYSTEM IMPLEMENTATION ---

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
            
            // UI: If we loaded a card in progress, SHOW IT immediately
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
        
        // Restart the routine if the game was active
        if(isGameActive && currentCard == null)
        {
            StartCoroutine(NextTurnRoutine());
        }
    }
}