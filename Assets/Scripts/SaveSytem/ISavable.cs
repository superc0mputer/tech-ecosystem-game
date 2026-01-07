using UnityEngine;

// ISaveable.cs
public interface ISaveable
{
    // Return a lightweight serializable object (POCO) representing this object's state
    object CaptureState();

    // Take a loaded object and apply it back to this script
    void RestoreState(object state);
}
