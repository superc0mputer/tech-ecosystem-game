using UnityEngine;

[System.Serializable]
public struct ChoiceData
{
    public string label;     // maps to "choice_x_label"
    public string flavor;    // maps to "choice_x_flavor"
    
    // Stores the raw string "industry:2|civil_society:-1"
    // Your runtime Logic Manager will parse this when the choice is clicked.
    public string effects;   // maps to "choice_x_effects" 
    
    [TextArea(2, 5)] 
    public string result;    // maps to "choice_x_result"
}

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Event Card")]
public class EventCardData : ScriptableObject
{
    [Header("CSV: Core Info")]
    public string id;            // maps to "id"
    public string characterId;   // maps to "character"
    public string title;         // maps to "title"
    [TextArea(3, 10)] 
    public string bodyText;      // maps to "body_text"

    [Header("CSV: Choices")]
    public ChoiceData choiceA;   // Container for all choice_a_... columns
    public ChoiceData choiceB;   // Container for all choice_b_... columns
}