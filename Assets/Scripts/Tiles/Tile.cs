using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isConstructible = true;
    [SerializeField] private bool _isNode = true;
    [SerializeField] private bool _isWalkable = true;


    // Exposed read-only property used by placement logic.
    public bool IsConstructible => _isConstructible;
}
