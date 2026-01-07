using UnityEngine;

[System.Serializable]
public struct StatBlock
{
    public int industry;
    public int civilSociety;
    public int governance;
    public int innovation;
}

[System.Serializable]
public struct ChoiceData
{
    public string label;     
    public string flavor;    
    public StatBlock effects; 
}

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Event Card")]
public class EventCardData : ScriptableObject
{
    [Header("CSV: Core Info")]
    public string id;            
    public string characterId;   
    public string title;         
    [TextArea(3, 10)] 
    public string bodyText;      

    [Header("CSV: Choices")]
    public ChoiceData choiceA;   
    public ChoiceData choiceB;   
}