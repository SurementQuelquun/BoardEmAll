using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int startingHealth = 10;

    private static int health;
    public static int Health => health;
    public static event Action<int> OnHealthChanged;

    // New event fired once when health reaches zero
    public static event Action OnGameOver;

    private void Awake()
    {
        // Initialize static health from the inspector value when the scene starts.
        health = startingHealth;
        OnHealthChanged?.Invoke(health);
    }

    public static void RemovePoints(int points)
    {
        //if (health == 0)
        //    return;

        health -= points;
        if (health <= 0)
        {
            health = 0;
            OnHealthChanged?.Invoke(health);
            // Fire GameOver only once
            OnGameOver?.Invoke();
            return;
        }

        OnHealthChanged?.Invoke(health);
    }
}