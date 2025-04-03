using UnityEngine;

public class SnapEventSO : ScriptableObject
{
    // For notifying game managers about a snap (see Snapper.cs), if desired
    public event System.Action<GameObject> OnSnap;
    public event System.Action<GameObject> OnUnsnap;

    public void RaiseSnap(GameObject obj) => OnSnap?.Invoke(obj);
    public void RaiseUnsnap(GameObject obj) => OnUnsnap?.Invoke(obj);
}