using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Movement")]
    public Vector2Int direction = new Vector2Int(1, 0); // default +X (right)
    public float moveSpeed = 2f;    // units per second
    public float startDelay = 2f;   // seconds before starting to move

    [Header("Animation")]
    public Animator animator;               // assign in inspector
    public string speedParameter = "Speed"; // animation parameter name

    [Header("Options")]
    public bool snapToGridOnStart = true; // snap to integer grid at start
    public float centerEpsilon = 0.05f;  // threshold to consider "on center" of tile

    bool isMoving = false;

    // track last tile we processed to reduce repeated checks
    private int _lastProcessedTileX = int.MinValue;
    private int _lastProcessedTileZ = int.MinValue;

    void Start()
    {
        currentHealth = maxHealth;
        StartCoroutine(StartMovingAfterDelay());
    }

    IEnumerator StartMovingAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        if (snapToGridOnStart)
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(Mathf.Round(p.x), p.y, Mathf.Round(p.z));
        }

        if (moveSpeed <= 0f) yield break;

        // Check current tile exists & is walkable
        int curX = Mathf.RoundToInt(transform.position.x);
        int curZ = Mathf.RoundToInt(transform.position.z);
        GameObject curTileGO = GameObject.Find($"Tile {curX} {curZ}");
        if (curTileGO == null) yield break;
        Tile curTile = curTileGO.GetComponent<Tile>();
        if (curTile == null || !curTile.IsWalkable) yield break;

        // Check immediate forward tile before starting
        int nextX = curX + direction.x;
        int nextZ = curZ + direction.y;
        if (!CheckWalkableAt(nextX, nextZ)) yield break;

        isMoving = true;
    }

    void Update()
    {

        animator.SetFloat(speedParameter, isMoving ? Mathf.Abs(moveSpeed) : 0f);

        if (!isMoving || currentHealth <= 0f) return;

        // Move using the grid direction
        Vector3 moveVec = new Vector3(direction.x, 0f, direction.y).normalized;
        transform.Translate(moveVec * moveSpeed * Time.deltaTime, Space.World);

        // Face movement direction smoothly
        if (moveVec.sqrMagnitude > 0.0001f)
            transform.forward = Vector3.Lerp(transform.forward, moveVec, 10f * Time.deltaTime);

        // Check tile logic only when approximately centered on a tile
        int curX = Mathf.RoundToInt(transform.position.x);
        int curZ = Mathf.RoundToInt(transform.position.z);
        Vector3 centerPos = new Vector3(curX, transform.position.y, curZ);

        if (Vector3.Distance(transform.position, centerPos) > centerEpsilon)
            return; // not yet centered

        // Avoid re-processing same tile repeatedly
        if (curX == _lastProcessedTileX && curZ == _lastProcessedTileZ)
            return;

        _lastProcessedTileX = curX;
        _lastProcessedTileZ = curZ;

        // Get current tile
        GameObject tileGO = GameObject.Find($"Tile {curX} {curZ}");
        if (tileGO == null)
        {
            StopMoving();
            return;
        }

        Tile currentTile = tileGO.GetComponent<Tile>();
        if (currentTile == null || !currentTile.IsWalkable)
        {
            StopMoving();
            return;
        }

        // If current tile is a node, check forward first; only if forward blocked consider left/right
        if (currentTile.IsNode)
        {
            bool aheadOk = CheckWalkableAt(curX + direction.x, curZ + direction.y);
            if (aheadOk)
            {
                // continue straight
                return;
            }

            Vector2Int left = new Vector2Int(-direction.y, direction.x);   // rotate left
            Vector2Int right = new Vector2Int(direction.y, -direction.x); // rotate right

            bool leftOk = CheckWalkableAt(curX + left.x, curZ + left.y);
            bool rightOk = CheckWalkableAt(curX + right.x, curZ + right.y);

            // Preference: left, then right
            if (leftOk)
            {
                direction = left;
                return;
            }
            else if (rightOk)
            {
                direction = right;
                return;
            }
            else
            {
                // no adjacent walkable tiles -> stop
                StopMoving();
                return;
            }
        }
        else
        {
            // Not a node: ensure forward tile is walkable; otherwise stop
            bool aheadOk = CheckWalkableAt(curX + direction.x, curZ + direction.y);
            if (!aheadOk)
            {
                StopMoving();
                return;
            }
        }
    }

    private bool CheckWalkableAt(int x, int z)
    {
        GameObject tileGO = GameObject.Find($"Tile {x} {z}");
        if (tileGO == null) return false;
        Tile t = tileGO.GetComponent<Tile>();
        return t != null && t.IsWalkable;
    }

    private void StopMoving()
    {
        isMoving = false;
        if (animator != null) animator.SetFloat(speedParameter, 0f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isMoving = false;
        if (animator != null) animator.SetFloat(speedParameter, 0f);
        Destroy(gameObject);
    }
}
