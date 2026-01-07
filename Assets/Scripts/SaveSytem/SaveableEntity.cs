using UnityEngine;

[RequireComponent(typeof(ISaveable))]
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id;
    public string Id => id;

    [ContextMenu("Generate ID")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }
}