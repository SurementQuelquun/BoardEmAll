using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int startingHealth = 10;

    private static int health;
    public static int Health => health;
    public static event Action<int> OnHealthChanged;

    private void Awake()
    {
        // Initialize static score from the inspector value when the scene starts.
        health = startingHealth;
        OnHealthChanged?.Invoke(health);
    }

    public static void RemovePoints(int points)
    {
        if (health == 0)
            return;
        health -= points;
        OnHealthChanged?.Invoke(health);
    }
}