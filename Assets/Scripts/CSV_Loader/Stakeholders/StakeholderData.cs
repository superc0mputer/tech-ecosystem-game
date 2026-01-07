using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stakeholder", menuName = "Game/Stakeholder")]
public class StakeholderData : ScriptableObject
{
    [Header("CSV: Identity")]
    public string id;              // e.g. stk_marcus
    public string group;           // Industry, Civil Society...
    public string archetype;       // "The Aggressive Investor"
    public string displayName;     // "Marcus 'Burn-Rate' Sterling"
    public string firstName;       
    public string lastName;        

    [Header("CSV: Narrative")]
    [TextArea(3,5)] public string goal;          
    [TextArea(3,5)] public string intrinsicNeed; 
    [TextArea(3,5)] public string extrinsicNeed; 
    [TextArea(3,5)] public string uniqueTension; 

    [Header("CSV: Assets")]
    public string headAddress;     // Addressable Key
    public string bodyAddress;     // Addressable Key

    [Header("Runtime Data")]
    public List<EventCardData> associatedEvents = new List<EventCardData>();
}