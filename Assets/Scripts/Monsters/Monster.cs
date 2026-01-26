using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Movement")]
    // Grid step direction: X,Z. (0,1) moves +Z, (1,0) moves +X, etc.
    public Vector2Int direction = new Vector2Int(0, 1);
    public float speed = 2f;

    // If true the monster will start walking automatically on Start()
    public bool startMoving = true;

    void Start()
    {
        currentHealth = maxHealth;

        if (startMoving)
            StartCoroutine(WalkStraight());
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
        Destroy(gameObject);
    }

    // Walk in a straight grid-aligned line. Stop when next tile is missing or not walkable.
    private IEnumerator WalkStraight()
    {
        // Small tolerance for arrival
        const float arriveEpsilon = 0.01f;

        while (true)
        {
            // Compute current grid coordinate (assumes tiles are placed at integer X,Z coordinates).
            int curX = Mathf.RoundToInt(transform.position.x);
            int curZ = Mathf.RoundToInt(transform.position.z);

            int nextX = curX + direction.x;
            int nextZ = curZ + direction.y;

            // Tile names are generated as "Tile {x} {z}" in GridManager.
            string nextTileName = $"Tile {nextX} {nextZ}";
            GameObject nextTileGO = GameObject.Find(nextTileName);

            // Stop if tile missing
            if (nextTileGO == null) yield break;

            Tile nextTile = nextTileGO.GetComponent<Tile>();

            // Stop if tile not walkable or no Tile component found
            if (nextTile == null || !nextTile.IsWalkable) yield break;

            // Move to center of next tile (keep current y)
            Vector3 target = new Vector3(nextX, transform.position.y, nextZ);

            while (Vector3.Distance(transform.position, target) > arriveEpsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            // Ensure exact alignment
            transform.position = target;

            // Continue to next iteration (will not return to previous tile because direction is constant)
            yield return null;
        }
    }
}
