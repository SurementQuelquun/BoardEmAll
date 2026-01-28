using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    float _currentHealth;

    [Header("Movement")]
    public Vector2Int direction = new Vector2Int(1, 0); // default +X
    public float moveSpeed = 2f;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 1;


    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";

    [Header("Options")]
    public bool snapToGridOnStart = true;
    public float centerEpsilon = 0.05f;

    bool _isMoving;
    int _lastTileX = int.MinValue;
    int _lastTileZ = int.MinValue;

    // Cached tile lookup and finish positions to avoid repeated GameObject.Find calls
    Dictionary<Vector2Int, Tile> _tiles;
    List<Vector2Int> _finishPositions;

    static readonly Vector2Int[] Cardinal = {
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,1), new Vector2Int(0,-1)
    };

    void Start()
    {
        _currentHealth = maxHealth;
        StartCoroutine(StartMovingAfterDelay());
    }

    IEnumerator StartMovingAfterDelay()
    {


        if (snapToGridOnStart)
        {
            var p = transform.position;
            transform.position = new Vector3(Mathf.Round(p.x), p.y, Mathf.Round(p.z));
        }

        if (moveSpeed <= 0f) yield break;

        EnsureTileCache();

        var cur = GridPos(transform.position);
        if (!TryGetTile(cur, out var curTile) || !curTile.IsWalkable) yield break;

        EnsureFinishCache();

        // keep configured direction if valid, otherwise pick best adjacent towards finish or any walkable
        var forward = cur + direction;
        if (IsWalkable(forward)) { _isMoving = true; yield break; }

        if (PickBestAdjacentTowardsFinish(cur, out var best))
        {
            direction = best;
            _isMoving = true;
            yield break;
        }

        if (PickAnyAdjacent(cur, out var any))
        {
            direction = any;
            _isMoving = true;
            yield break;
        }
    }

    void Update()
    {
        // drive animator
        if (animator != null) animator.SetFloat(speedParameter, _isMoving ? Mathf.Abs(moveSpeed) : 0f);

        if (!_isMoving || _currentHealth <= 0f) return;

        // move
        var moveVec = new Vector3(direction.x, 0f, direction.y).normalized;
        transform.Translate(moveVec * moveSpeed * Time.deltaTime, Space.World);
        if (moveVec.sqrMagnitude > 0.0001f) transform.forward = Vector3.Lerp(transform.forward, moveVec, 10f * Time.deltaTime);

        // operate only when near tile center and when entering new tile
        var cur = GridPos(transform.position);
        var center = new Vector3(cur.x, transform.position.y, cur.y);
        if (Vector3.Distance(transform.position, center) > centerEpsilon) return;
        if (cur.x == _lastTileX && cur.y == _lastTileZ) return;
        _lastTileX = cur.x; _lastTileZ = cur.y;

        EnsureTileCache();

        if (!TryGetTile(cur, out var tile) || !tile.IsWalkable) { Stop(); return; }

        // finish reached
        if (tile.IsFinish)
        {
            HealthManager.RemovePoints(damageToPlayer);
            Destroy(gameObject);
            return;
        }

        EnsureFinishCache();

        // on node choose neighbor that reduces distance to finish (avoid immediate back unless no other)
        if (tile.IsNode)
        {
            var chosen = ChooseBestMove(cur);
            if (chosen == null) { Stop(); return; }
            direction = chosen.Value;
            return;
        }
        else
        {
            // not node: ensure forward exists, otherwise try to move toward finish or stop
            var f = cur + direction;
            if (IsWalkable(f)) return;
            if (PickBestAdjacentTowardsFinish(cur, out var best)) { direction = best; return; }
            Stop();
            return; 
        }
    }

    // ----- helpers -----

    private Vector2Int GridPos(Vector3 world) => new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.z));

    private void EnsureTileCache()
    {
        if (_tiles != null && _tiles.Count > 0) return;
        _tiles = new Dictionary<Vector2Int, Tile>();
        foreach (var t in FindObjectsOfType<Tile>())
        {
            var p = t.transform.position;
            _tiles[new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.z))] = t;
        }
    }

    private void EnsureFinishCache()
    {
        if (_finishPositions != null && _finishPositions.Count > 0) return;
        _finishPositions = new List<Vector2Int>();
        EnsureTileCache();
        foreach (var kv in _tiles)
            if (kv.Value.IsFinish) _finishPositions.Add(kv.Key);
    }

    private bool TryGetTile(Vector2Int pos, out Tile tile)
    {
        if (_tiles == null) EnsureTileCache();
        return _tiles.TryGetValue(pos, out tile);
    }

    private bool IsWalkable(Vector2Int pos) => TryGetTile(pos, out var t) && t.IsWalkable;

    // returns candidate that minimizes squared distance to nearest finish
    private Vector2Int? ChooseBestMove(Vector2Int cur)
    {
        var ahead = direction;
        var back = new Vector2Int(-direction.x, -direction.y);

        var candidates = new List<Vector2Int>();
        if (IsWalkable(cur + ahead) && !IsOpposite(ahead)) candidates.Add(ahead);
        var left = new Vector2Int(-direction.y, direction.x);
        var right = new Vector2Int(direction.y, -direction.x);
        if (IsWalkable(cur + left) && !IsOpposite(left)) candidates.Add(left);
        if (IsWalkable(cur + right) && !IsOpposite(right)) candidates.Add(right);
        if (candidates.Count == 0 && IsWalkable(cur + back)) candidates.Add(back);
        if (candidates.Count == 0) return null;

        Vector2Int best = candidates[0];
        float bestD = DistanceToNearestFinish(cur + best);
        for (int i = 1; i < candidates.Count; i++)
        {
            float d = DistanceToNearestFinish(cur + candidates[i]);
            if (d < bestD - 1e-4f) { best = candidates[i]; bestD = d; }
            else if (Mathf.Approximately(d, bestD))
            {
                // prefer ahead, then left
                if (candidates[i] == ahead && best != ahead) { best = ahead; bestD = d; }
                else if (best != ahead && candidates[i] == left && best == right) { best = left; bestD = d; }
            }
        }
        return best;
    }

    private bool PickBestAdjacentTowardsFinish(Vector2Int cur, out Vector2Int choice)
    {
        EnsureFinishCache();
        choice = default;
        if (_finishPositions == null || _finishPositions.Count == 0) return false;

        Vector2Int best = default;
        float bestD = float.PositiveInfinity;
        bool found = false;

        // prefer current direction then cardinals
        var cand = new List<Vector2Int> { direction, Cardinal[0], Cardinal[1], Cardinal[2], Cardinal[3] };
        var seen = new HashSet<Vector2Int>();
        foreach (var c in cand)
        {
            if (!seen.Add(c)) continue;
            var p = cur + c;
            if (!IsWalkable(p)) continue;
            float d = DistanceToNearestFinish(p);
            if (!found || d < bestD) { best = c; bestD = d; found = true; }
        }

        if (found) { choice = best; return true; }
        return false;
    }

    private bool PickAnyAdjacent(Vector2Int cur, out Vector2Int choice)
    {
        choice = default;
        foreach (var c in Cardinal)
        {
            if (IsWalkable(cur + c)) { choice = c; return true; }
        }
        return false;
    }

    private float DistanceToNearestFinish(Vector2Int pos)
    {
        EnsureFinishCache();
        if (_finishPositions == null || _finishPositions.Count == 0) return float.PositiveInfinity;
        float best = float.PositiveInfinity;
        for (int i = 0; i < _finishPositions.Count; i++)
        {
            var f = _finishPositions[i];
            float dx = f.x - pos.x, dz = f.y - pos.y;
            float d2 = dx * dx + dz * dz;
            if (d2 < best) best = d2;
        }
        return best;
    }

    private bool IsOpposite(Vector2Int v) => v.x == -direction.x && v.y == -direction.y;

    private void Stop()
    {
        _isMoving = false;
        if (animator != null) animator.SetFloat(speedParameter, 0f);
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0f) Die();
    }

    void Die()
    {
        Stop();
        Destroy(gameObject);
    }
}