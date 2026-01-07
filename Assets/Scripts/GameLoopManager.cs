using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq; 

public class GameLoopManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;

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

        // If we already have a card (loaded from save), don't draw a new one
        if (currentCard == null && stakeholderManager.gameDeck.Count > 0)
        {
            currentCard = stakeholderManager.gameDeck[0];
            stakeholderManager.gameDeck.RemoveAt(0); 
        }

        if (currentCard != null)
        {
            Debug.Log($"Turn {currentTurn + 1}: Displaying card '{currentCard.name}'"); // Changed bodyText to name for generic debug
            // TODO: Spawn UI for currentCard
        }
        else
        {
            Debug.LogWarning("Deck ran out of cards!");
            EndGame(true);
        }
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
        Debug.Log(isWin ? "GAME OVER: You Survived!" : "GAME OVER: Fired.");
    }

    // --- SAVE SYSTEM IMPLEMENTATION ---

    [System.Serializable]
    private struct GameLoopData
    {
        public int currentTurn;
        public bool isGameActive;
        public string currentCardName; // Save string reference
    }

    public object CaptureState()
    {
        return new GameLoopData
        {
            currentTurn = this.currentTurn,
            isGameActive = this.isGameActive,
            // Handle case where we save in between turns (currentCard might be null)
            currentCardName = currentCard != null ? currentCard.name : ""
        };
    }

    public void RestoreState(object state)
    {
        var data = ((JObject)state).ToObject<GameLoopData>();

        this.currentTurn = data.currentTurn;
        this.isGameActive = data.isGameActive;

        // Restore the specific card the player was looking at
        if (!string.IsNullOrEmpty(data.currentCardName))
        {
            this.currentCard = stakeholderManager.FindCardByName(data.currentCardName);
        }
        else
        {
            this.currentCard = null;
        }
        
        // Restart the routine if the game was active
        if(isGameActive)
        {
            StartCoroutine(NextTurnRoutine());
        }
    }
}