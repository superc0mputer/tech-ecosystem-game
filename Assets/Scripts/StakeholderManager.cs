using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq; 

public class StakeholderManager : MonoBehaviour, ISaveable
{
    [Header("Database")]
    [SerializeField] private List<StakeholderData> allStakeholders;

    [Header("Runtime State")]
    public List<StakeholderData> activeStakeholders = new List<StakeholderData>();
    public List<EventCardData> gameDeck = new List<EventCardData>();

    private void Start()
    {
        // Only Initialize if we aren't loading immediately after start
        // Usually, you let Start run, and then LoadGame overwrites it.
        InitializeGame();
    }

    public void InitializeGame()
    {
        SelectStakeholders();
        BuildDeck();
    }

    private void SelectStakeholders()
    {
        activeStakeholders.Clear();
        var industryPool = allStakeholders.Where(s => s.group == "Industry").ToList();
        var civilPool = allStakeholders.Where(s => s.group == "Civil Society").ToList();
        var govPool = allStakeholders.Where(s => s.group == "Governance").ToList();
        var innovPool = allStakeholders.Where(s => s.group == "Innovation").ToList();

        if(industryPool.Count > 0) activeStakeholders.Add(GetRandom(industryPool));
        if(civilPool.Count > 0)    activeStakeholders.Add(GetRandom(civilPool));
        if(govPool.Count > 0)      activeStakeholders.Add(GetRandom(govPool));
        if(innovPool.Count > 0)    activeStakeholders.Add(GetRandom(innovPool));
    }

    private void BuildDeck()
    {
        gameDeck.Clear();
        foreach (var stakeholder in activeStakeholders)
        {
            gameDeck.AddRange(stakeholder.associatedEvents);
        }
        ShuffleDeck(gameDeck);
    }

    private StakeholderData GetRandom(List<StakeholderData> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    private void ShuffleDeck<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    // --- Helpers for Save System (Lookup by name) ---
    public StakeholderData FindStakeholderByName(string name)
    {
        return allStakeholders.FirstOrDefault(s => s.name == name);
    }

    public EventCardData FindCardByName(string name)
    {
        // Search through all possible cards in the active stakeholders
        // (Optimized: In a real game, you might want a global card database)
        foreach(var s in allStakeholders) 
        {
            var card = s.associatedEvents.FirstOrDefault(c => c.name == name);
            if (card != null) return card;
        }
        return null;
    }

    // --- SAVE SYSTEM IMPLEMENTATION ---

    [System.Serializable]
    private struct StakeholderSaveData
    {
        public List<string> activeStakeholderNames;
        public List<string> deckCardNames;
    }

    public object CaptureState()
    {
        return new StakeholderSaveData
        {
            // Save the Names of the scriptable objects
            activeStakeholderNames = activeStakeholders.Select(s => s.name).ToList(),
            deckCardNames = gameDeck.Select(c => c.name).ToList()
        };
    }

    public void RestoreState(object state)
    {
        var data = ((JObject)state).ToObject<StakeholderSaveData>();

        // Reconstruct Active Stakeholders
        activeStakeholders.Clear();
        foreach(string name in data.activeStakeholderNames)
        {
            var s = FindStakeholderByName(name);
            if(s != null) activeStakeholders.Add(s);
        }

        // Reconstruct Deck (Preserving order!)
        gameDeck.Clear();
        foreach(string cardName in data.deckCardNames)
        {
            var c = FindCardByName(cardName);
            if(c != null) gameDeck.Add(c);
        }
    }
}