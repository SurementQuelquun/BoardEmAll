using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isConstructible = true;
    [SerializeField] private bool _isNode = true;
    [SerializeField] private bool _isWalkable = true;


    // Exposed read-only property used by placement logic.
    public bool IsConstructible => _isConstructible;

    // Exposed read-only property used by pathing / monsters.
    public bool IsWalkable => _isWalkable;

    // Exposed read-only property used by pathing / monsters to detect nodes.
    public bool IsNode => _isNode;
}
