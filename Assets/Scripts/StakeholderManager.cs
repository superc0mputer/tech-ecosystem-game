using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class StakeholderManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private List<StakeholderData> allStakeholders;

    [Header("Runtime State")]
    public List<StakeholderData> activeStakeholders = new List<StakeholderData>();
    
    // This is the final deck of cards for the game
    public List<EventCardData> gameDeck = new List<EventCardData>();

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        SelectStakeholders();
        BuildDeck();
        
        Debug.Log($"Game Initialized with {activeStakeholders.Count} stakeholders and {gameDeck.Count} cards.");
    }

    private void SelectStakeholders()
    {
        activeStakeholders.Clear();

        // 1. Filter the database into temporary lists by group
        List<StakeholderData> industryPool = allStakeholders.Where(s => s.group == "Industry").ToList();
        List<StakeholderData> civilPool = allStakeholders.Where(s => s.group == "Civil Society").ToList();
        List<StakeholderData> govPool = allStakeholders.Where(s => s.group == "Governance").ToList();
        List<StakeholderData> innovPool = allStakeholders.Where(s => s.group == "Innovation").ToList();

        // 2. Pick one random character from each pool
        // (Ensure you have at least one character created for each group to avoid errors!)
        if(industryPool.Count > 0) activeStakeholders.Add(GetRandom(industryPool));
        if(civilPool.Count > 0)    activeStakeholders.Add(GetRandom(civilPool));
        if(govPool.Count > 0)      activeStakeholders.Add(GetRandom(govPool));
        if(innovPool.Count > 0)    activeStakeholders.Add(GetRandom(innovPool));
    }

    private void BuildDeck()
    {
        gameDeck.Clear();

        // Loop through the 4 selected stakeholders and add their cards to the pile
        foreach (var stakeholder in activeStakeholders)
        {
            gameDeck.AddRange(stakeholder.associatedEvents);
        }

        // Optional: Shuffle the deck here so events don't appear in order
        ShuffleDeck(gameDeck);
    }

    // Helper to get random item from list
    private StakeholderData GetRandom(List<StakeholderData> list)
    {
        int index = Random.Range(0, list.Count);
        return list[index];
    }

    // Fisher-Yates Shuffle Algorithm
    private void ShuffleDeck<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
