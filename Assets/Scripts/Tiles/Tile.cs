using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isConstructible = true;
    [SerializeField] public bool _isNode = true;
    [SerializeField] private bool _isWalkable = true;
    [SerializeField] private bool _isFinish = false;
    [SerializeField] private bool _isStart = false;

    // Exposed read-only property used by placement logic.
    public bool IsConstructible => _isConstructible;

    // Exposed read-only property used by pathing / monsters.
    public bool IsWalkable => _isWalkable;

    // Exposed read-only property used by pathing / monsters to detect nodes.
    public bool IsNode => _isNode;

    // Exposed read-only property used to mark the finish tile.
    public bool IsFinish => _isFinish;

    // Exposed read-only property used to mark the start tile (spawner).
    public bool IsStart => _isStart;
}
