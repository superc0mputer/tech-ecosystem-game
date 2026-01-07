using UnityEngine;
using System.Collections;

public class GameLoopManager : MonoBehaviour
{
    [Header("Dependencies")]
    public StakeholderManager stakeholderManager;
    public ResourceManager resourceManager;

    [Header("Game Settings")]
    public int maxTurns = 10; //TODO change number
    
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

        // 1. Setup Data
        stakeholderManager.InitializeGame();
        
        // 2. Setup Resources
        resourceManager.InitializeResources();

        // 3. Start First Turn
        StartCoroutine(NextTurnRoutine());
    }

    // This handles the timing of drawing a card
    private IEnumerator NextTurnRoutine()
    {
        if (!isGameActive) yield break;

        // Check Win Condition (Time limit reached)
        if (currentTurn >= maxTurns)
        {
            EndGame(true);
            yield break;
        }

        // Draw Card logic
        if (stakeholderManager.gameDeck.Count > 0)
        {
            currentCard = stakeholderManager.gameDeck[0];
            stakeholderManager.gameDeck.RemoveAt(0); // Remove card from deck
            
            // TODO: Here you would tell the UI to spawn/show the card prefab
            Debug.Log($"Turn {currentTurn + 1}: Displaying card '{currentCard.bodyText}'");
        }
        else
        {
            Debug.LogWarning("Deck ran out of cards!");
            EndGame(true);
        }
    }

    // Call this from your UI Buttons or Swipe Controller
    // isLeftChoice: true = Option A, false = Option B
    public void OnPlayerChoice(bool isLeftChoice)
    {
        if (!isGameActive || currentCard == null) return;

        ChoiceData selectedOption = isLeftChoice ? currentCard.choiceA : currentCard.choiceB;
        
        Debug.Log($"Selected: {selectedOption.label} ({selectedOption.flavor})");

        // 1. Apply Stats
        resourceManager.ApplyEffects(selectedOption.effects);

        // 2. Check Loss Condition
        if (resourceManager.CheckGameOverCondition())
        {
            EndGame(false);
        }
        else
        {
            // 3. Prepare Next Turn
            currentTurn++;
            StartCoroutine(NextTurnRoutine());
        }
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        if (isWin)
        {
            Debug.Log("GAME OVER: You Survived! Calculation Score...");
            // TODO: Calculate score based on ResourceManager values
        }
        else
        {
            Debug.Log("GAME OVER: You were fired (One value hit 0).");
        }
    }
}
