using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq; 

public class StakeholderManager : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    public GameUIController uiController;

    [Header("Database")]
    [SerializeField] private List<StakeholderData> allStakeholders;

    [Header("Runtime State")]
    public List<StakeholderData> activeStakeholders = new List<StakeholderData>();
    public List<EventCardData> gameDeck = new List<EventCardData>();

    public void InitializeGame()
    {
        // Only init if empty (prevents overwriting loaded data)
        if (activeStakeholders.Count == 0)
        {
            SelectStakeholders();
            BuildDeck();
        }
    }

    // (Keep SelectStakeholders, BuildDeck, GetRandom, ShuffleDeck, FindStakeholderByName as they were...)
    // For brevity, I am not pasting the logic methods again, just the Save System updates below.
    // Make sure you keep your existing helper methods here!

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
        
        UpdateStakeholderUI();
    }
    
    private void BuildDeck()
    {
        gameDeck.Clear();
        foreach (var stakeholder in activeStakeholders) gameDeck.AddRange(stakeholder.associatedEvents);
        ShuffleDeck(gameDeck);
    }

    private StakeholderData GetRandom(List<StakeholderData> list) => list[Random.Range(0, list.Count)];
    
    private void ShuffleDeck<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1) { n--; int k = Random.Range(0, n + 1); (list[k], list[n]) = (list[n], list[k]); }
    }

    public void UpdateStakeholderUI()
    {
        if (uiController == null) return;
        foreach(var stakeholder in activeStakeholders)
        {
            uiController.SetupStakeholderSlot(stakeholder.group, stakeholder.displayName, stakeholder.headAddress);
        }
    }
    
    public StakeholderData GetStakeholderById(string id)
    {
        var found = activeStakeholders.Find(s => s.id == id);
        if (found == null) found = allStakeholders.Find(s => s.id == id);
        return found;
    }
    
    public EventCardData FindCardByName(string name)
    {
        foreach(var s in allStakeholders) 
        {
            var card = s.associatedEvents.FirstOrDefault(c => c.name == name);
            if (card != null) return card;
        }
        return null;
    }
    
    public StakeholderData FindStakeholderByName(string name) => allStakeholders.FirstOrDefault(s => s.name == name);


    // --- SAVE SYSTEM FIX ---

    // CHANGE 1: Made Public
    [System.Serializable]
    public struct StakeholderSaveData
    {
        public List<string> activeStakeholderNames;
        public List<string> deckCardNames;
    }

    public object CaptureState()
    {
        Debug.Log($"[Stakeholder] Saving {activeStakeholders.Count} actors and {gameDeck.Count} cards.");
        return new StakeholderSaveData
        {
            activeStakeholderNames = activeStakeholders.Select(s => s.name).ToList(),
            deckCardNames = gameDeck.Select(c => c.name).ToList()
        };
    }

    public void RestoreState(object state)
    {
        // CHANGE 2: Safer conversion
        JToken token = state as JToken;
        if (token != null)
        {
            var data = token.ToObject<StakeholderSaveData>();

            // 1. Restore Actors
            activeStakeholders.Clear();
            foreach(string name in data.activeStakeholderNames)
            {
                var s = FindStakeholderByName(name);
                if(s != null) activeStakeholders.Add(s);
            }
            UpdateStakeholderUI();

            // 2. Restore Deck (Order matters!)
            gameDeck.Clear();
            foreach(string cardName in data.deckCardNames)
            {
                var c = FindCardByName(cardName);
                if(c != null) gameDeck.Add(c);
            }
            Debug.Log($"[Stakeholder] Restored.");
        }
        else
        {
            Debug.LogError("[Stakeholder] Failed to restore: Data invalid.");
        }
    }
}