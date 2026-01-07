using UnityEngine;
using System;

[RequireComponent(typeof(ISaveable))] // Enforce the interface
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id = string.Empty;

    public string Id => id;

    // Generate a unique ID automatically in the Editor so you don't forget
    [ContextMenu("Generate ID")] 
    private void GenerateId()
    {
        id = Guid.NewGuid().ToString();
    }
}