using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    private float maxHealth = 100f;
    private float currentHealth;

    private void Awake()
    {
        // Initialisation des PV
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Vérifier si l'entité est morte
        if (currentHealth <= 0f)
        {
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

