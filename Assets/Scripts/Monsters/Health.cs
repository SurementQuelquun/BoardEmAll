using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    private void Awake()
    {
        // Initialisation des PV
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Appliquer des dégâts à cette entité
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Vérifier si l'entité est morte
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Logique de destruction de l'entité
    /// </summary>
    private void Die()
    {
        // Pour l’instant, on détruit simplement le GameObject
        Destroy(gameObject);
    }

    /// <summary>
    /// Optionnel : récupérer les PV restants
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}

