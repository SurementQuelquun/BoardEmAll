using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isConstructible = true;
    [SerializeField] public bool _isNode = true;

    // Exposed read-only property used by placement logic.
    public bool IsConstructible => _isConstructible;
}
