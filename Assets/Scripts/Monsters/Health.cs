using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    private float maxHealth = 100f;
    private float currentHealth;

    // Reference to the Monster instance on the same GameObject
    private Monster _monster;

    private void Awake()
    {
        // Initialisation des PV
        currentHealth = maxHealth;

        // Cache Monster component to access instance fields like 'loot'
        _monster = GetComponent<Monster>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Vérifier si l'entité est morte
        if (currentHealth <= 0f)
        {
            int lootAmount = (_monster != null) ? _monster.loot : 0;
            CoinsManager.GainCoins(lootAmount);
            Die();
        }
    }
    private void Die()
    {
        // Pour l’instant, on détruit simplement le GameObject
        Destroy(gameObject);
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}

