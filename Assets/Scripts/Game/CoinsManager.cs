using System;
using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    [Header("Player Coins")]
    [SerializeField] private int startingCoins = 0;

    private static int coins;
    public static int Coins => coins;
    public static event Action<int> OnCoinsChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        coins = startingCoins;
        OnCoinsChanged?.Invoke(coins);
    }

    public static void GainCoins(int loot)
    {
        coins += loot;
        OnCoinsChanged?.Invoke(coins);
    }
    public static void SpendCoins(int spent)
    {
        coins -= spent;
        if(coins < 0) coins = 0;
        OnCoinsChanged?.Invoke(coins);
    }

}
