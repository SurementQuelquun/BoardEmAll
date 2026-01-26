using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;    // units per second along +X
    public float startDelay = 2f;   // seconds before starting to move

    [Header("Animation")]
    public Animator animator;               // assign in inspector
    public string speedParameter = "Speed"; // animation parameter name

    [Header("Options")]
    public bool snapToGridOnStart = true; // snap to integer grid at start

    bool isMoving = false;

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

        isMoving = true;
    }

    void Update()
    {
        // Update animator when not null
        if (animator != null)
        {
            animator.SetFloat(speedParameter, isMoving ? Mathf.Abs(moveSpeed) : 0f);
        }

        if (isMoving && currentHealth > 0f)
        {
            // Move continuously along +X while alive.
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);

            // Detect tile we're currently on (assumes tiles named "Tile {x} {z}" at integer coordinates)
            int curX = Mathf.RoundToInt(transform.position.x);
            int curZ = Mathf.RoundToInt(transform.position.z);
            string tileName = $"Tile {curX} {curZ}";
            GameObject tileGO = GameObject.Find(tileName);

            // If tile missing or not walkable -> stop moving and notify animator
            if (tileGO == null)
            {
                isMoving = false;
                if (animator != null) animator.SetFloat(speedParameter, 0f);
            }
            else
            {
                Tile tile = tileGO.GetComponent<Tile>();
                if (tile == null || !tile.IsWalkable)
                {
                    isMoving = false;
                    if (animator != null) animator.SetFloat(speedParameter, 0f);
                }
            }
        }
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
